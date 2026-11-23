using Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _125GateApp
{
    public partial class Form125GateApp : Form
    {
        public Form125GateApp()
        {
            InitializeComponent();
            SetupExtraPages();
        }
        private bool isDoor = false;

        private void Form125GateApp_Load(object sender, EventArgs e)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);

            //获取到文件所在目录
            //FileInfo finfo = new FileInfo(s);
            //string path = finfo.DirectoryName;
            //webBrowser1.Navigate(path + "/实验原理.html");
            //webBrowser2.Navigate(path + "/关键代码.html");
            // 串口由设备连接页面显式选择并打开。
        }


        public void Init_Com(string name, int baud)
        {

            if (ComHelper.openPort(name, baud))
            {
                //MessageBox.Show("串口打开成功");
                ComHelper.sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(50);
                //MessageBox.Show("124");//
                int len = ComHelper.sp.BytesToRead;
                if (len > 0)
                {
                    byte[] recbuf = new byte[len];
                    ComHelper.sp.Read(recbuf, 0, len);//读取
                    //updateMsgsBox(Protocol.byteToHexString(recbuf));
                    List<byte[]> list = Protocol.bytesHandle(recbuf, len);
                    foreach (byte[] buf in list)
                    {
                        ProtocolModel p = Protocol.byteToProtocolModel(buf, buf.Length);//格式化转换
                        //判断温度
                        if (p != null)
                        {
                            //判断是否为光线回复信息
                            if (p.OrderCode[0] == Protocol.SingleCard_ACK[0] && p.OrderCode[1] == Protocol.SingleCard_ACK[1])
                            {
                                updateValues(p);
                            }
                            else if (p.OrderCode[0] == Protocol.STEPMOTOR_ACK[0] && p.OrderCode[1] == Protocol.STEPMOTOR_ACK[1])
                            {
                                updateValues(p);
                            }
                            else if (p.OrderCode[0] == Protocol.ACOUSTOALARM_ACK[0] && p.OrderCode[1] == Protocol.ACOUSTOALARM_ACK[1])
                            {
                                updateValues(p);
                            }
                            updateMsgsBox(Protocol.byteToHexString(buf));
                        }
                    }
                }
            }
            catch
            { }

        }


        delegate void upateValue(ProtocolModel p);

        delegate void updateMsgBox(string hexbuf);
        private void updateMsgsBox(string hexbuf)
        {
            tbMsg.Invoke(new updateMsgBox((msg) =>
            {
                if (tbMsg.Lines.GetUpperBound(0) >= 200)
                    tbMsg.Text = "";

                tbMsg.AppendText(msg + "\n");
            }), hexbuf);



        }

        private void updateValues(ProtocolModel p)
        {

            panel1.Invoke(new upateValue((msg) =>
            {
                byte[] content = msg.Content;

                //door
                 if (p.OrderCode[0] == Protocol.STEPMOTOR_ACK[0] && p.OrderCode[1] == Protocol.STEPMOTOR_ACK[1])
                {
                    if (content[2] == 0x01)
                    {

                        lbDoorState.Text = "开门成功";
                    }
                    else if (content[2] == (byte)0xFF)
                    {

                        lbDoorState.Text = "门打开失败，检测硬件传感器";
                    }

                }

                if (p.OrderCode[0] == Protocol.ACOUSTOALARM_ACK[0] && p.OrderCode[1] == Protocol.ACOUSTOALARM_ACK[1])
                {
                    if (content[2] == 0x01)
                    {
                        lbDoorState.Text = "开门失败，该卡未录入";

                    }
                    else if (content[2] == (byte)0xFF)
                    {

                        lbDoorState.Text = "声光报警器打开失败";
                    }
                }


                if (p.OrderCode[0] == Protocol.SingleCard_ACK[0] && p.OrderCode[1] == Protocol.SingleCard_ACK[1])
                {
                    if (isDoor == false)
                    {
                        lbCardZC.Text = Protocol.BytesToString(content);
                    }
                    else
                    {
                        bool isFindNum = false;
                        lbDoorCard.Text = Protocol.BytesToString(content);
                        JArray ja = JsonFormat.ReadSensorJson();
                        foreach (JObject j in ja)
                        {

                            if (j["cardnum"].ToString().Equals(lbDoorCard.Text) && (bool?)j["enabled"] != false)
                            {
                                lbName.Text = j["name"].ToString();
                                lbSex.Text = j["sex"].ToString();
                                isFindNum = true;
                                lbNum.Text = j["stuid"].ToString();
                                break;
                            }

                        }
                        //Thread.Sleep(1000);
                        if (isFindNum) ///成功开门
                        {
                            ControlStepMotor((byte)0x01, 90);
                        }
                        else //失败报警
                        {
                            Random r = new Random();
                            byte comTpye = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
                            byte order = Protocol.getOrder(true, false, false, 2, false);
                            byte[] local = new byte[4];
                            byte[] target = { 0x00, 0x00, 0x02, 0x05 };

                            byte[] contentA = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            byte[] buf = Protocol.sendMsg(comTpye, order, (byte)r.Next(100), local, target, Protocol.ACOUSTOALARM_REQ, contentA);
                            tbSend.AppendText(Protocol.byteToHexString(buf) + "\n");
                            ComHelper.sp.Write(buf, 0, buf.Length);
                        }

                    }
                }
                // lbCardMsg.Text = Protocol.BytesToString(content);


            }), p);

        }

        private void ControlStepMotor(byte func, int angle)
        {
            Random r = new Random();
            byte comTpye = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
            byte order = Protocol.getOrder(true, false, false, 2, false);
            byte[] local = new byte[4];
            byte[] target = { 0x00, 0x00, 0x02, 0x0B };


            byte[] content = new byte[11];
            content[0] = func;
            content[1] = (byte)(angle >> 8);
            content[2] = (byte)angle;
            byte[] buf = Protocol.sendMsg(comTpye, order, (byte)r.Next(100), local, target, Protocol.STEPMOTOR_REQ, content);
            tbSend.AppendText(Protocol.byteToHexString(buf)+"\n");
            ComHelper.sp.Write(buf, 0, buf.Length);
        }


        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            isDoor = tabControl2.SelectedTab == tabPage5;
        }

     

        private void btnZC_Click(object sender, EventArgs e)
        {
            lbZCState.Text = "....";
            if (!string.IsNullOrWhiteSpace(lbCardZC.Text) && lbCardZC.Text != "..." && lbCardZC.Text != "...." && !tbName.Text.Equals("") && !tbNum.Text.Equals("") && cbSex.SelectedIndex != -1)
            {
                if (JsonFormat.AddJson(lbCardZC.Text, tbName.Text, tbNum.Text, cbSex.Text))
                    lbZCState.Text = "注册成功";
                else
                    lbZCState.Text = "注册失败";
            }
            else {

                lbZCState.Text = "先填写信息";
            }
        }

        private void llSendClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tbSend.Text = "";
        }

        private void llRecClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tbMsg.Text = "";
        }

        private void llDoorGetCard_Click(object sender, EventArgs e)
        {
            if (!DeviceReady()) return;
            lbNum.Text = "...";
            lbName.Text = "...";
            lbSex.Text = "...";
            lbDoorState.Text = "...";
            lbDoorCard.Text = "...";
            Random r = new Random();
            byte comTpye = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
            byte order = Protocol.getOrder(true, false, false, 2, false);
            byte[] local = new byte[4];
            byte[] target = { 0x00, 0x00, 0x03, 0x01 };
            byte[] content = { 0x00 };
            byte[] buf = Protocol.sendMsg(comTpye, order, (byte)r.Next(100), local, target, Protocol.SingleCard_REQ, content);
            tbSend.AppendText(Protocol.byteToHexString(buf) + "\n");
            ComHelper.sp.Write(buf, 0, buf.Length);
        }

        private void llZCGetCard_Click(object sender, EventArgs e)
        {
            if (!DeviceReady()) return;
            lbCardZC.Text = "...";
            Random r = new Random();
            byte comTpye = ConsumeType.getType(ConsumeType.RS485, ConsumeType.WIRED);
            byte order = Protocol.getOrder(true, false, false, 2, false);
            byte[] local = new byte[4];
            byte[] target = { 0x00, 0x00, 0x03, 0x01 };
            byte[] content = { 0x00 };
            byte[] buf = Protocol.sendMsg(comTpye, order, (byte)r.Next(100), local, target, Protocol.SingleCard_REQ, content);
            tbSend.AppendText(Protocol.byteToHexString(buf) + "\n");
            ComHelper.sp.Write(buf, 0, buf.Length);
        }

        




    }
}
