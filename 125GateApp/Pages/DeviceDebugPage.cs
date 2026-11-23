using _125GateApp.Models;
using _125GateApp.Services;
using _125GateApp.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace _125GateApp.Pages
{
    public class DeviceDebugPage : UserControl
    {
        private readonly GateProtocolService _protocol;
        private readonly AppSettings _settings;
        private readonly Random _rng = new Random();

        private RadioButton rbRealSerial;
        private RadioButton rbSimMode;
        private CheckBox chkSimNoCard;
        private CheckBox chkSimMotorFault;
        private ComboBox cbPorts;
        private ComboBox cbBaud;
        private Label lblSerialState;

        private Label lblReaderResult;
        private NumericUpDown nudAngle;
        private NumericUpDown nudPollMs;
        private NumericUpDown nudDebounceSec;
        private NumericUpDown nudAutoCloseSec;
        private CheckBox chkUseAcousto;
        private CheckBox chkUseBuzzer;
        private ComboBox cbGateZone;

        private NumericUpDown nudTestAngle;
        private Label lblActuatorResult;

        private TextBox tbT5557CardId;
        private TextBox tbVendorCode;
        private TextBox tbEm4100Hex;
        private Label lblT5557Result;

        public event EventHandler ModeOrConnectionChanged;
        public event EventHandler SettingsUpdated;
        public event StatusTextEventHandler StatusMessage;

        public DeviceDebugPage(GateProtocolService protocol, AppSettings settings)
        {
            _protocol = protocol;
            _settings = settings;
            InitializeLayout();
            LoadCurrentSettingsToUI();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = CampusTheme.PageBg;
            this.Padding = new Padding(10);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.RowCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.Controls.Add(grid);

            grid.Controls.Add(CreateSerialAndModeCard(), 0, 0);
            grid.Controls.Add(CreateGateSettingsCard(), 1, 0);
            grid.Controls.Add(CreateActuatorDebugCard(), 0, 1);
            grid.Controls.Add(CreateT5557CopyCard(), 1, 1);
        }

        private Panel CreateCardContainer(string title, out Panel bodyPanel)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(12);
            card.Margin = new Padding(4);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 24;

            bodyPanel = new Panel();
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.AutoScroll = true;

            card.Controls.Add(bodyPanel);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Panel CreateSerialAndModeCard()
        {
            Panel body;
            Panel card = CreateCardContainer("1. 串口连接与无硬件仿真设置", out body);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Top;
            tbl.AutoSize = true;
            tbl.ColumnCount = 2;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddRowLabel(tbl, 0, "运行模式：");
            FlowLayoutPanel modeRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            rbRealSerial = new RadioButton { Text = "真实串口硬件", AutoSize = true, Font = CampusTheme.BodyFont, Checked = true };
            rbSimMode = new RadioButton { Text = "无硬件仿真测试", AutoSize = true, Font = CampusTheme.BodyBoldFont, ForeColor = Color.FromArgb(79, 70, 229) };
            rbSimMode.CheckedChanged += RbMode_CheckedChanged;
            modeRow.Controls.Add(rbRealSerial);
            modeRow.Controls.Add(rbSimMode);
            tbl.Controls.Add(modeRow, 1, 0);

            AddRowLabel(tbl, 1, "仿真选项：");
            FlowLayoutPanel simOptRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            chkSimNoCard = new CheckBox { Text = "模拟无卡(0xFF)", AutoSize = true, Font = CampusTheme.SmallFont };
            chkSimNoCard.CheckedChanged += delegate { _protocol.SimulateNoCard = chkSimNoCard.Checked; };
            chkSimMotorFault = new CheckBox { Text = "模拟电机故障", AutoSize = true, Font = CampusTheme.SmallFont };
            chkSimMotorFault.CheckedChanged += delegate { _protocol.SimulateMotorFault = chkSimMotorFault.Checked; };
            simOptRow.Controls.Add(chkSimNoCard);
            simOptRow.Controls.Add(chkSimMotorFault);
            tbl.Controls.Add(simOptRow, 1, 1);

            AddRowLabel(tbl, 2, "串口 / 波特：");
            FlowLayoutPanel portRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            cbPorts = new ComboBox { Font = CampusTheme.BodyFont, Width = 86, DropDownStyle = ComboBoxStyle.DropDown };
            Button btnScan = new Button { Text = "刷新", Width = 54, Height = 24 };
            CampusTheme.StyleSecondaryButton(btnScan);
            btnScan.Click += delegate { RefreshPortsList(); };

            cbBaud = new ComboBox { Font = CampusTheme.BodyFont, Width = 84, DropDownStyle = ComboBoxStyle.DropDownList };
            cbBaud.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });

            portRow.Controls.Add(cbPorts);
            portRow.Controls.Add(btnScan);
            portRow.Controls.Add(cbBaud);
            tbl.Controls.Add(portRow, 1, 2);

            AddRowLabel(tbl, 3, "连接控制：");
            FlowLayoutPanel connRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 2) };
            Button btnOpen = new Button { Text = "连接串口", Width = 82, Height = 27 };
            CampusTheme.StylePrimaryButton(btnOpen, CampusTheme.SuccessGreen);
            btnOpen.Click += BtnOpen_Click;

            Button btnClose = new Button { Text = "断开", Width = 62, Height = 27 };
            CampusTheme.StyleSecondaryButton(btnClose);
            btnClose.Click += delegate
            {
                _protocol.ClosePort();
                UpdateConnectionLabel();
                if (ModeOrConnectionChanged != null) ModeOrConnectionChanged(this, EventArgs.Empty);
            };

            Button btnSaveCom = new Button { Text = "保存串口配置", Width = 108, Height = 27 };
            CampusTheme.StylePrimaryButton(btnSaveCom, CampusTheme.NavyAccent);
            btnSaveCom.Click += BtnSaveCom_Click;

            connRow.Controls.Add(btnOpen);
            connRow.Controls.Add(btnClose);
            connRow.Controls.Add(btnSaveCom);
            tbl.Controls.Add(connRow, 1, 3);

            AddRowLabel(tbl, 4, "当前状态：");
            lblSerialState = new Label { Text = "未初始化", Font = CampusTheme.BodyBoldFont, AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            tbl.Controls.Add(lblSerialState, 1, 4);

            body.Controls.Add(tbl);
            return card;
        }

        private Panel CreateGateSettingsCard()
        {
            Panel body;
            Panel card = CreateCardContainer("2. 125K 阅读器检测与门禁参数", out body);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Top;
            tbl.AutoSize = true;
            tbl.ColumnCount = 2;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddRowLabel(tbl, 0, "阅读器测试：");
            FlowLayoutPanel rfRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            Button btnComTest = new Button { Text = "通信测试", Width = 78, Height = 26 };
            CampusTheme.StylePrimaryButton(btnComTest, CampusTheme.NavyAccent);
            btnComTest.Click += delegate { RunSimpleCmd("串口通信测试", _protocol.TestReaderCom); };

            Button btnRfOn = new Button { Text = "开启射频场", Width = 88, Height = 26 };
            CampusTheme.StylePrimaryButton(btnRfOn, CampusTheme.SuccessGreen);
            btnRfOn.Click += delegate { RunRfCmd(true); };

            Button btnRfOff = new Button { Text = "关闭射频场", Width = 88, Height = 26 };
            CampusTheme.StyleSecondaryButton(btnRfOff);
            btnRfOff.Click += delegate { RunRfCmd(false); };

            rfRow.Controls.Add(btnComTest);
            rfRow.Controls.Add(btnRfOn);
            rfRow.Controls.Add(btnRfOff);
            tbl.Controls.Add(rfRow, 1, 0);

            AddRowLabel(tbl, 1, "当前门区：");
            cbGateZone = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = CampusTheme.BodyFont, Width = 175 };
            cbGateZone.Items.AddRange(CampusZones.All);
            tbl.Controls.Add(cbGateZone, 1, 1);

            AddRowLabel(tbl, 2, "角度 / 关门：");
            FlowLayoutPanel timingRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            nudAngle = new NumericUpDown { Minimum = 30, Maximum = 360, Increment = 15, Width = 58, Font = CampusTheme.BodyFont };
            Label l1 = new Label { Text = "°  自动关门:", AutoSize = true, Font = CampusTheme.SmallFont, Margin = new Padding(2, 5, 2, 0) };
            nudAutoCloseSec = new NumericUpDown { Minimum = 2, Maximum = 30, Width = 48, Font = CampusTheme.BodyFont };
            Label l2 = new Label { Text = "秒", AutoSize = true, Font = CampusTheme.SmallFont, Margin = new Padding(2, 5, 2, 0) };
            timingRow.Controls.Add(nudAngle);
            timingRow.Controls.Add(l1);
            timingRow.Controls.Add(nudAutoCloseSec);
            timingRow.Controls.Add(l2);
            tbl.Controls.Add(timingRow, 1, 2);

            AddRowLabel(tbl, 3, "轮询 / 去抖：");
            FlowLayoutPanel pollRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            nudPollMs = new NumericUpDown { Minimum = 300, Maximum = 5000, Increment = 100, Width = 64, Font = CampusTheme.BodyFont };
            Label p1 = new Label { Text = "ms  防连刷:", AutoSize = true, Font = CampusTheme.SmallFont, Margin = new Padding(2, 5, 2, 0) };
            nudDebounceSec = new NumericUpDown { Minimum = 1, Maximum = 15, Width = 48, Font = CampusTheme.BodyFont };
            Label p2 = new Label { Text = "秒", AutoSize = true, Font = CampusTheme.SmallFont, Margin = new Padding(2, 5, 2, 0) };
            pollRow.Controls.Add(nudPollMs);
            pollRow.Controls.Add(p1);
            pollRow.Controls.Add(nudDebounceSec);
            pollRow.Controls.Add(p2);
            tbl.Controls.Add(pollRow, 1, 3);

            AddRowLabel(tbl, 4, "报警联动：");
            FlowLayoutPanel alarmOpt = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            chkUseAcousto = new CheckBox { Text = "声光报警(0205)", AutoSize = true, Font = CampusTheme.SmallFont };
            chkUseBuzzer = new CheckBox { Text = "板载蜂鸣(0301)", AutoSize = true, Font = CampusTheme.SmallFont };
            Button btnSaveCfg = new Button { Text = "保存门禁参数", Width = 104, Height = 25 };
            CampusTheme.StylePrimaryButton(btnSaveCfg, CampusTheme.NavyAccent);
            btnSaveCfg.Click += BtnSaveCfg_Click;
            alarmOpt.Controls.Add(chkUseAcousto);
            alarmOpt.Controls.Add(chkUseBuzzer);
            alarmOpt.Controls.Add(btnSaveCfg);
            tbl.Controls.Add(alarmOpt, 1, 4);

            AddRowLabel(tbl, 5, "指令回执：");
            lblReaderResult = new Label { Text = "等待测试", Font = CampusTheme.SmallFont, ForeColor = CampusTheme.TextMuted, AutoSize = true, Margin = new Padding(2, 5, 2, 2) };
            tbl.Controls.Add(lblReaderResult, 1, 5);

            body.Controls.Add(tbl);
            return card;
        }

        private Panel CreateActuatorDebugCard()
        {
            Panel body;
            Panel card = CreateCardContainer("3. 门禁执行器联调（步进电机 / 声光报警 / 蜂鸣器）", out body);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Top;
            tbl.AutoSize = true;
            tbl.ColumnCount = 2;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddRowLabel(tbl, 0, "步进电机020B：");
            FlowLayoutPanel motorRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            nudTestAngle = new NumericUpDown { Minimum = 15, Maximum = 360, Value = 90, Increment = 15, Width = 58, Font = CampusTheme.BodyFont };
            Button btnFwd = new Button { Text = "正转开闸", Width = 80, Height = 26 };
            CampusTheme.StylePrimaryButton(btnFwd, CampusTheme.SuccessGreen);
            btnFwd.Click += delegate { RunMotorTest(0x01); };

            Button btnRev = new Button { Text = "反转关闸", Width = 80, Height = 26 };
            CampusTheme.StyleSecondaryButton(btnRev);
            btnRev.Click += delegate { RunMotorTest(0x02); };

            motorRow.Controls.Add(nudTestAngle);
            motorRow.Controls.Add(btnFwd);
            motorRow.Controls.Add(btnRev);
            tbl.Controls.Add(motorRow, 1, 0);

            AddRowLabel(tbl, 1, "声光报警0205：");
            FlowLayoutPanel acoustoRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 2) };
            Button btnAcOn = new Button { Text = "开启报警", Width = 82, Height = 26 };
            CampusTheme.StylePrimaryButton(btnAcOn, CampusTheme.DangerRed);
            btnAcOn.Click += delegate
            {
                string err;
                if (!_protocol.ControlAcoustoAlarm(true, out err)) lblActuatorResult.Text = err;
            };
            Button btnAcOff = new Button { Text = "关闭报警", Width = 82, Height = 26 };
            CampusTheme.StyleSecondaryButton(btnAcOff);
            btnAcOff.Click += delegate
            {
                string err;
                if (!_protocol.ControlAcoustoAlarm(false, out err)) lblActuatorResult.Text = err;
            };
            acoustoRow.Controls.Add(btnAcOn);
            acoustoRow.Controls.Add(btnAcOff);
            tbl.Controls.Add(acoustoRow, 1, 1);

            AddRowLabel(tbl, 2, "板载蜂鸣0301：");
            FlowLayoutPanel beepRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 2) };
            Button btnBeepOn = new Button { Text = "鸣响测试", Width = 82, Height = 26 };
            CampusTheme.StylePrimaryButton(btnBeepOn, CampusTheme.WarningAmber);
            btnBeepOn.Click += delegate
            {
                string err;
                if (!_protocol.ControlOnboardBuzzer(true, out err)) lblActuatorResult.Text = err;
            };
            Button btnBeepOff = new Button { Text = "静音关闭", Width = 82, Height = 26 };
            CampusTheme.StyleSecondaryButton(btnBeepOff);
            btnBeepOff.Click += delegate
            {
                string err;
                if (!_protocol.ControlOnboardBuzzer(false, out err)) lblActuatorResult.Text = err;
            };
            beepRow.Controls.Add(btnBeepOn);
            beepRow.Controls.Add(btnBeepOff);
            tbl.Controls.Add(beepRow, 1, 2);

            AddRowLabel(tbl, 3, "执行反馈：");
            lblActuatorResult = new Label { Text = "待调试", Font = CampusTheme.BodyBoldFont, ForeColor = CampusTheme.TextMuted, AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            tbl.Controls.Add(lblActuatorResult, 1, 3);

            body.Controls.Add(tbl);
            return card;
        }

        private Panel CreateT5557CopyCard()
        {
            Panel body;
            Panel card = CreateCardContainer("4. T5557 新卡写入（打通实验6：写为EM4100门禁卡）", out body);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Top;
            tbl.AutoSize = true;
            tbl.ColumnCount = 2;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddRowLabel(tbl, 0, "10位卡号：");
            FlowLayoutPanel idRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            tbT5557CardId = new TextBox { Font = CampusTheme.MonoFont, Width = 128, Text = "0006849456" };
            Button btnRand = new Button { Text = "随机卡号", Width = 78, Height = 24 };
            CampusTheme.StyleSecondaryButton(btnRand);
            btnRand.Click += delegate
            {
                tbT5557CardId.Text = "000" + _rng.Next(1000000, 9999999).ToString();
                GenerateEm4100Preview();
            };
            idRow.Controls.Add(tbT5557CardId);
            idRow.Controls.Add(btnRand);
            tbl.Controls.Add(idRow, 1, 0);

            AddRowLabel(tbl, 1, "厂商代码：");
            FlowLayoutPanel venRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            tbVendorCode = new TextBox { Font = CampusTheme.MonoFont, Width = 54, Text = "00" };
            Button btnGen = new Button { Text = "生成EM4100编码", Width = 122, Height = 24 };
            CampusTheme.StyleSecondaryButton(btnGen);
            btnGen.Click += delegate { GenerateEm4100Preview(); };
            venRow.Controls.Add(tbVendorCode);
            venRow.Controls.Add(btnGen);
            tbl.Controls.Add(venRow, 1, 1);

            AddRowLabel(tbl, 2, "8字节载荷：");
            tbEm4100Hex = new TextBox { Font = CampusTheme.MonoFont, Width = 230, ReadOnly = true, BackColor = Color.FromArgb(248, 250, 252) };
            tbl.Controls.Add(tbEm4100Hex, 1, 2);

            AddRowLabel(tbl, 3, "制卡与验证：");
            FlowLayoutPanel actRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 2) };
            Button btnWrite = new Button { Text = "写入T5557新卡", Width = 132, Height = 27 };
            CampusTheme.StylePrimaryButton(btnWrite, CampusTheme.NavyAccent);
            btnWrite.Click += BtnWriteT5557_Click;

            Button btnVerify = new Button { Text = "读卡验证", Width = 88, Height = 27 };
            CampusTheme.StylePrimaryButton(btnVerify, CampusTheme.SuccessGreen);
            btnVerify.Click += delegate
            {
                string err;
                if (!_protocol.RequestSingleCard(CardReadSource.DebugTab, out err))
                {
                    lblT5557Result.Text = err;
                    lblT5557Result.ForeColor = CampusTheme.DangerRed;
                }
            };
            actRow.Controls.Add(btnWrite);
            actRow.Controls.Add(btnVerify);
            tbl.Controls.Add(actRow, 1, 3);

            AddRowLabel(tbl, 4, "制卡反馈：");
            lblT5557Result = new Label { Text = "注意：写入前需先在实验3完成T5557解密与初始化", Font = CampusTheme.SmallFont, ForeColor = CampusTheme.TextMuted, AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            tbl.Controls.Add(lblT5557Result, 1, 4);

            body.Controls.Add(tbl);
            GenerateEm4100Preview();
            return card;
        }

        private static void AddRowLabel(TableLayoutPanel tbl, int row, string text)
        {
            Label lbl = new Label { Text = text, Font = CampusTheme.BodyFont, AutoSize = true, Margin = new Padding(0, 4, 4, 2) };
            tbl.Controls.Add(lbl, 0, row);
        }

        public void LoadCurrentSettingsToUI()
        {
            RefreshPortsList();
            ComConfig comCfg = JsonFormat.LoadComConfig();
            cbPorts.Text = comCfg.COM;
            cbBaud.Text = comCfg.BAUD.ToString();
            if (cbBaud.SelectedIndex < 0) cbBaud.SelectedIndex = 3; // 57600

            rbSimMode.Checked = _protocol.IsSimulationMode;
            rbRealSerial.Checked = !_protocol.IsSimulationMode;

            cbGateZone.Text = _settings.CurrentGateZone;
            if (cbGateZone.SelectedIndex < 0) cbGateZone.SelectedIndex = 0;

            nudAngle.Value = ClampNumeric(nudAngle, _settings.OpenAngle);
            nudPollMs.Value = ClampNumeric(nudPollMs, _settings.PollIntervalMs);
            nudDebounceSec.Value = ClampNumeric(nudDebounceSec, _settings.DebounceSeconds);
            nudAutoCloseSec.Value = ClampNumeric(nudAutoCloseSec, _settings.AutoCloseSeconds);
            chkUseAcousto.Checked = _settings.EnableAcoustoAlarm;
            chkUseBuzzer.Checked = _settings.EnableOnboardBuzzer;

            UpdateConnectionLabel();
        }

        private static decimal ClampNumeric(NumericUpDown nud, int val)
        {
            if (val < nud.Minimum) return nud.Minimum;
            if (val > nud.Maximum) return nud.Maximum;
            return val;
        }

        public void SyncSimulationModeFromHost()
        {
            rbSimMode.Checked = _protocol.IsSimulationMode;
            rbRealSerial.Checked = !_protocol.IsSimulationMode;
            UpdateConnectionLabel();
        }

        private void RefreshPortsList()
        {
            string prev = cbPorts.Text;
            cbPorts.Items.Clear();
            string[] ports = GateProtocolService.GetAvailablePorts();
            for (int i = 0; i < ports.Length; i++)
            {
                cbPorts.Items.Add(ports[i]);
            }
            if (!string.IsNullOrEmpty(prev))
            {
                cbPorts.Text = prev;
            }
            else if (cbPorts.Items.Count > 0)
            {
                cbPorts.SelectedIndex = 0;
            }
            else
            {
                cbPorts.Text = "COM3";
            }
        }

        private void RbMode_CheckedChanged(object sender, EventArgs e)
        {
            _protocol.IsSimulationMode = rbSimMode.Checked;
            _settings.SimulationMode = rbSimMode.Checked;
            JsonFormat.SaveSettings(_settings);
            UpdateConnectionLabel();
            if (ModeOrConnectionChanged != null)
            {
                ModeOrConnectionChanged(this, EventArgs.Empty);
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            string port = cbPorts.Text.Trim();
            int baud = 57600;
            int.TryParse(cbBaud.Text, out baud);

            if (_protocol.IsSimulationMode)
            {
                rbRealSerial.Checked = true;
            }

            string err;
            if (_protocol.OpenPort(port, baud, out err))
            {
                lblSerialState.Text = string.Format("已连接 {0} ({1} bps)", port, baud);
                lblSerialState.ForeColor = CampusTheme.SuccessGreen;
            }
            else
            {
                lblSerialState.Text = err;
                lblSerialState.ForeColor = CampusTheme.DangerRed;
                MessageBox.Show(err, "串口打开失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (ModeOrConnectionChanged != null)
            {
                ModeOrConnectionChanged(this, EventArgs.Empty);
            }
        }

        private void BtnSaveCom_Click(object sender, EventArgs e)
        {
            string port = cbPorts.Text.Trim();
            int baud = 57600;
            int.TryParse(cbBaud.Text, out baud);
            if (JsonFormat.SaveComConfig(port, baud))
            {
                lblSerialState.Text = string.Format("已保存 {0}/{1} 到 GlobalCom.json", port, baud);
                lblSerialState.ForeColor = CampusTheme.NavyAccent;
                if (StatusMessage != null) StatusMessage(this, lblSerialState.Text);
            }
        }

        private void BtnSaveCfg_Click(object sender, EventArgs e)
        {
            _settings.OpenAngle = (int)nudAngle.Value;
            _settings.PollIntervalMs = (int)nudPollMs.Value;
            _settings.DebounceSeconds = (int)nudDebounceSec.Value;
            _settings.AutoCloseSeconds = (int)nudAutoCloseSec.Value;
            _settings.EnableAcoustoAlarm = chkUseAcousto.Checked;
            _settings.EnableOnboardBuzzer = chkUseBuzzer.Checked;
            _settings.CurrentGateZone = cbGateZone.Text;
            _settings.SimulationMode = _protocol.IsSimulationMode;

            JsonFormat.SaveSettings(_settings);
            lblReaderResult.Text = "门禁参数已保存到 settings.json";
            lblReaderResult.ForeColor = CampusTheme.SuccessGreen;
            if (SettingsUpdated != null) SettingsUpdated(this, EventArgs.Empty);
            if (StatusMessage != null) StatusMessage(this, "门禁参数设置已保存");
        }

        private delegate bool SimpleProtocolCall(out string err);

        private void RunSimpleCmd(string label, SimpleProtocolCall call)
        {
            string err;
            if (!call(out err))
            {
                lblReaderResult.Text = err;
                lblReaderResult.ForeColor = CampusTheme.DangerRed;
            }
            else
            {
                lblReaderResult.Text = "已发送：" + label;
                lblReaderResult.ForeColor = CampusTheme.NavyAccent;
            }
        }

        private void RunRfCmd(bool on)
        {
            string err;
            if (!_protocol.SetRfSwitch(on, out err))
            {
                lblReaderResult.Text = err;
                lblReaderResult.ForeColor = CampusTheme.DangerRed;
            }
            else
            {
                lblReaderResult.Text = on ? "已发送开启射频场指令" : "已发送关闭射频场指令";
                lblReaderResult.ForeColor = CampusTheme.NavyAccent;
            }
        }

        private void RunMotorTest(byte direction)
        {
            string err;
            int angle = (int)nudTestAngle.Value;
            if (!_protocol.ControlStepMotor(direction, angle, out err))
            {
                lblActuatorResult.Text = err;
                lblActuatorResult.ForeColor = CampusTheme.DangerRed;
            }
            else
            {
                lblActuatorResult.Text = string.Format("已下发步进电机{0} {1}°", direction == 0x01 ? "正转" : "反转", angle);
                lblActuatorResult.ForeColor = CampusTheme.NavyAccent;
            }
        }

        private bool GenerateEm4100Preview()
        {
            uint cardId;
            if (!uint.TryParse(tbT5557CardId.Text.Trim(), out cardId))
            {
                tbEm4100Hex.Text = "卡号必须为无符号十进制整数";
                return false;
            }
            byte vendor = 0;
            byte.TryParse(tbVendorCode.Text.Trim(), out vendor);
            byte[] payload = GateProtocolService.EncodeEm4100Payload(cardId, vendor);
            tbEm4100Hex.Text = BitConverter.ToString(payload).Replace("-", " ");
            return true;
        }

        private void BtnWriteT5557_Click(object sender, EventArgs e)
        {
            if (!GenerateEm4100Preview())
            {
                lblT5557Result.Text = "卡号格式不正确，请输入有效十进制卡号（如 0006849456）";
                lblT5557Result.ForeColor = CampusTheme.DangerRed;
                return;
            }

            uint cardId = uint.Parse(tbT5557CardId.Text.Trim());
            byte vendor = 0;
            byte.TryParse(tbVendorCode.Text.Trim(), out vendor);

            string err;
            if (!_protocol.WriteT5557NewCard(cardId, vendor, out err))
            {
                lblT5557Result.Text = err;
                lblT5557Result.ForeColor = CampusTheme.DangerRed;
            }
            else
            {
                lblT5557Result.Text = "已发送T5557新卡写入指令，等待下位机确认...";
                lblT5557Result.ForeColor = CampusTheme.NavyAccent;
            }
        }

        public void HandleDeviceCommandResult(DeviceCommandResultEventArgs e)
        {
            if (e == null) return;
            if (e.CommandName == "T5557新卡制作")
            {
                lblT5557Result.Text = e.Message;
                lblT5557Result.ForeColor = e.Success ? CampusTheme.SuccessGreen : CampusTheme.DangerRed;
            }
            else
            {
                lblReaderResult.Text = e.Message;
                lblReaderResult.ForeColor = e.Success ? CampusTheme.SuccessGreen : CampusTheme.DangerRed;
            }
        }

        public void HandleMotorOrAlarmResult(string msg, bool success)
        {
            lblActuatorResult.Text = msg;
            lblActuatorResult.ForeColor = success ? CampusTheme.SuccessGreen : CampusTheme.DangerRed;
        }

        public void HandleDebugCardRead(CardReadEventArgs e)
        {
            if (e == null) return;
            if (e.CardFound)
            {
                lblT5557Result.Text = "读卡验证成功！当前卡号：" + e.CardNumber;
                lblT5557Result.ForeColor = CampusTheme.SuccessGreen;
            }
            else
            {
                lblT5557Result.Text = "读卡验证失败：" + e.RawMessage;
                lblT5557Result.ForeColor = CampusTheme.WarningAmber;
            }
        }

        private void UpdateConnectionLabel()
        {
            if (_protocol.IsSimulationMode)
            {
                lblSerialState.Text = "无硬件仿真模式运行中（所有指令生成真实CRC帧）";
                lblSerialState.ForeColor = Color.FromArgb(79, 70, 229);
            }
            else if (_protocol.IsPortOpen)
            {
                lblSerialState.Text = string.Format("串口已连接：{0} ({1} bps)", _protocol.CurrentPortName, _protocol.CurrentBaudRate);
                lblSerialState.ForeColor = CampusTheme.SuccessGreen;
            }
            else
            {
                lblSerialState.Text = "串口未连接（可点击连接或切换至仿真模式）";
                lblSerialState.ForeColor = CampusTheme.WarningAmber;
            }
        }
    }
}
