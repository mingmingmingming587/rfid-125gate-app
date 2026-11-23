using _125GateApp.Models;
using _125GateApp.Services;
using _125GateApp.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _125GateApp.Pages
{
    public class GateMonitorPage : UserControl
    {
        private readonly GateProtocolService _protocol;
        private readonly AppSettings _settings;

        private Timer _pollTimer;
        private Timer _autoCloseTimer;
        private Timer _alarmFlashTimer;
        private Timer _alarmAutoStopTimer;

        private bool _isDoorOpen = false;
        private bool _isAlarming = false;
        private bool _alarmFlashState = false;
        private int _autoCloseRemaining = 0;
        private int _alarmRemaining = 0;
        private string _lastDebouncedCard = string.Empty;
        private string _lastDeniedCard = string.Empty;
        private DateTime _lastSwipeTime = DateTime.MinValue;
        private string _pendingSwipeSource = "硬件读卡";
        private bool _awaitingPollResponse = false;

        // Controls
        private Button btnSingleSwipe;
        private CheckBox chkAutoPoll;
        private ComboBox cbOpenAngle;
        private CheckBox chkAutoClose;
        private Button btnManualOpen;
        private Button btnManualClose;
        private Button btnStopAlarm;

        private ComboBox cbSimCards;
        private TextBox tbSimCustomCard;
        private Button btnSimSwipe;

        private Panel pnlGateCanvas;
        private Label lblGateBanner;
        private Label lblCountdown;

        private Label lblValCard;
        private Label lblValName;
        private Label lblValStuId;
        private Label lblValSexRole;
        private Label lblValDept;
        private Label lblValZone;
        private Label lblValStatus;
        private Label lblValTime;
        private DataGridView dgvRecentLogs;

        public event EventHandler LogsUpdated;
        public event EventHandler RequestEnableSimulation;
        public event StatusTextEventHandler StatusMessage;

        public GateMonitorPage(GateProtocolService protocol, AppSettings settings)
        {
            _protocol = protocol;
            _settings = settings;
            InitializeTimers();
            InitializeLayout();
            RefreshSimulationDropdown();
            RefreshRecentLogs();
        }

        private void InitializeTimers()
        {
            _pollTimer = new Timer();
            _pollTimer.Interval = Math.Max(300, _settings.PollIntervalMs);
            _pollTimer.Tick += PollTimer_Tick;

            _autoCloseTimer = new Timer();
            _autoCloseTimer.Interval = 1000;
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;

            _alarmFlashTimer = new Timer();
            _alarmFlashTimer.Interval = 350;
            _alarmFlashTimer.Tick += delegate
            {
                _alarmFlashState = !_alarmFlashState;
                if (pnlGateCanvas != null) pnlGateCanvas.Invalidate();
            };

            _alarmAutoStopTimer = new Timer();
            _alarmAutoStopTimer.Interval = 1000;
            _alarmAutoStopTimer.Tick += AlarmAutoStopTimer_Tick;
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = CampusTheme.PageBg;
            this.Padding = new Padding(10);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(root);

            // Row 0: 硬件控制栏
            Panel ctrlBar = CreateControlBar();
            ctrlBar.Dock = DockStyle.Top;
            ctrlBar.Margin = new Padding(0, 0, 0, 6);
            root.Controls.Add(ctrlBar, 0, 0);

            // Row 1: 无硬件仿真快捷刷卡栏
            Panel simBar = CreateSimulationBar();
            simBar.Dock = DockStyle.Top;
            simBar.Margin = new Padding(0, 0, 0, 8);
            root.Controls.Add(simBar, 0, 1);

            // Row 2: 左右分栏
            TableLayoutPanel split = new TableLayoutPanel();
            split.Dock = DockStyle.Fill;
            split.ColumnCount = 2;
            split.RowCount = 1;
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            root.Controls.Add(split, 0, 2);

            Panel leftVisual = CreateLeftVisualPanel();
            leftVisual.Dock = DockStyle.Fill;
            leftVisual.Margin = new Padding(0, 0, 8, 0);
            split.Controls.Add(leftVisual, 0, 0);

            TableLayoutPanel rightPanel = new TableLayoutPanel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.ColumnCount = 1;
            rightPanel.RowCount = 2;
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 174F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            split.Controls.Add(rightPanel, 1, 0);

            Panel personCard = CreatePersonCard();
            personCard.Dock = DockStyle.Fill;
            personCard.Margin = new Padding(0, 0, 0, 8);
            rightPanel.Controls.Add(personCard, 0, 0);

            Panel logsCard = CreateRecentLogsCard();
            logsCard.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(logsCard, 0, 1);
        }

        private Panel CreateControlBar()
        {
            Panel bar = new Panel();
            bar.BackColor = CampusTheme.CardBg;
            bar.BorderStyle = BorderStyle.FixedSingle;
            bar.Padding = new Padding(8, 6, 8, 6);
            bar.AutoSize = true;

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoSize = true;
            flow.WrapContents = true;

            btnSingleSwipe = new Button();
            btnSingleSwipe.Text = "单次刷卡检测";
            btnSingleSwipe.Width = 112;
            btnSingleSwipe.Height = 28;
            CampusTheme.StylePrimaryButton(btnSingleSwipe, CampusTheme.NavyAccent);
            btnSingleSwipe.Click += delegate { TriggerCardScan("硬件读卡"); };

            chkAutoPoll = new CheckBox();
            chkAutoPoll.Text = "自动轮询感应";
            chkAutoPoll.Font = CampusTheme.BodyBoldFont;
            chkAutoPoll.ForeColor = CampusTheme.NavyHeader;
            chkAutoPoll.AutoSize = true;
            chkAutoPoll.Margin = new Padding(8, 6, 8, 0);
            chkAutoPoll.CheckedChanged += ChkAutoPoll_CheckedChanged;

            Label lblAngle = new Label();
            lblAngle.Text = "开闸角度:";
            lblAngle.Font = CampusTheme.BodyFont;
            lblAngle.AutoSize = true;
            lblAngle.Margin = new Padding(6, 7, 2, 0);

            cbOpenAngle = new ComboBox();
            cbOpenAngle.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOpenAngle.Font = CampusTheme.BodyFont;
            cbOpenAngle.Width = 68;
            cbOpenAngle.Margin = new Padding(0, 3, 6, 0);
            cbOpenAngle.Items.AddRange(new object[] { "90", "120", "180" });
            cbOpenAngle.Text = _settings.OpenAngle.ToString();
            if (cbOpenAngle.SelectedIndex < 0) cbOpenAngle.SelectedIndex = 0;

            chkAutoClose = new CheckBox();
            chkAutoClose.Text = "延时自动关门";
            chkAutoClose.Font = CampusTheme.BodyFont;
            chkAutoClose.AutoSize = true;
            chkAutoClose.Checked = _settings.AutoCloseEnabled;
            chkAutoClose.Margin = new Padding(4, 6, 10, 0);
            chkAutoClose.CheckedChanged += delegate { _settings.AutoCloseEnabled = chkAutoClose.Checked; };

            btnManualOpen = new Button();
            btnManualOpen.Text = "手动开闸";
            btnManualOpen.Width = 84;
            btnManualOpen.Height = 28;
            CampusTheme.StylePrimaryButton(btnManualOpen, CampusTheme.SuccessGreen);
            btnManualOpen.Click += BtnManualOpen_Click;

            btnManualClose = new Button();
            btnManualClose.Text = "手动关闸复位";
            btnManualClose.Width = 104;
            btnManualClose.Height = 28;
            CampusTheme.StyleSecondaryButton(btnManualClose);
            btnManualClose.Click += BtnManualClose_Click;

            btnStopAlarm = new Button();
            btnStopAlarm.Text = "停止报警";
            btnStopAlarm.Width = 84;
            btnStopAlarm.Height = 28;
            CampusTheme.StylePrimaryButton(btnStopAlarm, CampusTheme.DangerRed);
            btnStopAlarm.Click += BtnStopAlarm_Click;

            flow.Controls.Add(btnSingleSwipe);
            flow.Controls.Add(chkAutoPoll);
            flow.Controls.Add(lblAngle);
            flow.Controls.Add(cbOpenAngle);
            flow.Controls.Add(chkAutoClose);
            flow.Controls.Add(btnManualOpen);
            flow.Controls.Add(btnManualClose);
            flow.Controls.Add(btnStopAlarm);

            bar.Controls.Add(flow);
            return bar;
        }

        private Panel CreateSimulationBar()
        {
            Panel bar = new Panel();
            bar.BackColor = CampusTheme.InfoSoft;
            bar.BorderStyle = BorderStyle.FixedSingle;
            bar.Padding = new Padding(8, 5, 8, 5);
            bar.AutoSize = true;

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoSize = true;
            flow.WrapContents = true;

            Label lblSimTitle = new Label();
            lblSimTitle.Text = "无硬件仿真刷卡测试：";
            lblSimTitle.Font = CampusTheme.BodyBoldFont;
            lblSimTitle.ForeColor = CampusTheme.NavyHeader;
            lblSimTitle.AutoSize = true;
            lblSimTitle.Margin = new Padding(0, 6, 4, 0);

            cbSimCards = new ComboBox();
            cbSimCards.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSimCards.Font = CampusTheme.BodyFont;
            cbSimCards.Width = 280;
            cbSimCards.Margin = new Padding(0, 3, 6, 0);
            cbSimCards.SelectedIndexChanged += delegate
            {
                if (cbSimCards.SelectedIndex >= 0)
                {
                    string item = cbSimCards.Text;
                    if (item.StartsWith("["))
                    {
                        tbSimCustomCard.Enabled = false;
                    }
                    else
                    {
                        tbSimCustomCard.Enabled = true;
                        int dashIdx = item.IndexOf(' ');
                        tbSimCustomCard.Text = dashIdx > 0 ? item.Substring(0, dashIdx) : item;
                    }
                }
            };

            Label lblCustom = new Label();
            lblCustom.Text = "卡号:";
            lblCustom.Font = CampusTheme.BodyFont;
            lblCustom.AutoSize = true;
            lblCustom.Margin = new Padding(4, 6, 2, 0);

            tbSimCustomCard = new TextBox();
            tbSimCustomCard.Font = CampusTheme.MonoFont;
            tbSimCustomCard.Width = 112;
            tbSimCustomCard.Margin = new Padding(0, 3, 6, 0);
            tbSimCustomCard.Text = "0006849456";

            btnSimSwipe = new Button();
            btnSimSwipe.Text = "执行模拟刷卡";
            btnSimSwipe.Width = 112;
            btnSimSwipe.Height = 27;
            CampusTheme.StylePrimaryButton(btnSimSwipe, Color.FromArgb(79, 70, 229));
            btnSimSwipe.Click += BtnSimSwipe_Click;

            flow.Controls.Add(lblSimTitle);
            flow.Controls.Add(cbSimCards);
            flow.Controls.Add(lblCustom);
            flow.Controls.Add(tbSimCustomCard);
            flow.Controls.Add(btnSimSwipe);

            bar.Controls.Add(flow);
            return bar;
        }

        private Panel CreateLeftVisualPanel()
        {
            Panel card = new Panel();
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = "智能道闸状态与通行判定";
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 26;

            lblGateBanner = new Label();
            lblGateBanner.Text = "待机中：请刷校园门禁卡";
            lblGateBanner.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            lblGateBanner.ForeColor = CampusTheme.NavyHeader;
            lblGateBanner.BackColor = CampusTheme.InfoSoft;
            lblGateBanner.TextAlign = ContentAlignment.MiddleCenter;
            lblGateBanner.Dock = DockStyle.Top;
            lblGateBanner.Height = 48;

            lblCountdown = new Label();
            lblCountdown.Text = "当前门区：" + _settings.CurrentGateZone + "  |  闸机已关闭";
            lblCountdown.Font = CampusTheme.BodyFont;
            lblCountdown.ForeColor = CampusTheme.TextMuted;
            lblCountdown.TextAlign = ContentAlignment.MiddleCenter;
            lblCountdown.Dock = DockStyle.Bottom;
            lblCountdown.Height = 26;

            pnlGateCanvas = new Panel();
            pnlGateCanvas.Dock = DockStyle.Fill;
            pnlGateCanvas.BackColor = Color.FromArgb(248, 250, 252);
            pnlGateCanvas.Paint += PnlGateCanvas_Paint;

            card.Controls.Add(pnlGateCanvas);
            card.Controls.Add(lblCountdown);
            card.Controls.Add(lblGateBanner);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Panel CreatePersonCard()
        {
            Panel card = new Panel();
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = "当前刷卡人员信息";
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 24;

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 4;
            grid.RowCount = 4;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            lblValCard = AddInfoField(grid, 0, 0, "卡号：", "--");
            lblValStatus = AddInfoField(grid, 0, 2, "卡状态：", "--");
            lblValName = AddInfoField(grid, 1, 0, "姓名：", "--");
            lblValStuId = AddInfoField(grid, 1, 2, "学号/工号：", "--");
            lblValSexRole = AddInfoField(grid, 2, 0, "性别/身份：", "--");
            lblValDept = AddInfoField(grid, 2, 2, "院系/单位：", "--");
            lblValZone = AddInfoField(grid, 3, 0, "授权门区：", "--");
            lblValTime = AddInfoField(grid, 3, 2, "刷卡时间：", "--");

            card.Controls.Add(grid);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Label AddInfoField(TableLayoutPanel grid, int row, int col, string caption, string initial)
        {
            Label cap = new Label();
            cap.Text = caption;
            cap.Font = CampusTheme.BodyFont;
            cap.ForeColor = CampusTheme.TextMuted;
            cap.AutoSize = true;
            cap.Margin = new Padding(0, 4, 2, 2);
            grid.Controls.Add(cap, col, row);

            Label val = new Label();
            val.Text = initial;
            val.Font = CampusTheme.BodyBoldFont;
            val.ForeColor = CampusTheme.TextPrimary;
            val.AutoSize = true;
            val.Margin = new Padding(0, 4, 4, 2);
            grid.Controls.Add(val, col + 1, row);
            return val;
        }

        private Panel CreateRecentLogsCard()
        {
            Panel card = new Panel();
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = "实时通行动态（最近 15 条）";
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 24;

            dgvRecentLogs = new DataGridView();
            dgvRecentLogs.Dock = DockStyle.Fill;
            CampusTheme.StyleDataGridView(dgvRecentLogs);
            dgvRecentLogs.Columns.Add("time", "时间");
            dgvRecentLogs.Columns.Add("card", "卡号");
            dgvRecentLogs.Columns.Add("name", "姓名");
            dgvRecentLogs.Columns.Add("event", "事件类型");
            dgvRecentLogs.Columns.Add("result", "结果");

            card.Controls.Add(dgvRecentLogs);
            card.Controls.Add(lblTitle);
            return card;
        }

        public void SyncSettings()
        {
            _pollTimer.Interval = Math.Max(300, _settings.PollIntervalMs);
            cbOpenAngle.Text = _settings.OpenAngle.ToString();
            chkAutoClose.Checked = _settings.AutoCloseEnabled;
            UpdateCountdownLabel();
        }

        public void RefreshSimulationDropdown()
        {
            string prev = cbSimCards.Text;
            cbSimCards.Items.Clear();
            List<CampusCardRecord> cards = JsonFormat.LoadCards();
            for (int i = 0; i < cards.Count; i++)
            {
                CampusCardRecord c = cards[i];
                cbSimCards.Items.Add(string.Format("{0} - {1} [{2}/{3}]", c.cardnum, c.name, c.role, c.status));
            }
            cbSimCards.Items.Add("0099887766 - [未注册陌生卡]");
            cbSimCards.Items.Add("[模拟无卡 0xFF]");

            if (!string.IsNullOrEmpty(prev))
            {
                int idx = cbSimCards.Items.IndexOf(prev);
                if (idx >= 0) { cbSimCards.SelectedIndex = idx; return; }
            }
            if (cbSimCards.Items.Count > 0)
            {
                cbSimCards.SelectedIndex = 0;
            }
        }

        public void RefreshRecentLogs()
        {
            List<AccessLogEntry> logs = JsonFormat.LoadLogs();
            dgvRecentLogs.Rows.Clear();
            int count = Math.Min(logs.Count, 15);
            for (int i = 0; i < count; i++)
            {
                AccessLogEntry log = logs[i];
                string shortTime = log.timestamp.Length >= 19 ? log.timestamp.Substring(11, 8) : log.timestamp;
                int idx = dgvRecentLogs.Rows.Add(shortTime, log.cardnum, log.name, log.eventType, log.result);
                DataGridViewRow row = dgvRecentLogs.Rows[idx];
                if (log.result == "允许通行")
                {
                    row.DefaultCellStyle.BackColor = CampusTheme.SuccessSoft;
                }
                else if (log.result == "拒绝并报警")
                {
                    row.DefaultCellStyle.BackColor = CampusTheme.DangerSoft;
                    row.DefaultCellStyle.ForeColor = CampusTheme.DangerRed;
                }
            }
        }

        private void ChkAutoPoll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoPoll.Checked)
            {
                if (!_protocol.IsSimulationMode && !_protocol.IsPortOpen)
                {
                    chkAutoPoll.Checked = false;
                    MessageBox.Show("当前未连接串口，请先打开串口或启用【无硬件仿真模式】后再开启自动轮询。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _pollTimer.Interval = Math.Max(300, _settings.PollIntervalMs);
                _pollTimer.Start();
                RaiseStatus("已开启门禁自动轮询感应");
            }
            else
            {
                _pollTimer.Stop();
                _awaitingPollResponse = false;
                RaiseStatus("已停止自动轮询");
            }
        }

        public void StopPollingIfRunning()
        {
            if (chkAutoPoll.Checked)
            {
                chkAutoPoll.Checked = false;
            }
            _pollTimer.Stop();
            _autoCloseTimer.Stop();
            _alarmFlashTimer.Stop();
            _alarmAutoStopTimer.Stop();
            if (_isAlarming)
            {
                StopAlarmVisualAndDevice(true);
            }
        }

        private void PollTimer_Tick(object sender, EventArgs e)
        {
            if (_awaitingPollResponse) return;
            _awaitingPollResponse = true;
            TriggerCardScan("硬件读卡");
        }

        private void TriggerCardScan(string sourceLabel)
        {
            _pendingSwipeSource = _protocol.IsSimulationMode ? "仿真读卡" : sourceLabel;
            string err;
            if (!_protocol.RequestSingleCard(CardReadSource.GateMonitorTab, out err))
            {
                _awaitingPollResponse = false;
                if (chkAutoPoll.Checked)
                {
                    chkAutoPoll.Checked = false;
                }
                SetBanner(err, CampusTheme.DangerRed, CampusTheme.DangerSoft);
            }
        }

        private void BtnSimSwipe_Click(object sender, EventArgs e)
        {
            if (!_protocol.IsSimulationMode)
            {
                DialogResult res = MessageBox.Show(
                    "当前处于真实串口模式，是否一键切换到【无硬件仿真模式】并立即模拟刷卡？",
                    "切换到仿真模式",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (res != DialogResult.Yes) return;
                if (RequestEnableSimulation != null)
                {
                    RequestEnableSimulation(this, EventArgs.Empty);
                }
            }

            if (cbSimCards.Text == "[模拟无卡 0xFF]")
            {
                _protocol.SimulateNoCard = true;
                _protocol.SimulatedCardNumber = string.Empty;
            }
            else
            {
                string custom = tbSimCustomCard.Text.Trim();
                if (!GateProtocolService.IsValidCardNumber(custom))
                {
                    MessageBox.Show("请输入6~12位数字卡号进行模拟刷卡。", "卡号格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _protocol.SimulateNoCard = false;
                _protocol.SimulatedCardNumber = custom;
            }

            TriggerCardScan("仿真读卡");
        }

        public void HandleCardRead(CardReadEventArgs e)
        {
            _awaitingPollResponse = false;
            if (e == null) return;

            if (!e.CardFound)
            {
                // 卡片移开感应区（0xFF）时重置防连刷状态，下次放卡可立即响应
                _lastDebouncedCard = string.Empty;
                _lastDeniedCard = string.Empty;

                // 无卡 0xFF 正常忽略（如果是手动单次点击，提示未检测到卡，绝不误触发报警）
                if (!chkAutoPoll.Checked)
                {
                    SetBanner("未检测到低频卡（下位机返回 0xFF），请贴近读卡器重试", CampusTheme.WarningAmber, CampusTheme.WarningSoft);
                    lblValCard.Text = "未检测到卡";
                    lblValTime.Text = DateTime.Now.ToString("HH:mm:ss");
                }
                return;
            }

            // 自动轮询防连刷去抖：合法卡按去抖秒数防连刷；同一张被拦截的非法卡一直贴着不移开时，避免反复循环拉响警报
            if (chkAutoPoll.Checked)
            {
                if (string.Equals(e.CardNumber, _lastDeniedCard, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if (string.Equals(e.CardNumber, _lastDebouncedCard, StringComparison.OrdinalIgnoreCase) &&
                    (DateTime.Now - _lastSwipeTime).TotalSeconds < Math.Max(1, _settings.DebounceSeconds))
                {
                    return;
                }
            }

            _lastDebouncedCard = e.CardNumber;
            _lastSwipeTime = DateTime.Now;

            ProcessAccessCheck(e.CardNumber, e.IsSimulated ? "仿真读卡" : _pendingSwipeSource);
        }

        private void ProcessAccessCheck(string cardNumber, string sourceText)
        {
            AccessDecision decision = AccessEvaluator.Evaluate(cardNumber, _settings.CurrentGateZone);
            string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            lblValCard.Text = cardNumber;
            lblValTime.Text = DateTime.Now.ToString("HH:mm:ss");

            if (decision.MatchedCard != null)
            {
                CampusCardRecord m = decision.MatchedCard;
                lblValName.Text = m.name;
                lblValStuId.Text = m.stuid;
                lblValSexRole.Text = m.sex + " / " + m.role;
                lblValDept.Text = m.department;
                lblValZone.Text = m.gateZone;
                lblValStatus.Text = m.status;
                lblValStatus.ForeColor = m.status == CardStatuses.Normal ? CampusTheme.SuccessGreen : CampusTheme.DangerRed;
            }
            else
            {
                lblValName.Text = "未登记人员";
                lblValStuId.Text = "--";
                lblValSexRole.Text = "--";
                lblValDept.Text = "--";
                lblValZone.Text = "--";
                lblValStatus.Text = "未注册";
                lblValStatus.ForeColor = CampusTheme.DangerRed;
            }

            if (decision.Granted)
            {
                _lastDeniedCard = string.Empty;
                // 若此前正在报警，刷合法卡放行时立即下发硬件停警指令
                StopAlarmVisualAndDevice(_isAlarming);
                SetBanner(decision.Reason, CampusTheme.SuccessGreen, CampusTheme.SuccessSoft);

                int angle = GetSelectedAngle();
                string err;
                if (_protocol.ControlStepMotor(0x01, angle, out err))
                {
                    SetDoorOpenState(true);
                }
                else
                {
                    SetBanner("验证通过，但开闸指令失败：" + err, CampusTheme.WarningAmber, CampusTheme.WarningSoft);
                }
            }
            else
            {
                _lastDeniedCard = cardNumber;
                SetDoorOpenState(false);
                StartAlarmWithAutoStop(decision.Reason);
            }

            AccessLogEntry log = new AccessLogEntry
            {
                timestamp = nowStr,
                cardnum = cardNumber,
                name = decision.MatchedCard != null ? decision.MatchedCard.name : "未知访客",
                stuid = decision.MatchedCard != null ? decision.MatchedCard.stuid : "-",
                role = decision.MatchedCard != null ? decision.MatchedCard.role : "未注册",
                department = decision.MatchedCard != null ? decision.MatchedCard.department : "-",
                eventType = decision.EventType,
                result = decision.ResultText,
                source = sourceText,
                detail = decision.Reason
            };
            JsonFormat.AppendLog(log);
            RefreshRecentLogs();
            if (LogsUpdated != null)
            {
                LogsUpdated(this, EventArgs.Empty);
            }
            RaiseStatus(decision.Reason);
        }

        public void HandleMotorAck(MotorResultEventArgs e)
        {
            if (e == null) return;
            if (!e.Success)
            {
                SetBanner("硬件异常：" + e.Message, CampusTheme.DangerRed, CampusTheme.DangerSoft);
                RaiseStatus(e.Message);
            }
            else
            {
                RaiseStatus(e.Message);
            }
        }

        public void HandleAlarmAck(AlarmResultEventArgs e)
        {
            if (e == null) return;
            RaiseStatus(e.Message);
        }

        private void BtnManualOpen_Click(object sender, EventArgs e)
        {
            StopAlarmVisualAndDevice(_isAlarming);
            string err;
            if (_protocol.ControlStepMotor(0x01, GetSelectedAngle(), out err))
            {
                SetDoorOpenState(true);
                SetBanner("管理员手动开闸放行", CampusTheme.NavyAccent, CampusTheme.InfoSoft);
                RecordControlLog("手动开闸", "管理员从监控台手动开启门禁闸机");
            }
            else
            {
                MessageBox.Show(err, "开闸失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnManualClose_Click(object sender, EventArgs e)
        {
            _autoCloseTimer.Stop();
            _autoCloseRemaining = 0;
            string err;
            if (_protocol.ControlStepMotor(0x02, GetSelectedAngle(), out err))
            {
                SetDoorOpenState(false);
                SetBanner("已发送手动关闸复位指令", CampusTheme.NavyHeader, CampusTheme.InfoSoft);
                RecordControlLog("手动关闸", "管理员手动反转关闸复位");
            }
            else
            {
                MessageBox.Show(err, "关闸失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartAlarmWithAutoStop(string reason)
        {
            _alarmAutoStopTimer.Stop();
            _isAlarming = true;
            _alarmFlashTimer.Start();
            SetBanner("拦截报警：" + reason, CampusTheme.DangerRed, CampusTheme.DangerSoft);
            TriggerHardwareAlarms(true);

            if (_settings.AlarmAutoStopSeconds > 0)
            {
                _alarmRemaining = _settings.AlarmAutoStopSeconds;
                _alarmAutoStopTimer.Start();
            }
            else
            {
                _alarmRemaining = 0;
            }
            UpdateCountdownLabel();
            pnlGateCanvas.Invalidate();
        }

        private void AlarmAutoStopTimer_Tick(object sender, EventArgs e)
        {
            _alarmRemaining--;
            if (_alarmRemaining <= 0)
            {
                _alarmAutoStopTimer.Stop();
                StopAlarmVisualAndDevice(true);
                SetBanner("报警提示已自动停止，门禁恢复待机", CampusTheme.NavyHeader, CampusTheme.InfoSoft);
            }
            else
            {
                UpdateCountdownLabel();
            }
        }

        private void BtnStopAlarm_Click(object sender, EventArgs e)
        {
            StopAlarmVisualAndDevice(true);
            SetBanner("已解除报警状态，恢复待机", CampusTheme.NavyHeader, CampusTheme.InfoSoft);
        }

        private void StopAlarmVisualAndDevice(bool sendOffCommand)
        {
            _alarmAutoStopTimer.Stop();
            _alarmRemaining = 0;
            _isAlarming = false;
            _alarmFlashState = false;
            _alarmFlashTimer.Stop();
            if (sendOffCommand)
            {
                TriggerHardwareAlarms(false);
            }
            pnlGateCanvas.Invalidate();
            UpdateCountdownLabel();
        }

        private void TriggerHardwareAlarms(bool turnOn)
        {
            string err;
            if (turnOn)
            {
                if (_settings.EnableAcoustoAlarm)
                {
                    _protocol.ControlAcoustoAlarm(true, out err);
                }
                if (_settings.EnableOnboardBuzzer)
                {
                    _protocol.ControlOnboardBuzzer(true, out err);
                }
            }
            else
            {
                // 停止报警时优先发送声光报警器关闭指令（板载蜂鸣器在固件中滴3声后自停，必要时也一并静音）
                if (_settings.EnableAcoustoAlarm)
                {
                    _protocol.ControlAcoustoAlarm(false, out err);
                }
                if (_settings.EnableOnboardBuzzer)
                {
                    _protocol.ControlOnboardBuzzer(false, out err);
                }
            }
        }

        private void SetDoorOpenState(bool open)
        {
            _isDoorOpen = open;
            _autoCloseTimer.Stop();
            if (open && chkAutoClose.Checked)
            {
                _autoCloseRemaining = Math.Max(2, _settings.AutoCloseSeconds);
                _autoCloseTimer.Start();
            }
            else
            {
                _autoCloseRemaining = 0;
            }
            UpdateCountdownLabel();
            pnlGateCanvas.Invalidate();
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            _autoCloseRemaining--;
            if (_autoCloseRemaining <= 0)
            {
                _autoCloseTimer.Stop();
                string err;
                if (_protocol.ControlStepMotor(0x02, GetSelectedAngle(), out err))
                {
                    _isDoorOpen = false;
                    SetBanner("延时结束，已自动反转关闸恢复待机", CampusTheme.NavyHeader, CampusTheme.InfoSoft);
                    RecordControlLog("自动关闸", "开闸延时结束后自动反转复位");
                }
                else
                {
                    _isDoorOpen = false;
                }
                UpdateCountdownLabel();
                pnlGateCanvas.Invalidate();
            }
            else
            {
                UpdateCountdownLabel();
            }
        }

        private void RecordControlLog(string eventType, string detail)
        {
            AccessLogEntry log = new AccessLogEntry
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                cardnum = "SYSTEM",
                name = "值班管理员",
                stuid = "ADMIN",
                role = "管理员",
                department = _settings.CurrentGateZone,
                eventType = eventType,
                result = "设备动作",
                source = "手动控制",
                detail = detail
            };
            JsonFormat.AppendLog(log);
            RefreshRecentLogs();
            if (LogsUpdated != null)
            {
                LogsUpdated(this, EventArgs.Empty);
            }
        }

        private int GetSelectedAngle()
        {
            int angle;
            if (int.TryParse(cbOpenAngle.Text, out angle) && angle > 0)
            {
                return angle;
            }
            return 90;
        }

        private void UpdateCountdownLabel()
        {
            string doorText;
            if (_isAlarming)
            {
                doorText = _alarmRemaining > 0
                    ? string.Format("声光报警拦截中（{0} 秒后自动停止）", _alarmRemaining)
                    : "声光报警拦截中（请点击停止报警）";
            }
            else if (_isDoorOpen)
            {
                doorText = _autoCloseRemaining > 0
                    ? string.Format("闸机已开启（{0} 秒后自动反转关闸）", _autoCloseRemaining)
                    : "闸机已开启（等待手动关闸）";
            }
            else
            {
                doorText = "闸机已关闭（待机监控中）";
            }
            lblCountdown.Text = string.Format("当前门区：{0}  |  {1}", _settings.CurrentGateZone, doorText);
        }

        private void SetBanner(string text, Color fore, Color back)
        {
            lblGateBanner.Text = text;
            lblGateBanner.ForeColor = fore;
            lblGateBanner.BackColor = back;
        }

        private void PnlGateCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = pnlGateCanvas.ClientSize.Width;
            int h = pnlGateCanvas.ClientSize.Height;

            if (_isAlarming && _alarmFlashState)
            {
                using (SolidBrush flashBg = new SolidBrush(Color.FromArgb(254, 226, 226)))
                {
                    g.FillRectangle(flashBg, pnlGateCanvas.ClientRectangle);
                }
            }

            // 地面标线
            int groundY = h - 34;
            using (Pen groundPen = new Pen(Color.FromArgb(203, 213, 225), 3f))
            {
                g.DrawLine(groundPen, 24, groundY, w - 24, groundY);
            }

            // 闸机主机箱（左侧立柱）
            int boxX = 48;
            int boxW = 50;
            int boxH = Math.Min(108, Math.Max(68, groundY - 24));
            int boxY = groundY - boxH;
            Rectangle boxRect = new Rectangle(boxX, boxY, boxW, boxH);
            using (SolidBrush boxBrush = new SolidBrush(Color.FromArgb(30, 58, 95)))
            {
                g.FillRectangle(boxBrush, boxRect);
            }
            using (Pen boxPen = new Pen(Color.FromArgb(15, 23, 42), 2f))
            {
                g.DrawRectangle(boxPen, boxRect);
            }

            // 状态指示灯
            Color ledColor = _isAlarming
                ? (_alarmFlashState ? CampusTheme.DangerRed : Color.FromArgb(185, 28, 28))
                : (_isDoorOpen ? CampusTheme.SuccessGreen : CampusTheme.NavyAccent);

            Rectangle ledRect = new Rectangle(boxX + 16, boxY + 14, 22, 22);
            using (SolidBrush ledBrush = new SolidBrush(ledColor))
            {
                g.FillEllipse(ledBrush, ledRect);
            }
            using (Pen ledBorder = new Pen(Color.White, 2f))
            {
                g.DrawEllipse(ledBorder, ledRect);
            }

            // 转轴与道闸横杆
            int pivotX = boxX + boxW - 4;
            int pivotY = boxY + 52;
            int armLen = Math.Min(210, Math.Max(120, w - pivotX - 70));
            double rad = _isDoorOpen ? (-72.0 * Math.PI / 180.0) : 0.0;
            float endX = pivotX + (float)(armLen * Math.Cos(rad));
            float endY = pivotY + (float)(armLen * Math.Sin(rad));

            Color armColor = _isAlarming ? CampusTheme.DangerRed : (_isDoorOpen ? CampusTheme.SuccessGreen : Color.FromArgb(234, 88, 12));
            using (Pen armPen = new Pen(armColor, 11f))
            {
                armPen.StartCap = LineCap.Round;
                armPen.EndCap = LineCap.Round;
                g.DrawLine(armPen, pivotX, pivotY, endX, endY);
            }

            // 横杆白色反光条
            using (Pen stripePen = new Pen(Color.White, 3f))
            {
                stripePen.DashStyle = DashStyle.Dash;
                g.DrawLine(stripePen, pivotX + 12 * (float)Math.Cos(rad), pivotY + 12 * (float)Math.Sin(rad), endX, endY);
            }

            // 轴心圆盘
            using (SolidBrush hubBrush = new SolidBrush(Color.White))
            using (Pen hubPen = new Pen(Color.FromArgb(51, 65, 85), 3f))
            {
                g.FillEllipse(hubBrush, pivotX - 9, pivotY - 9, 18, 18);
                g.DrawEllipse(hubPen, pivotX - 9, pivotY - 9, 18, 18);
            }

            // 右侧读卡区立柱标识
            int readerX = w - 76;
            Rectangle readerPost = new Rectangle(readerX, groundY - 76, 36, 76);
            using (SolidBrush postBrush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                g.FillRectangle(postBrush, readerPost);
            }
            using (Font rfFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold))
            using (SolidBrush wBrush = new SolidBrush(Color.White))
            {
                g.DrawString("125K", rfFont, wBrush, readerX + 3, groundY - 64);
                g.DrawString("读卡", rfFont, wBrush, readerX + 5, groundY - 46);
            }

            // 状态文字标识（置于地面标线下方，不遮挡道闸横杆）
            string bigLabel = _isAlarming ? "报警拦截 · 禁止通行" : (_isDoorOpen ? "闸机开启 · 允许通行" : "闸机闭合 · 待机感应");
            using (Font stateFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold))
            using (SolidBrush stateBrush = new SolidBrush(ledColor))
            {
                g.DrawString(bigLabel, stateFont, stateBrush, boxX + boxW + 24, groundY + 6);
            }
        }

        private void RaiseStatus(string msg)
        {
            if (StatusMessage != null)
            {
                StatusMessage(this, msg);
            }
        }
    }
}
