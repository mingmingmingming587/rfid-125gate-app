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
    public class RegisterPage : UserControl
    {
        private readonly GateProtocolService _protocol;
        private readonly Random _rng = new Random();

        private TextBox tbCardNum;
        private CheckBox chkManualCard;
        private Button btnReadCard;
        private Button btnSimCard;
        private Label lblCardStatus;

        private TextBox tbName;
        private TextBox tbStuId;
        private ComboBox cbSex;
        private ComboBox cbRole;
        private ComboBox cbDepartment;
        private ComboBox cbGateZone;
        private ComboBox cbStatus;
        private DateTimePicker dtpValidUntil;
        private CheckBox chkPermanent;

        private Button btnSave;
        private Button btnLoadExisting;
        private Button btnClear;
        private Label lblSaveResult;

        private Panel pnlCardPreview;
        private DataGridView dgvRecent;

        public event EventHandler CardsChanged;
        public event StatusTextEventHandler StatusMessage;

        public RegisterPage(GateProtocolService protocol)
        {
            _protocol = protocol;
            InitializeLayout();
            RefreshRecentList();
            UpdatePreview();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = CampusTheme.PageBg;
            this.Padding = new Padding(10);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F));
            this.Controls.Add(root);

            Panel leftCard = CreateLeftFormCard();
            leftCard.Dock = DockStyle.Fill;
            leftCard.Margin = new Padding(0, 0, 8, 0);
            root.Controls.Add(leftCard, 0, 0);

            TableLayoutPanel rightCol = new TableLayoutPanel();
            rightCol.Dock = DockStyle.Fill;
            rightCol.ColumnCount = 1;
            rightCol.RowCount = 2;
            rightCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 236F));
            rightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(rightCol, 1, 0);

            Panel previewWrap = CreatePreviewCard();
            previewWrap.Dock = DockStyle.Fill;
            previewWrap.Margin = new Padding(0, 0, 0, 8);
            rightCol.Controls.Add(previewWrap, 0, 0);

            Panel recentWrap = CreateRecentPanel();
            recentWrap.Dock = DockStyle.Fill;
            rightCol.Controls.Add(recentWrap, 0, 1);
        }

        private Panel CreateLeftFormCard()
        {
            Panel card = new Panel();
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(12, 10, 12, 8);

            Label lblTitle = new Label();
            lblTitle.Text = "校园门禁卡发卡与人员注册";
            lblTitle.Font = CampusTheme.TitleFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 26;

            Panel body = new Panel();
            body.Dock = DockStyle.Fill;
            body.AutoScroll = true;

            TableLayoutPanel form = new TableLayoutPanel();
            form.Dock = DockStyle.Top;
            form.AutoSize = true;
            form.ColumnCount = 2;
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            form.Padding = new Padding(0, 6, 0, 0);

            // Row 0: 卡号扫描行
            AddFormLabel(form, 0, "低频卡号：");
            FlowLayoutPanel cardRow = new FlowLayoutPanel();
            cardRow.Dock = DockStyle.Fill;
            cardRow.AutoSize = true;
            cardRow.WrapContents = true;
            cardRow.Margin = new Padding(0, 2, 0, 2);

            tbCardNum = new TextBox();
            tbCardNum.Font = CampusTheme.MonoFont;
            tbCardNum.Width = 122;
            tbCardNum.ReadOnly = true;
            tbCardNum.BackColor = Color.FromArgb(248, 250, 252);
            tbCardNum.TextChanged += delegate { OnCardNumberEdited(); UpdatePreview(); };

            btnReadCard = new Button();
            btnReadCard.Text = "获取卡号";
            btnReadCard.Width = 82;
            btnReadCard.Height = 26;
            CampusTheme.StylePrimaryButton(btnReadCard, CampusTheme.NavyAccent);
            btnReadCard.Click += BtnReadCard_Click;

            btnSimCard = new Button();
            btnSimCard.Text = "仿真新卡";
            btnSimCard.Width = 80;
            btnSimCard.Height = 26;
            CampusTheme.StyleSecondaryButton(btnSimCard);
            btnSimCard.Click += BtnSimCard_Click;

            chkManualCard = new CheckBox();
            chkManualCard.Text = "手输";
            chkManualCard.Font = CampusTheme.SmallFont;
            chkManualCard.AutoSize = true;
            chkManualCard.Margin = new Padding(4, 5, 0, 0);
            chkManualCard.CheckedChanged += delegate
            {
                tbCardNum.ReadOnly = !chkManualCard.Checked;
                tbCardNum.BackColor = chkManualCard.Checked ? Color.White : Color.FromArgb(248, 250, 252);
            };

            cardRow.Controls.Add(tbCardNum);
            cardRow.Controls.Add(btnReadCard);
            cardRow.Controls.Add(btnSimCard);
            cardRow.Controls.Add(chkManualCard);
            form.Controls.Add(cardRow, 1, 0);

            // Row 1: 读卡状态提示
            AddFormLabel(form, 1, "卡号状态：");
            lblCardStatus = new Label();
            lblCardStatus.Text = "请将EM4100/T5557低频卡放到感应区，点击【获取卡号】";
            lblCardStatus.Font = CampusTheme.SmallFont;
            lblCardStatus.ForeColor = CampusTheme.TextMuted;
            lblCardStatus.AutoSize = true;
            lblCardStatus.Margin = new Padding(3, 3, 3, 3);
            form.Controls.Add(lblCardStatus, 1, 1);

            // Row 2: 姓名
            AddFormLabel(form, 2, "持卡人姓名：");
            tbName = new TextBox();
            tbName.Font = CampusTheme.BodyFont;
            tbName.Width = 240;
            tbName.Margin = new Padding(3, 2, 3, 2);
            tbName.TextChanged += delegate { UpdatePreview(); };
            form.Controls.Add(tbName, 1, 2);

            // Row 3: 学号/工号
            AddFormLabel(form, 3, "学号 / 工号：");
            tbStuId = new TextBox();
            tbStuId.Font = CampusTheme.BodyFont;
            tbStuId.Width = 240;
            tbStuId.Margin = new Padding(3, 2, 3, 2);
            tbStuId.TextChanged += delegate { UpdatePreview(); };
            form.Controls.Add(tbStuId, 1, 3);

            // Row 4: 性别 & 身份类型
            AddFormLabel(form, 4, "性别与身份：");
            FlowLayoutPanel roleRow = new FlowLayoutPanel();
            roleRow.AutoSize = true;
            roleRow.Margin = new Padding(0);
            cbSex = new ComboBox();
            cbSex.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSex.Font = CampusTheme.BodyFont;
            cbSex.Width = 80;
            cbSex.Items.AddRange(new object[] { "男", "女" });
            cbSex.SelectedIndex = 0;

            cbRole = new ComboBox();
            cbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cbRole.Font = CampusTheme.BodyFont;
            cbRole.Width = 152;
            cbRole.Items.AddRange(CampusRoles.All);
            cbRole.SelectedIndex = 0;
            cbRole.SelectedIndexChanged += delegate { UpdatePreview(); };

            roleRow.Controls.Add(cbSex);
            roleRow.Controls.Add(cbRole);
            form.Controls.Add(roleRow, 1, 4);

            // Row 5: 院系/单位
            AddFormLabel(form, 5, "院系 / 单位：");
            cbDepartment = new ComboBox();
            cbDepartment.DropDownStyle = ComboBoxStyle.DropDown;
            cbDepartment.Font = CampusTheme.BodyFont;
            cbDepartment.Width = 240;
            cbDepartment.Margin = new Padding(3, 2, 3, 2);
            cbDepartment.Items.AddRange(CampusDepartments.All);
            cbDepartment.SelectedIndex = 0;
            cbDepartment.TextChanged += delegate { UpdatePreview(); };
            form.Controls.Add(cbDepartment, 1, 5);

            // Row 6: 授权门区 & 卡状态
            AddFormLabel(form, 6, "门区与状态：");
            FlowLayoutPanel zoneRow = new FlowLayoutPanel();
            zoneRow.AutoSize = true;
            zoneRow.Margin = new Padding(0);
            cbGateZone = new ComboBox();
            cbGateZone.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGateZone.Font = CampusTheme.BodyFont;
            cbGateZone.Width = 152;
            cbGateZone.Items.AddRange(CampusZones.All);
            cbGateZone.SelectedIndex = 0;
            cbGateZone.SelectedIndexChanged += delegate { UpdatePreview(); };

            cbStatus = new ComboBox();
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.Font = CampusTheme.BodyFont;
            cbStatus.Width = 80;
            cbStatus.Items.AddRange(CardStatuses.All);
            cbStatus.SelectedIndex = 0;
            cbStatus.SelectedIndexChanged += delegate { UpdatePreview(); };

            zoneRow.Controls.Add(cbGateZone);
            zoneRow.Controls.Add(cbStatus);
            form.Controls.Add(zoneRow, 1, 6);

            // Row 7: 有效期
            AddFormLabel(form, 7, "卡片有效期：");
            FlowLayoutPanel validRow = new FlowLayoutPanel();
            validRow.AutoSize = true;
            validRow.Margin = new Padding(0);
            dtpValidUntil = new DateTimePicker();
            dtpValidUntil.Format = DateTimePickerFormat.Short;
            dtpValidUntil.Font = CampusTheme.BodyFont;
            dtpValidUntil.Width = 130;
            dtpValidUntil.Value = DateTime.Today.AddYears(1);
            dtpValidUntil.Enabled = false;
            dtpValidUntil.ValueChanged += delegate { UpdatePreview(); };

            chkPermanent = new CheckBox();
            chkPermanent.Text = "长期有效";
            chkPermanent.Font = CampusTheme.BodyFont;
            chkPermanent.AutoSize = true;
            chkPermanent.Checked = true;
            chkPermanent.Margin = new Padding(8, 4, 0, 0);
            chkPermanent.CheckedChanged += delegate
            {
                dtpValidUntil.Enabled = !chkPermanent.Checked;
                UpdatePreview();
            };

            validRow.Controls.Add(dtpValidUntil);
            validRow.Controls.Add(chkPermanent);
            form.Controls.Add(validRow, 1, 7);

            // Row 8: 按钮操作行
            FlowLayoutPanel btnRow = new FlowLayoutPanel();
            btnRow.AutoSize = true;
            btnRow.Margin = new Padding(0, 6, 0, 2);

            btnSave = new Button();
            btnSave.Text = "注册 / 保存卡片";
            btnSave.Width = 128;
            btnSave.Height = 28;
            CampusTheme.StylePrimaryButton(btnSave, CampusTheme.SuccessGreen);
            btnSave.Click += BtnSave_Click;

            btnLoadExisting = new Button();
            btnLoadExisting.Text = "载入已有信息";
            btnLoadExisting.Width = 110;
            btnLoadExisting.Height = 28;
            btnLoadExisting.Enabled = false;
            CampusTheme.StyleSecondaryButton(btnLoadExisting);
            btnLoadExisting.Click += BtnLoadExisting_Click;

            btnClear = new Button();
            btnClear.Text = "清空表单";
            btnClear.Width = 88;
            btnClear.Height = 28;
            CampusTheme.StyleSecondaryButton(btnClear);
            btnClear.Click += delegate { ClearForm(); };

            btnRow.Controls.Add(btnSave);
            btnRow.Controls.Add(btnLoadExisting);
            btnRow.Controls.Add(btnClear);
            form.Controls.Add(btnRow, 1, 8);

            // Row 9: 结果反馈
            AddFormLabel(form, 9, "操作反馈：");
            lblSaveResult = new Label();
            lblSaveResult.Text = "待操作";
            lblSaveResult.Font = CampusTheme.BodyBoldFont;
            lblSaveResult.ForeColor = CampusTheme.TextMuted;
            lblSaveResult.AutoSize = true;
            lblSaveResult.Margin = new Padding(3, 6, 3, 3);
            form.Controls.Add(lblSaveResult, 1, 9);

            body.Controls.Add(form);
            card.Controls.Add(body);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Panel CreatePreviewCard()
        {
            Panel wrap = new Panel();
            wrap.BackColor = CampusTheme.CardBg;
            wrap.BorderStyle = BorderStyle.FixedSingle;
            wrap.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = "电子校园门禁卡实时预览";
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 24;

            pnlCardPreview = new Panel();
            pnlCardPreview.Dock = DockStyle.Fill;
            pnlCardPreview.BackColor = Color.Transparent;
            pnlCardPreview.Paint += PnlCardPreview_Paint;

            wrap.Controls.Add(pnlCardPreview);
            wrap.Controls.Add(lblTitle);
            return wrap;
        }

        private Panel CreateRecentPanel()
        {
            Panel wrap = new Panel();
            wrap.BackColor = CampusTheme.CardBg;
            wrap.BorderStyle = BorderStyle.FixedSingle;
            wrap.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = "最近注册卡片档案（双击可快速回填修改）";
            lblTitle.Font = CampusTheme.SectionFont;
            lblTitle.ForeColor = CampusTheme.NavyHeader;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 24;

            dgvRecent = new DataGridView();
            dgvRecent.Dock = DockStyle.Fill;
            CampusTheme.StyleDataGridView(dgvRecent);
            dgvRecent.Columns.Add("cardnum", "卡号");
            dgvRecent.Columns.Add("name", "姓名");
            dgvRecent.Columns.Add("stuid", "学号/工号");
            dgvRecent.Columns.Add("role", "身份");
            dgvRecent.Columns.Add("status", "状态");
            dgvRecent.CellDoubleClick += DgvRecent_CellDoubleClick;

            wrap.Controls.Add(dgvRecent);
            wrap.Controls.Add(lblTitle);
            return wrap;
        }

        private void AddFormLabel(TableLayoutPanel panel, int row, string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = CampusTheme.BodyFont;
            lbl.ForeColor = CampusTheme.TextPrimary;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(0, 5, 4, 2);
            panel.Controls.Add(lbl, 0, row);
        }

        private void BtnReadCard_Click(object sender, EventArgs e)
        {
            lblCardStatus.Text = "正在向 125K 阅读器发送寻卡请求...";
            lblCardStatus.ForeColor = CampusTheme.NavyAccent;

            string err;
            if (!_protocol.RequestSingleCard(CardReadSource.RegisterTab, out err))
            {
                lblCardStatus.Text = err;
                lblCardStatus.ForeColor = CampusTheme.DangerRed;
                RaiseStatus(err);
            }
        }

        private void BtnSimCard_Click(object sender, EventArgs e)
        {
            string randomCard = "000" + _rng.Next(1000000, 9999999).ToString();
            _protocol.SimulateNoCard = false;
            _protocol.SimulatedCardNumber = randomCard;

            if (_protocol.IsSimulationMode)
            {
                BtnReadCard_Click(sender, e);
            }
            else
            {
                tbCardNum.Text = randomCard;
                lblCardStatus.Text = "已填入测试卡号（当前为实机模式，仅预览填号）";
                lblCardStatus.ForeColor = CampusTheme.WarningAmber;
            }
        }

        public void HandleCardRead(CardReadEventArgs e)
        {
            if (e == null) return;

            if (!e.CardFound)
            {
                tbCardNum.Text = string.Empty;
                lblCardStatus.Text = e.RawMessage + "，请将卡片贴近125K天线区";
                lblCardStatus.ForeColor = CampusTheme.WarningAmber;
                btnLoadExisting.Enabled = false;
                return;
            }

            tbCardNum.Text = e.CardNumber;
            CheckExistingCardStatus(e.CardNumber);
        }

        private void OnCardNumberEdited()
        {
            string card = tbCardNum.Text.Trim();
            if (string.IsNullOrEmpty(card))
            {
                btnLoadExisting.Enabled = false;
                return;
            }
            if (GateProtocolService.IsValidCardNumber(card))
            {
                CheckExistingCardStatus(card);
            }
            else
            {
                lblCardStatus.Text = "卡号格式应为 6~12 位数字（EM4100标准为10位十进制）";
                lblCardStatus.ForeColor = CampusTheme.WarningAmber;
                btnLoadExisting.Enabled = false;
            }
        }

        private void CheckExistingCardStatus(string cardNumber)
        {
            CampusCardRecord existing = JsonFormat.FindCard(cardNumber);
            if (existing != null)
            {
                lblCardStatus.Text = string.Format("读卡成功：该卡已绑定【{0} / {1}】，点击【载入已有信息】或直接覆盖保存", existing.name, existing.stuid);
                lblCardStatus.ForeColor = CampusTheme.WarningAmber;
                btnLoadExisting.Enabled = true;
            }
            else
            {
                lblCardStatus.Text = string.Format("读卡成功：{0}（新卡，可填写校园身份信息注册）", cardNumber);
                lblCardStatus.ForeColor = CampusTheme.SuccessGreen;
                btnLoadExisting.Enabled = false;
            }
        }

        private void BtnLoadExisting_Click(object sender, EventArgs e)
        {
            CampusCardRecord rec = JsonFormat.FindCard(tbCardNum.Text.Trim());
            if (rec != null)
            {
                LoadRecordToForm(rec);
                lblSaveResult.Text = "已载入现有档案，修改后点击保存即可";
                lblSaveResult.ForeColor = CampusTheme.NavyAccent;
            }
        }

        public void LoadRecordToForm(CampusCardRecord rec)
        {
            if (rec == null) return;
            tbCardNum.Text = rec.cardnum;
            tbName.Text = rec.name;
            tbStuId.Text = rec.stuid;
            SelectComboValue(cbSex, rec.sex);
            SelectComboValue(cbRole, rec.role);
            cbDepartment.Text = rec.department;
            SelectComboValue(cbGateZone, rec.gateZone);
            SelectComboValue(cbStatus, rec.status);

            DateTime dt;
            if (!string.IsNullOrEmpty(rec.validUntil) && rec.validUntil != "长期有效" && DateTime.TryParse(rec.validUntil, out dt))
            {
                chkPermanent.Checked = false;
                dtpValidUntil.Value = dt;
            }
            else
            {
                chkPermanent.Checked = true;
            }
            UpdatePreview();
        }

        private static void SelectComboValue(ComboBox cb, string val)
        {
            if (cb.Items.Count == 0) return;
            int idx = cb.Items.IndexOf(val);
            cb.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string card = tbCardNum.Text.Trim();
            if (!GateProtocolService.IsValidCardNumber(card))
            {
                lblSaveResult.Text = "注册失败：请先获取有效125K卡号（6~12位数字）！";
                lblSaveResult.ForeColor = CampusTheme.DangerRed;
                return;
            }

            if (string.IsNullOrEmpty(tbName.Text.Trim()) || string.IsNullOrEmpty(tbStuId.Text.Trim()))
            {
                lblSaveResult.Text = "注册失败：姓名与学号/工号不能为空！";
                lblSaveResult.ForeColor = CampusTheme.DangerRed;
                return;
            }

            CampusCardRecord existing = JsonFormat.FindCard(card);
            if (existing != null)
            {
                DialogResult ans = MessageBox.Show(
                    string.Format("卡号 {0} 已注册给【{1}（{2}）】。\r\n是否使用当前填写的信息覆盖更新该卡片？", card, existing.name, existing.stuid),
                    "卡片重复确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (ans != DialogResult.Yes)
                {
                    return;
                }
            }

            CampusCardRecord record = new CampusCardRecord
            {
                cardnum = card,
                name = tbName.Text.Trim(),
                stuid = tbStuId.Text.Trim(),
                sex = cbSex.Text,
                role = cbRole.Text,
                department = string.IsNullOrEmpty(cbDepartment.Text.Trim()) ? CampusDepartments.All[0] : cbDepartment.Text.Trim(),
                gateZone = cbGateZone.Text,
                status = cbStatus.Text,
                validUntil = chkPermanent.Checked ? "长期有效" : dtpValidUntil.Value.ToString("yyyy-MM-dd")
            };

            if (JsonFormat.UpsertCard(record))
            {
                lblSaveResult.Text = string.Format("{0}成功：{1}（{2}）", existing != null ? "更新" : "注册", record.name, record.cardnum);
                lblSaveResult.ForeColor = CampusTheme.SuccessGreen;
                CheckExistingCardStatus(record.cardnum);
                RefreshRecentList();
                UpdatePreview();
                RaiseStatus(lblSaveResult.Text);
                if (CardsChanged != null)
                {
                    CardsChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                lblSaveResult.Text = "保存失败：无法写入 db.json";
                lblSaveResult.ForeColor = CampusTheme.DangerRed;
            }
        }

        private void ClearForm()
        {
            tbCardNum.Text = string.Empty;
            tbName.Text = string.Empty;
            tbStuId.Text = string.Empty;
            cbSex.SelectedIndex = 0;
            cbRole.SelectedIndex = 0;
            cbDepartment.SelectedIndex = 0;
            cbGateZone.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
            chkPermanent.Checked = true;
            btnLoadExisting.Enabled = false;
            lblCardStatus.Text = "请将低频卡放到感应区，点击【获取卡号】";
            lblCardStatus.ForeColor = CampusTheme.TextMuted;
            lblSaveResult.Text = "表单已重置";
            lblSaveResult.ForeColor = CampusTheme.TextMuted;
            UpdatePreview();
        }

        public void RefreshRecentList()
        {
            List<CampusCardRecord> cards = JsonFormat.LoadCards();
            cards.Sort(delegate(CampusCardRecord a, CampusCardRecord b)
            {
                return string.Compare(b.updatedAt, a.updatedAt, StringComparison.Ordinal);
            });

            dgvRecent.Rows.Clear();
            int count = Math.Min(cards.Count, 8);
            for (int i = 0; i < count; i++)
            {
                CampusCardRecord c = cards[i];
                int rowIdx = dgvRecent.Rows.Add(c.cardnum, c.name, c.stuid, c.role, c.status);
                DataGridViewRow row = dgvRecent.Rows[rowIdx];
                if (c.status == CardStatuses.Lost)
                {
                    row.DefaultCellStyle.ForeColor = CampusTheme.WarningAmber;
                }
                else if (c.status == CardStatuses.Disabled)
                {
                    row.DefaultCellStyle.ForeColor = CampusTheme.DangerRed;
                }
            }
        }

        private void DgvRecent_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string cardNum = Convert.ToString(dgvRecent.Rows[e.RowIndex].Cells[0].Value);
            CampusCardRecord rec = JsonFormat.FindCard(cardNum);
            if (rec != null)
            {
                LoadRecordToForm(rec);
                CheckExistingCardStatus(cardNum);
                lblSaveResult.Text = "已载入选中卡片信息";
                lblSaveResult.ForeColor = CampusTheme.NavyAccent;
            }
        }

        private void UpdatePreview()
        {
            if (pnlCardPreview != null)
            {
                pnlCardPreview.Invalidate();
            }
        }

        private void PnlCardPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Math.Max(260, pnlCardPreview.ClientSize.Width - 16);
            int h = Math.Max(140, pnlCardPreview.ClientSize.Height - 10);
            Rectangle cardRect = new Rectangle(8, 4, w, h);

            Color c1 = Color.FromArgb(24, 58, 110);
            Color c2 = Color.FromArgb(46, 108, 185);
            string statusText = cbStatus != null && cbStatus.Text != "" ? cbStatus.Text : CardStatuses.Normal;
            Color badgeColor = CampusTheme.SuccessGreen;

            if (statusText == CardStatuses.Lost)
            {
                c1 = Color.FromArgb(146, 64, 14);
                c2 = Color.FromArgb(217, 119, 6);
                badgeColor = CampusTheme.WarningAmber;
            }
            else if (statusText == CardStatuses.Disabled)
            {
                c1 = Color.FromArgb(127, 29, 29);
                c2 = Color.FromArgb(185, 28, 28);
                badgeColor = CampusTheme.DangerRed;
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(cardRect, c1, c2, LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(brush, cardRect);
            }

            using (Pen borderPen = new Pen(Color.FromArgb(180, 255, 255, 255), 1.5f))
            {
                g.DrawRectangle(borderPen, cardRect);
            }

            // RFID chip icon decoration
            Rectangle chipRect = new Rectangle(cardRect.X + 20, cardRect.Y + 42, 34, 24);
            using (SolidBrush goldBrush = new SolidBrush(Color.FromArgb(245, 208, 102)))
            {
                g.FillRectangle(goldBrush, chipRect);
            }
            using (Pen chipPen = new Pen(Color.FromArgb(140, 100, 20), 1f))
            {
                g.DrawRectangle(chipPen, chipRect);
                g.DrawLine(chipPen, chipRect.X + 11, chipRect.Y, chipRect.X + 11, chipRect.Bottom);
                g.DrawLine(chipPen, chipRect.X + 23, chipRect.Y, chipRect.X + 23, chipRect.Bottom);
                g.DrawLine(chipPen, chipRect.X, chipRect.Y + 12, chipRect.Right, chipRect.Y + 12);
            }

            using (Font hdrFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold))
            using (Font subFont = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular))
            using (Font nameFont = new Font("Microsoft YaHei UI", 12.5F, FontStyle.Bold))
            using (Font numFont = new Font("Consolas", 11F, FontStyle.Bold))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            using (SolidBrush softWhite = new SolidBrush(Color.FromArgb(215, 230, 250)))
            {
                g.DrawString("CAMPUS RFID PASS  ·  125KHz 校园门禁卡", hdrFont, whiteBrush, cardRect.X + 18, cardRect.Y + 12);

                // Status badge
                Rectangle badgeRect = new Rectangle(cardRect.Right - 72, cardRect.Y + 10, 56, 22);
                using (SolidBrush bBrush = new SolidBrush(badgeColor))
                {
                    g.FillRectangle(bBrush, badgeRect);
                }
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(statusText, subFont, whiteBrush, badgeRect, sf);

                string cardNumDisp = string.IsNullOrEmpty(tbCardNum.Text.Trim()) ? "---- ---- --" : tbCardNum.Text.Trim();
                string nameDisp = string.IsNullOrEmpty(tbName.Text.Trim()) ? "未填写姓名" : tbName.Text.Trim();
                string stuDisp = string.IsNullOrEmpty(tbStuId.Text.Trim()) ? "未填写学号/工号" : tbStuId.Text.Trim();
                string deptDisp = string.IsNullOrEmpty(cbDepartment.Text.Trim()) ? "物联网工程系" : cbDepartment.Text.Trim();
                string roleDisp = string.IsNullOrEmpty(cbRole.Text) ? CampusRoles.Undergraduate : cbRole.Text;
                string zoneDisp = string.IsNullOrEmpty(cbGateZone.Text) ? CampusZones.All[0] : cbGateZone.Text;
                string validDisp = chkPermanent != null && chkPermanent.Checked ? "长期有效" : dtpValidUntil.Value.ToString("yyyy-MM-dd");

                g.DrawString("NO. " + cardNumDisp, numFont, softWhite, cardRect.X + 66, cardRect.Y + 44);
                g.DrawString(nameDisp + "  (" + roleDisp + ")", nameFont, whiteBrush, cardRect.X + 20, cardRect.Y + 76);
                g.DrawString("学号/工号：" + stuDisp, subFont, softWhite, cardRect.X + 20, cardRect.Y + 106);
                g.DrawString("院系/单位：" + deptDisp, subFont, softWhite, cardRect.X + 20, cardRect.Y + 126);
                g.DrawString("授权门区：" + zoneDisp + "   |   有效期：" + validDisp, subFont, softWhite, cardRect.X + 20, cardRect.Y + 146);
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
