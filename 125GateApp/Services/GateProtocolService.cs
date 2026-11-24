using _125GateApp.Models;
using Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace _125GateApp.Services
{
    public delegate void StatusTextEventHandler(object sender, string message);

    public enum CardReadSource
    {
        RegisterTab,
        GateMonitorTab,
        DebugTab
    }

    public class ProtocolFrameEventArgs : EventArgs
    {
        public string HexText { get; set; }
        public string Description { get; set; }
    }

    public class CardReadEventArgs : EventArgs
    {
        public bool CardFound { get; set; }
        public string CardNumber { get; set; }
        public string RawMessage { get; set; }
        public CardReadSource Source { get; set; }
        public bool IsSimulated { get; set; }
    }

    public class MotorResultEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public byte Direction { get; set; }
        public string Message { get; set; }
    }

    public class AlarmResultEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public bool IsOnboardBuzzer { get; set; }
        public string Message { get; set; }
    }

    public class DeviceCommandResultEventArgs : EventArgs
    {
        public string CommandName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 封装 F-Table 125K 阅读器、步进电机、声光报警通信协议与无硬件仿真引擎。
    /// </summary>
    public class GateProtocolService : IDisposable
    {
        private static readonly byte[] TargetReader125K = new byte[] { 0x00, 0x00, 0x03, 0x01 };
        private static readonly byte[] TargetActuator020B = new byte[] { 0x00, 0x00, 0x02, 0x0B };
        private static readonly byte[] TargetAcoustoAlarm = new byte[] { 0x00, 0x00, 0x02, 0x05 };
        private static readonly byte[] LocalHostId = new byte[] { 0x00, 0x00, 0x00, 0x00 };

        private readonly Random _rng = new Random();
        private readonly object _serialLock = new object();
        private readonly Queue<PendingTx> _txQueue = new Queue<PendingTx>();
        private readonly AutoResetEvent _txSignal = new AutoResetEvent(false);
        private readonly Thread _txWorker;
        private DateTime _lastSerialWriteUtc = DateTime.MinValue;
        private const int Rs485InterFrameMs = 80;
        private bool _serialHooked = false;
        private volatile bool _disposed = false;

        private class PendingTx
        {
            public byte[] Frame;
            public string Hex;
            public string Description;
        }

        private CardReadSource _pendingReadSource = CardReadSource.GateMonitorTab;
        private byte _lastMotorDirection = 0x01;
        private int _lastServoTargetValue = 90;
        private bool _lastServoOpening = true;
        private bool _lastAcoustoTurnOn = false;
        private bool _lastRfSwitchTarget = true;

        public bool IsSimulationMode { get; set; }
        public string SimulatedCardNumber { get; set; }
        public bool SimulateNoCard { get; set; }
        public bool SimulateMotorFault { get; set; }

        public bool IsPortOpen
        {
            get
            {
                return ComHelper.sp != null && ComHelper.sp.IsOpen;
            }
        }

        public string CurrentPortName { get; private set; }
        public int CurrentBaudRate { get; private set; }

        public event EventHandler<ProtocolFrameEventArgs> FrameSent;
        public event EventHandler<ProtocolFrameEventArgs> FrameReceived;
        public event EventHandler<CardReadEventArgs> CardReadCompleted;
        public event EventHandler<MotorResultEventArgs> MotorActionCompleted;
        public event EventHandler<AlarmResultEventArgs> AlarmActionCompleted;
        public event EventHandler<DeviceCommandResultEventArgs> DeviceCommandCompleted;
        public event StatusTextEventHandler ConnectionStatusChanged;

        public GateProtocolService()
        {
            CurrentPortName = "COM3";
            CurrentBaudRate = 57600;
            SimulatedCardNumber = "0006849456";
            SimulateNoCard = false;
            SimulateMotorFault = false;

            _txWorker = new Thread(SerialTxWorkerLoop);
            _txWorker.IsBackground = true;
            _txWorker.Name = "RS485TxWorker";
            _txWorker.Start();
        }

        public static string[] GetAvailablePorts()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                Array.Sort(ports, StringComparer.OrdinalIgnoreCase);
                return ports;
            }
            catch
            {
                return new string[0];
            }
        }

        public bool OpenPort(string portName, int baudRate, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(portName))
            {
                errorMessage = "请选择有效串口号。";
                return false;
            }

            try
            {
                ClosePort();

                bool opened = ComHelper.openPort(portName, baudRate);
                if (opened && ComHelper.sp != null && ComHelper.sp.IsOpen)
                {
                    HookSerialDataReceived();
                    CurrentPortName = portName;
                    CurrentBaudRate = baudRate;
                    RaiseConnectionChanged(string.Format("串口已连接: {0} ({1} bps)", portName, baudRate));
                    return true;
                }

                errorMessage = string.Format("串口 {0} 打开失败，请检查设备连接或端口是否被占用。", portName);
                RaiseConnectionChanged(string.Format("串口未连接 ({0} 打开失败)", portName));
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = string.Format("打开串口 {0} 异常: {1}", portName, ex.Message);
                RaiseConnectionChanged("串口未连接");
                return false;
            }
        }

        public void ClosePort()
        {
            try
            {
                if (ComHelper.sp != null && _serialHooked)
                {
                    ComHelper.sp.DataReceived -= OnSerialDataReceived;
                    _serialHooked = false;
                }
                ComHelper.closePort();
            }
            catch
            {
                _serialHooked = false;
            }
            RaiseConnectionChanged("串口已断开");
        }

        private void HookSerialDataReceived()
        {
            if (ComHelper.sp != null)
            {
                ComHelper.sp.DataReceived -= OnSerialDataReceived;
                ComHelper.sp.DataReceived += OnSerialDataReceived;
                _serialHooked = true;
            }
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(50);
                if (ComHelper.sp == null || !ComHelper.sp.IsOpen)
                {
                    return;
                }
                int len = ComHelper.sp.BytesToRead;
                if (len <= 0)
                {
                    return;
                }
                byte[] recbuf = new byte[len];
                ComHelper.sp.Read(recbuf, 0, len);
                ProcessIncomingBuffer(recbuf, len);
            }
            catch
            {
            }
        }

        private void ProcessIncomingBuffer(byte[] rawBuffer, int len)
        {
            List<byte[]> frames = Protocol.bytesHandle(rawBuffer, len);
            if (frames == null)
            {
                return;
            }

            foreach (byte[] frame in frames)
            {
                ProtocolModel model = Protocol.byteToProtocolModel(frame, frame.Length);
                if (model != null)
                {
                    string hex = Protocol.byteToHexString(frame);
                    string desc = DescribeOrderCode(model.OrderCode, false);
                    RaiseFrameReceived(hex, desc);
                    DispatchProtocolModel(model);
                }
            }
        }

        private void DispatchProtocolModel(ProtocolModel p)
        {
            if (p == null || p.OrderCode == null || p.OrderCode.Length < 2)
            {
                return;
            }

            byte[] content = p.Content ?? new byte[0];

            if (MatchesCode(p.OrderCode, Protocol.SingleCard_ACK))
            {
                HandleSingleCardAck(content);
            }
            else if (MatchesCode(p.OrderCode, Protocol.STEPMOTOR_ACK))
            {
                bool ok = content.Length >= 3 && content[2] == 0x01;
                string msg = ok
                    ? (_lastMotorDirection == 0x01 ? "步进电机正转开闸成功" : "步进电机反转关闸成功")
                    : "步进电机执行失败，请检查执行器连接";
                if (MotorActionCompleted != null)
                {
                    MotorActionCompleted(this, new MotorResultEventArgs
                    {
                        Success = ok,
                        Direction = _lastMotorDirection,
                        Message = msg
                    });
                }
            }
            else if (MatchesCode(p.OrderCode, Protocol.STEERING_ACK))
            {
                bool ok = content.Length >= 3 && content[2] == 0x01;
                string msg = ok
                    ? string.Format(_lastServoOpening ? "舵机开闸到位（目标 {0}）" : "舵机关闸复位（目标 {0}）", _lastServoTargetValue)
                    : "舵机执行失败，请检查020B舵机连接";
                if (MotorActionCompleted != null)
                {
                    MotorActionCompleted(this, new MotorResultEventArgs
                    {
                        Success = ok,
                        Direction = _lastServoOpening ? (byte)0x01 : (byte)0x02,
                        Message = msg
                    });
                }
            }
            else if (MatchesCode(p.OrderCode, Protocol.ACOUSTOALARM_ACK))
            {
                bool ok = content.Length >= 3 && content[2] == 0x01;
                string msg = ok
                    ? (_lastAcoustoTurnOn ? "声光报警器已开启" : "声光报警器已关闭")
                    : "声光报警器响应失败";
                if (AlarmActionCompleted != null)
                {
                    AlarmActionCompleted(this, new AlarmResultEventArgs
                    {
                        Success = ok,
                        IsOnboardBuzzer = false,
                        Message = msg
                    });
                }
            }
            else if (MatchesCode(p.OrderCode, Protocol.BuzzerControl_ACK))
            {
                bool ok = content.Length >= 3 && content[2] == 0x01;
                if (AlarmActionCompleted != null)
                {
                    AlarmActionCompleted(this, new AlarmResultEventArgs
                    {
                        Success = ok,
                        IsOnboardBuzzer = true,
                        Message = ok ? "125K板载蜂鸣器已响应" : "125K板载蜂鸣器响应失败"
                    });
                }
            }
            else if (MatchesCode(p.OrderCode, Protocol.COMTEST_ACK))
            {
                bool ok = content.Length >= 1 && content[0] == 0x01;
                RaiseDeviceCommand("串口通信测试", ok, ok ? "125K阅读器串口通信正常" : "串口测试返回异常");
            }
            else if (MatchesCode(p.OrderCode, Protocol.RFIDSWITCH_ACK))
            {
                bool ok = content.Length >= 1 && content[0] == 0x01;
                string msg = ok
                    ? (_lastRfSwitchTarget ? "125K射频天线已开启" : "125K射频天线已关闭")
                    : "射频开关操作失败";
                RaiseDeviceCommand("射频开关控制", ok, msg);
            }
            else if (MatchesCode(p.OrderCode, Protocol.NewCard_ACK))
            {
                bool ok = content.Length >= 1 && content[0] == 0x01;
                RaiseDeviceCommand("T5557新卡制作", ok, ok ? "T5557写入EM4100卡号成功，可读取验证" : "T5557写入失败，请确认卡在感应区且已解密初始化");
            }
        }

        private void HandleSingleCardAck(byte[] content)
        {
            // 修复原工程 Bug：下位机无卡时返回 1 字节 0xFF，原代码会解析为乱码 "" 并误触发报警
            if (content == null || content.Length == 0 || (content.Length == 1 && content[0] == 0xFF))
            {
                if (CardReadCompleted != null)
                {
                    CardReadCompleted(this, new CardReadEventArgs
                    {
                        CardFound = false,
                        CardNumber = string.Empty,
                        RawMessage = "未检测到125K卡片",
                        Source = _pendingReadSource,
                        IsSimulated = IsSimulationMode
                    });
                }
                return;
            }

            string cardStr = Protocol.BytesToString(content);
            if (cardStr != null)
            {
                cardStr = cardStr.Trim('\0', ' ', '\r', '\n', '\t');
            }
            else
            {
                cardStr = string.Empty;
            }

            if (!IsValidCardNumber(cardStr))
            {
                if (CardReadCompleted != null)
                {
                    CardReadCompleted(this, new CardReadEventArgs
                    {
                        CardFound = false,
                        CardNumber = string.Empty,
                        RawMessage = string.Format("读卡数据异常: {0}", cardStr),
                        Source = _pendingReadSource,
                        IsSimulated = IsSimulationMode
                    });
                }
                return;
            }

            if (CardReadCompleted != null)
            {
                CardReadCompleted(this, new CardReadEventArgs
                {
                    CardFound = true,
                    CardNumber = cardStr,
                    RawMessage = cardStr,
                    Source = _pendingReadSource,
                    IsSimulated = IsSimulationMode
                });
            }
        }

        public static bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                return false;
            }
            return Regex.IsMatch(cardNumber.Trim(), @"^\d{6,12}$");
        }

        public bool RequestSingleCard(CardReadSource source, out string errorMsg)
        {
            _pendingReadSource = source;
            byte[] content = new byte[] { 0x00 };
            return SendCommand(TargetReader125K, Protocol.SingleCard_REQ, content, "单次寻卡请求", out errorMsg);
        }

        public bool TestReaderCom(out string errorMsg)
        {
            byte[] content = new byte[] { 0x00 };
            return SendCommand(TargetReader125K, Protocol.COMTEST_REQ, content, "串口通信测试", out errorMsg);
        }

        public bool SetRfSwitch(bool turnOn, out string errorMsg)
        {
            _lastRfSwitchTarget = turnOn;
            byte[] content = new byte[] { turnOn ? (byte)0x01 : (byte)0x00 };
            return SendCommand(TargetReader125K, Protocol.RFIDSWITCH_REQ, content, turnOn ? "开启射频场" : "关闭射频场", out errorMsg);
        }

        public bool ControlStepMotor(byte direction, int angle, out string errorMsg)
        {
            _lastMotorDirection = direction;
            if (angle <= 0) angle = 90;
            if (angle > 720) angle = 720;

            byte[] content = new byte[11];
            content[0] = direction;
            content[1] = (byte)(angle >> 8);
            content[2] = (byte)angle;

            string desc = string.Format("步进电机{0} {1}°", direction == 0x01 ? "正转开闸" : "反转关闸", angle);
            return SendCommand(TargetActuator020B, Protocol.STEPMOTOR_REQ, content, desc, out errorMsg);
        }

        public bool ControlServo(int angleOrPulse, bool isOpening, out string errorMsg)
        {
            if (angleOrPulse < 0) angleOrPulse = 0;
            if (angleOrPulse > 200) angleOrPulse = 200;

            _lastServoTargetValue = angleOrPulse;
            _lastServoOpening = isOpening;

            byte[] content = new byte[9];
            content[0] = (byte)angleOrPulse;

            string desc = string.Format("舵机{0} ({1})", isOpening ? "开闸" : "关闸复位", angleOrPulse);
            return SendCommand(TargetActuator020B, Protocol.STEERING_REQ, content, desc, out errorMsg);
        }

        public bool ControlAcoustoAlarm(bool turnOn, out string errorMsg)
        {
            // 官方 AcoustoAlarm.dll 协议约定：0x01 开启报警，0x02 关闭报警（发 0x00 硬件不停止）
            _lastAcoustoTurnOn = turnOn;
            byte[] content = new byte[]
            {
                turnOn ? (byte)0x01 : (byte)0x02,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            return SendCommand(TargetAcoustoAlarm, Protocol.ACOUSTOALARM_REQ, content, turnOn ? "开启声光报警器" : "关闭声光报警器", out errorMsg);
        }

        public bool ControlOnboardBuzzer(bool turnOn, out string errorMsg)
        {
            byte[] content = new byte[]
            {
                turnOn ? (byte)0x01 : (byte)0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            return SendCommand(TargetReader125K, Protocol.BuzzerControl_REQ, content, turnOn ? "开启125K板载蜂鸣器" : "关闭125K板载蜂鸣器", out errorMsg);
        }

        public bool WriteT5557NewCard(uint cardId, byte vendorCode, out string errorMsg)
        {
            byte[] payload = EncodeEm4100Payload(cardId, vendorCode);
            if (IsSimulationMode)
            {
                SimulatedCardNumber = cardId.ToString("D10");
                SimulateNoCard = false;
            }
            return SendCommand(TargetReader125K, Protocol.NewCard_REQ, payload, string.Format("T5557制作EM4100卡 ({0})", cardId.ToString("D10")), out errorMsg);
        }

        /// <summary>
        /// 按照 EM4100 64-bit 帧规则（9位1起始头 + 10行4bit数据与行偶校验 + 4位列偶校验 + 1位0停止位）生成 T5557 块1/块2配置数据。
        /// </summary>
        public static byte[] EncodeEm4100Payload(uint cardId, byte vendorCode)
        {
            int[] bits = new int[64];
            int idx = 0;

            // 9-bit header: 111111111
            for (int i = 0; i < 9; i++)
            {
                bits[idx++] = 1;
            }

            int[] colParity = new int[4];

            // 10 nibbles: 2 nibbles for vendorCode, 8 nibbles for 32-bit cardId
            for (int row = 0; row < 10; row++)
            {
                int nibble;
                if (row == 0)
                {
                    nibble = (vendorCode >> 4) & 0x0F;
                }
                else if (row == 1)
                {
                    nibble = vendorCode & 0x0F;
                }
                else
                {
                    int shift = (9 - row) * 4;
                    nibble = (int)((cardId >> shift) & 0x0F);
                }

                int rowSum = 0;
                for (int col = 0; col < 4; col++)
                {
                    int bit = (nibble >> (3 - col)) & 0x01;
                    bits[idx++] = bit;
                    rowSum += bit;
                    colParity[col] ^= bit;
                }
                bits[idx++] = rowSum & 0x01;
            }

            // Column even parity + stop bit 0
            for (int col = 0; col < 4; col++)
            {
                bits[idx++] = colParity[col] & 0x01;
            }
            bits[idx++] = 0;

            byte[] result = new byte[8];
            for (int b = 0; b < 8; b++)
            {
                byte val = 0;
                for (int k = 0; k < 8; k++)
                {
                    val = (byte)((val << 1) | bits[b * 8 + k]);
                }
                result[b] = val;
            }
            return result;
        }

        private bool SendCommand(byte[] targetId, byte[] orderCode, byte[] content, string description, out string errorMsg)
        {
            errorMsg = string.Empty;
            byte comType = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
            byte order = Protocol.getOrder(true, false, false, 2, false);
            byte randomByte;
            lock (_serialLock)
            {
                randomByte = (byte)_rng.Next(1, 250);
            }

            byte[] frame = Protocol.sendMsg(comType, order, randomByte, LocalHostId, targetId, orderCode, content);
            string hex = Protocol.byteToHexString(frame);

            if (IsSimulationMode)
            {
                RaiseFrameSent(hex, description + " [仿真]");
                SimulateDeviceResponse(targetId, orderCode, randomByte, content);
                return true;
            }

            if (!IsPortOpen)
            {
                errorMsg = "串口未连接！请前往【设备调试】页打开串口，或开启【无硬件仿真模式】。";
                return false;
            }

            try
            {
                lock (_serialLock)
                {
                    _txQueue.Enqueue(new PendingTx
                    {
                        Frame = frame,
                        Hex = hex,
                        Description = description
                    });
                }
                _txSignal.Set();
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = string.Format("发送串口指令失败: {0}", ex.Message);
                return false;
            }
        }

        private void SerialTxWorkerLoop()
        {
            while (!_disposed)
            {
                PendingTx next = null;
                lock (_serialLock)
                {
                    if (_txQueue.Count > 0)
                    {
                        next = _txQueue.Dequeue();
                    }
                }

                if (next == null)
                {
                    _txSignal.WaitOne(100);
                    continue;
                }

                // 保证 RS485 半双工总线上连续两帧至少间隔 80ms，防止从机回传 ACK 与下一帧指令碰撞丢包
                double elapsedMs = (DateTime.UtcNow - _lastSerialWriteUtc).TotalMilliseconds;
                if (elapsedMs < Rs485InterFrameMs)
                {
                    int waitMs = (int)Math.Ceiling(Rs485InterFrameMs - elapsedMs);
                    if (waitMs > 0)
                    {
                        Thread.Sleep(waitMs);
                    }
                }

                try
                {
                    if (ComHelper.sp != null && ComHelper.sp.IsOpen)
                    {
                        ComHelper.sp.Write(next.Frame, 0, next.Frame.Length);
                        _lastSerialWriteUtc = DateTime.UtcNow;
                        RaiseFrameSent(next.Hex, next.Description);
                    }
                }
                catch
                {
                }
            }
        }

        private void SimulateDeviceResponse(byte[] targetId, byte[] reqOrderCode, byte randomByte, byte[] reqContent)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(60);
                if (_disposed) return;

                byte comType = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
                byte ackOrder = Protocol.getOrder(false, false, false, 2, false);
                byte[] ackCode = null;
                byte[] ackContent = null;

                if (MatchesCode(reqOrderCode, Protocol.SingleCard_REQ))
                {
                    ackCode = Protocol.SingleCard_ACK;
                    if (SimulateNoCard || string.IsNullOrEmpty(SimulatedCardNumber))
                    {
                        ackContent = new byte[] { 0xFF };
                    }
                    else
                    {
                        ackContent = Encoding.ASCII.GetBytes(SimulatedCardNumber.Trim());
                    }
                }
                else if (MatchesCode(reqOrderCode, Protocol.STEPMOTOR_REQ))
                {
                    ackCode = Protocol.STEPMOTOR_ACK;
                    byte status = SimulateMotorFault ? (byte)0xFF : (byte)0x01;
                    ackContent = new byte[] { status, 0x00, status, 0, 0, 0, 0, 0, 0, 0, 0 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.STEERING_REQ))
                {
                    ackCode = Protocol.STEERING_ACK;
                    byte status = SimulateMotorFault ? (byte)0xFF : (byte)0x01;
                    ackContent = new byte[] { status, 0x00, status, 0, 0, 0, 0, 0, 0, 0, 0 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.ACOUSTOALARM_REQ))
                {
                    ackCode = Protocol.ACOUSTOALARM_ACK;
                    ackContent = new byte[] { 0x01, 0x00, 0x01, 0, 0, 0, 0, 0, 0, 0, 0 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.BuzzerControl_REQ))
                {
                    ackCode = Protocol.BuzzerControl_ACK;
                    ackContent = new byte[] { 0x01, 0x00, 0x01, 0, 0, 0, 0, 0, 0, 0, 0 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.COMTEST_REQ))
                {
                    ackCode = Protocol.COMTEST_ACK;
                    ackContent = new byte[] { 0x01 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.RFIDSWITCH_REQ))
                {
                    ackCode = Protocol.RFIDSWITCH_ACK;
                    ackContent = new byte[] { 0x01 };
                }
                else if (MatchesCode(reqOrderCode, Protocol.NewCard_REQ))
                {
                    ackCode = Protocol.NewCard_ACK;
                    ackContent = new byte[] { 0x01 };
                }

                if (ackCode != null && ackContent != null)
                {
                    byte[] ackFrame = Protocol.sendMsg(comType, ackOrder, randomByte, targetId, LocalHostId, ackCode, ackContent);
                    ProcessIncomingBuffer(ackFrame, ackFrame.Length);
                }
            });
        }

        private static bool MatchesCode(byte[] a, byte[] b)
        {
            return a != null && b != null && a.Length >= 2 && b.Length >= 2 && a[0] == b[0] && a[1] == b[1];
        }

        private static string DescribeOrderCode(byte[] code, bool isReq)
        {
            if (code == null || code.Length < 2) return "未知指令";
            if (MatchesCode(code, Protocol.SingleCard_ACK)) return "寻卡响应";
            if (MatchesCode(code, Protocol.STEPMOTOR_ACK)) return "步进电机应答";
            if (MatchesCode(code, Protocol.STEERING_ACK)) return "舵机应答";
            if (MatchesCode(code, Protocol.ACOUSTOALARM_ACK)) return "声光报警器应答";
            if (MatchesCode(code, Protocol.BuzzerControl_ACK)) return "板载蜂鸣器应答";
            if (MatchesCode(code, Protocol.COMTEST_ACK)) return "串口测试应答";
            if (MatchesCode(code, Protocol.RFIDSWITCH_ACK)) return "射频开关应答";
            if (MatchesCode(code, Protocol.NewCard_ACK)) return "T5557制卡应答";
            return string.Format("指令 {0:X2} {1:X2}", code[0], code[1]);
        }

        private void RaiseFrameSent(string hex, string desc)
        {
            if (FrameSent != null)
            {
                FrameSent(this, new ProtocolFrameEventArgs { HexText = hex, Description = desc });
            }
        }

        private void RaiseFrameReceived(string hex, string desc)
        {
            if (FrameReceived != null)
            {
                FrameReceived(this, new ProtocolFrameEventArgs { HexText = hex, Description = desc });
            }
        }

        private void RaiseDeviceCommand(string cmdName, bool success, string msg)
        {
            if (DeviceCommandCompleted != null)
            {
                DeviceCommandCompleted(this, new DeviceCommandResultEventArgs
                {
                    CommandName = cmdName,
                    Success = success,
                    Message = msg
                });
            }
        }

        private void RaiseConnectionChanged(string status)
        {
            if (ConnectionStatusChanged != null)
            {
                ConnectionStatusChanged(this, status);
            }
        }

        public void Dispose()
        {
            // 等待少量未发完的关报警等收尾帧发出后再关闭串口
            for (int i = 0; i < 6; i++)
            {
                int remaining;
                lock (_serialLock)
                {
                    remaining = _txQueue.Count;
                }
                if (remaining == 0) break;
                Thread.Sleep(60);
            }
            _disposed = true;
            _txSignal.Set();
            ClosePort();
        }
    }
}
