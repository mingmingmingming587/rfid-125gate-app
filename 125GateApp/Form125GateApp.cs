using _125GateApp.Models;
using _125GateApp.Pages;
using _125GateApp.Services;
using _125GateApp.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace _125GateApp
{
    public partial class Form125GateApp : Form
    {
        private GateProtocolService _protocol;
        private AppSettings _settings;

        private RegisterPage _registerPage;
        private GateMonitorPage _monitorPage;
        private CardManagementPage _managePage;
        private AccessLogsPage _logsPage;
        private DeviceDebugPage _debugPage;

        private bool _protocolExpanded = true;

        public Form125GateApp()
        {
            InitializeComponent();
        }

        private void Form125GateApp_Load(object sender, EventArgs e)
        {
            JsonFormat.EnsureInitialized();
            _settings = JsonFormat.LoadSettings();
            _protocol = new GateProtocolService();
            _protocol.IsSimulationMode = _settings.SimulationMode;

            WireProtocolEvents();
            InitializePages();
            InitializeHardwareOrSimulation();
            UpdateHeaderStatus();
        }

        private void InitializePages()
        {
            _registerPage = new RegisterPage(_protocol);
            _monitorPage = new GateMonitorPage(_protocol, _settings);
            _managePage = new CardManagementPage();
            _logsPage = new AccessLogsPage();
            _debugPage = new DeviceDebugPage(_protocol, _settings);

            tabPageRegister.Controls.Add(_registerPage);
            tabPageMonitor.Controls.Add(_monitorPage);
            tabPageManage.Controls.Add(_managePage);
            tabPageLogs.Controls.Add(_logsPage);
            tabPageDebug.Controls.Add(_debugPage);

            _registerPage.CardsChanged += delegate
            {
                _managePage.RefreshGrid();
                _monitorPage.RefreshSimulationDropdown();
            };

            _managePage.CardsChanged += delegate
            {
                _registerPage.RefreshRecentList();
                _monitorPage.RefreshSimulationDropdown();
            };

            _monitorPage.LogsUpdated += delegate
            {
                _logsPage.RefreshData();
            };

            _monitorPage.RequestEnableSimulation += delegate
            {
                SetSimulationMode(true);
            };

            _debugPage.ModeOrConnectionChanged += delegate
            {
                UpdateHeaderStatus();
            };

            _debugPage.SettingsUpdated += delegate
            {
                _monitorPage.SyncSettings();
                UpdateHeaderStatus();
            };

            _registerPage.StatusMessage += delegate(object s, string msg) { lblAppSub.Text = "状态通知：" + msg; };
            _monitorPage.StatusMessage += delegate(object s, string msg) { lblAppSub.Text = "门禁动态：" + msg; };
            _managePage.StatusMessage += delegate(object s, string msg) { lblAppSub.Text = "卡务管理：" + msg; };
            _logsPage.StatusMessage += delegate(object s, string msg) { lblAppSub.Text = "日志统计：" + msg; };
            _debugPage.StatusMessage += delegate(object s, string msg) { lblAppSub.Text = "设备调试：" + msg; };
        }

        private void WireProtocolEvents()
        {
            _protocol.FrameSent += delegate(object s, ProtocolFrameEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    AppendProtocolLine(tbSend, string.Format("[{0}] {1} -> {2}", DateTime.Now.ToString("HH:mm:ss"), args.Description, args.HexText));
                });
            };

            _protocol.FrameReceived += delegate(object s, ProtocolFrameEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    AppendProtocolLine(tbMsg, string.Format("[{0}] {1} <- {2}", DateTime.Now.ToString("HH:mm:ss"), args.Description, args.HexText));
                });
            };

            _protocol.CardReadCompleted += delegate(object s, CardReadEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    if (args.Source == CardReadSource.RegisterTab)
                    {
                        _registerPage.HandleCardRead(args);
                    }
                    else if (args.Source == CardReadSource.DebugTab)
                    {
                        _debugPage.HandleDebugCardRead(args);
                    }
                    else
                    {
                        _monitorPage.HandleCardRead(args);
                    }
                });
            };

            _protocol.MotorActionCompleted += delegate(object s, MotorResultEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    _monitorPage.HandleMotorAck(args);
                    _debugPage.HandleMotorOrAlarmResult(args.Message, args.Success);
                });
            };

            _protocol.AlarmActionCompleted += delegate(object s, AlarmResultEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    _monitorPage.HandleAlarmAck(args);
                    _debugPage.HandleMotorOrAlarmResult(args.Message, args.Success);
                });
            };

            _protocol.DeviceCommandCompleted += delegate(object s, DeviceCommandResultEventArgs args)
            {
                SafeUiInvoke(delegate
                {
                    _debugPage.HandleDeviceCommandResult(args);
                });
            };

            _protocol.ConnectionStatusChanged += delegate
            {
                SafeUiInvoke(UpdateHeaderStatus);
            };
        }

        private void InitializeHardwareOrSimulation()
        {
            if (_protocol.IsSimulationMode)
            {
                _debugPage.SyncSimulationModeFromHost();
                return;
            }

            ComConfig cfg = JsonFormat.LoadComConfig();
            string err;
            if (!_protocol.OpenPort(cfg.COM, cfg.BAUD, out err))
            {
                lblAppSub.Text = "提示：未连接到 " + cfg.COM + "，可点击右上角【切换仿真模式】进行无硬件实验演示";
            }
            _debugPage.LoadCurrentSettingsToUI();
        }

        private void SetSimulationMode(bool enable)
        {
            _protocol.IsSimulationMode = enable;
            _settings.SimulationMode = enable;
            JsonFormat.SaveSettings(_settings);
            _debugPage.SyncSimulationModeFromHost();
            UpdateHeaderStatus();
        }

        private void UpdateHeaderStatus()
        {
            if (_protocol == null) return;

            if (_protocol.IsSimulationMode)
            {
                lblConnBadge.Text = "当前模式：无硬件仿真测试";
                lblConnBadge.BackColor = Color.FromArgb(67, 56, 202);
                lblConnBadge.ForeColor = Color.White;
                btnQuickSimToggle.Text = "切回实机串口";
                btnQuickSimToggle.BackColor = CampusTheme.SuccessGreen;
            }
            else if (_protocol.IsPortOpen)
            {
                lblConnBadge.Text = string.Format("实机在线：{0} ({1})", _protocol.CurrentPortName, _protocol.CurrentBaudRate);
                lblConnBadge.BackColor = CampusTheme.SuccessGreen;
                lblConnBadge.ForeColor = Color.White;
                btnQuickSimToggle.Text = "切换仿真模式";
                btnQuickSimToggle.BackColor = Color.FromArgb(79, 70, 229);
            }
            else
            {
                lblConnBadge.Text = "实机串口未连接";
                lblConnBadge.BackColor = CampusTheme.WarningAmber;
                lblConnBadge.ForeColor = Color.White;
                btnQuickSimToggle.Text = "切换仿真模式";
                btnQuickSimToggle.BackColor = Color.FromArgb(79, 70, 229);
            }
        }

        private void btnQuickSimToggle_Click(object sender, EventArgs e)
        {
            bool nextSim = !_protocol.IsSimulationMode;
            SetSimulationMode(nextSim);

            if (!nextSim && !_protocol.IsPortOpen)
            {
                ComConfig cfg = JsonFormat.LoadComConfig();
                string err;
                if (!_protocol.OpenPort(cfg.COM, cfg.BAUD, out err))
                {
                    lblAppSub.Text = err;
                }
                _debugPage.LoadCurrentSettingsToUI();
            }
        }

        private void btnToggleProtocol_Click(object sender, EventArgs e)
        {
            _protocolExpanded = !_protocolExpanded;
            panel2.Visible = _protocolExpanded;
            btnToggleProtocol.Text = _protocolExpanded ? "折叠协议区" : "展开协议区";
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabPageManage)
            {
                _managePage.RefreshGrid();
            }
            else if (tabMain.SelectedTab == tabPageLogs)
            {
                _logsPage.RefreshData();
            }
            else if (tabMain.SelectedTab == tabPageMonitor)
            {
                _monitorPage.RefreshSimulationDropdown();
                _monitorPage.RefreshRecentLogs();
            }
            else if (tabMain.SelectedTab == tabPageRegister)
            {
                _registerPage.RefreshRecentList();
            }
        }

        private void SafeUiInvoke(MethodInvoker action)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch
            {
            }
        }

        private static void AppendProtocolLine(TextBox tb, string line)
        {
            if (tb.Lines.Length >= 200)
            {
                tb.Clear();
            }
            tb.AppendText(line + Environment.NewLine);
            tb.SelectionStart = tb.TextLength;
            tb.ScrollToCaret();
        }

        private void llSendClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tbSend.Text = string.Empty;
        }

        private void llRecClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tbMsg.Text = string.Empty;
        }

        private void Form125GateApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_monitorPage != null)
            {
                _monitorPage.StopPollingIfRunning();
            }
            if (_protocol != null)
            {
                _protocol.Dispose();
            }
        }
    }
}
