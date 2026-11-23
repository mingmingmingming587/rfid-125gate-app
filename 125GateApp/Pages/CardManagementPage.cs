using _125GateApp.Models;
using _125GateApp.Services;
using _125GateApp.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _125GateApp.Pages
{
    public class CardManagementPage : UserControl
    {
        private Label lblTotalCount;
        private Label lblNormalCount;
        private Label lblLostCount;
        private Label lblDisabledCount;

        private TextBox tbSearch;
        private ComboBox cbFilterRole;
        private ComboBox cbFilterStatus;
        private Button btnRefresh;
        private Button btnImportDemo;
        private Button btnExportCsv;

        private DataGridView dgvCards;

        private TextBox tbEditCard;
        private TextBox tbEditName;
        private TextBox tbEditStuId;
        private ComboBox cbEditSex;
        private ComboBox cbEditRole;
        private ComboBox cbEditDept;
        private ComboBox cbEditZone;
        private ComboBox cbEditStatus;
        private TextBox tbEditValid;

        private Button btnSaveEdit;
        private Button btnMarkLost;
        private Button btnMarkNormal;
        private Button btnMarkDisabled;
        private Button btnDelete;

        private List<CampusCardRecord> _allCards = new List<CampusCardRecord>();

        public event EventHandler CardsChanged;
        public event StatusTextEventHandler StatusMessage;

        public CardManagementPage()
        {
            InitializeLayout();
            RefreshGrid();
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
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(root);

            // Row 0: KPI 与 搜索栏
            Panel topPanel = CreateTopFilterPanel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Margin = new Padding(0, 0, 0, 8);
            root.Controls.Add(topPanel, 0, 0);

            // Row 1: 表格区
            Panel gridPanel = new Panel();
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.BackColor = CampusTheme.CardBg;
            gridPanel.BorderStyle = BorderStyle.FixedSingle;
            gridPanel.Padding = new Padding(8);

            dgvCards = new DataGridView();
            dgvCards.Dock = DockStyle.Fill;
            CampusTheme.StyleDataGridView(dgvCards);
            dgvCards.Columns.Add("cardnum", "卡号");
            dgvCards.Columns.Add("name", "姓名");
            dgvCards.Columns.Add("stuid", "学号/工号");
            dgvCards.Columns.Add("sex", "性别");
            dgvCards.Columns.Add("role", "身份类型");
            dgvCards.Columns.Add("department", "院系/单位");
            dgvCards.Columns.Add("gateZone", "授权门区");
            dgvCards.Columns.Add("status", "状态");
            dgvCards.Columns.Add("validUntil", "有效期");
            dgvCards.Columns.Add("updatedAt", "更新时间");
            dgvCards.SelectionChanged += DgvCards_SelectionChanged;

            gridPanel.Controls.Add(dgvCards);
            root.Controls.Add(gridPanel, 0, 1);

            // Row 2: 底部编辑与状态操作栏
            Panel editPanel = CreateBottomEditorPanel();
            editPanel.Dock = DockStyle.Bottom;
            editPanel.Margin = new Padding(0, 8, 0, 0);
            root.Controls.Add(editPanel, 0, 2);
        }

        private Panel CreateTopFilterPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = CampusTheme.CardBg;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Padding = new Padding(10, 8, 10, 8);
            panel.AutoSize = true;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.AutoSize = true;
            layout.ColumnCount = 1;
            layout.RowCount = 2;

            // Row 0: 汇总标签
            FlowLayoutPanel statsRow = new FlowLayoutPanel();
            statsRow.AutoSize = true;
            statsRow.Margin = new Padding(0, 0, 0, 6);

            lblTotalCount = CreateStatPill(statsRow, "总注册卡：0", CampusTheme.NavyHeader);
            lblNormalCount = CreateStatPill(statsRow, "正常：0", CampusTheme.SuccessGreen);
            lblLostCount = CreateStatPill(statsRow, "挂失：0", CampusTheme.WarningAmber);
            lblDisabledCount = CreateStatPill(statsRow, "禁用：0", CampusTheme.DangerRed);
            layout.Controls.Add(statsRow, 0, 0);

            // Row 1: 搜索筛选与导出导入
            FlowLayoutPanel filterRow = new FlowLayoutPanel();
            filterRow.AutoSize = true;
            filterRow.WrapContents = true;

            Label lblSearch = new Label();
            lblSearch.Text = "关键词检索:";
            lblSearch.Font = CampusTheme.BodyFont;
            lblSearch.AutoSize = true;
            lblSearch.Margin = new Padding(0, 6, 4, 0);

            tbSearch = new TextBox();
            tbSearch.Font = CampusTheme.BodyFont;
            tbSearch.Width = 180;
            tbSearch.Margin = new Padding(0, 3, 10, 0);
            tbSearch.TextChanged += delegate { ApplyFilter(); };

            Label lblRole = new Label();
            lblRole.Text = "身份:";
            lblRole.Font = CampusTheme.BodyFont;
            lblRole.AutoSize = true;
            lblRole.Margin = new Padding(0, 6, 4, 0);

            cbFilterRole = new ComboBox();
            cbFilterRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilterRole.Font = CampusTheme.BodyFont;
            cbFilterRole.Width = 96;
            cbFilterRole.Margin = new Padding(0, 3, 10, 0);
            cbFilterRole.Items.Add("全部");
            cbFilterRole.Items.AddRange(CampusRoles.All);
            cbFilterRole.SelectedIndex = 0;
            cbFilterRole.SelectedIndexChanged += delegate { ApplyFilter(); };

            Label lblStat = new Label();
            lblStat.Text = "状态:";
            lblStat.Font = CampusTheme.BodyFont;
            lblStat.AutoSize = true;
            lblStat.Margin = new Padding(0, 6, 4, 0);

            cbFilterStatus = new ComboBox();
            cbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilterStatus.Font = CampusTheme.BodyFont;
            cbFilterStatus.Width = 86;
            cbFilterStatus.Margin = new Padding(0, 3, 10, 0);
            cbFilterStatus.Items.Add("全部");
            cbFilterStatus.Items.AddRange(CardStatuses.All);
            cbFilterStatus.SelectedIndex = 0;
            cbFilterStatus.SelectedIndexChanged += delegate { ApplyFilter(); };

            btnRefresh = new Button();
            btnRefresh.Text = "刷新列表";
            btnRefresh.Width = 84;
            btnRefresh.Height = 27;
            CampusTheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += delegate { RefreshGrid(); };

            btnImportDemo = new Button();
            btnImportDemo.Text = "导入校园测试数据";
            btnImportDemo.Width = 132;
            btnImportDemo.Height = 27;
            CampusTheme.StylePrimaryButton(btnImportDemo, CampusTheme.NavyAccent);
            btnImportDemo.Click += BtnImportDemo_Click;

            btnExportCsv = new Button();
            btnExportCsv.Text = "导出人员 CSV";
            btnExportCsv.Width = 108;
            btnExportCsv.Height = 27;
            CampusTheme.StyleSecondaryButton(btnExportCsv);
            btnExportCsv.Click += BtnExportCsv_Click;

            filterRow.Controls.Add(lblSearch);
            filterRow.Controls.Add(tbSearch);
            filterRow.Controls.Add(lblRole);
            filterRow.Controls.Add(cbFilterRole);
            filterRow.Controls.Add(lblStat);
            filterRow.Controls.Add(cbFilterStatus);
            filterRow.Controls.Add(btnRefresh);
            filterRow.Controls.Add(btnImportDemo);
            filterRow.Controls.Add(btnExportCsv);
            layout.Controls.Add(filterRow, 0, 1);

            panel.Controls.Add(layout);
            return panel;
        }

        private static Label CreateStatPill(FlowLayoutPanel parent, string text, Color color)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = CampusTheme.BodyBoldFont;
            lbl.ForeColor = color;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(0, 0, 18, 0);
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Panel CreateBottomEditorPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = CampusTheme.CardBg;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Padding = new Padding(10, 8, 10, 8);
            panel.AutoSize = true;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.AutoSize = true;
            layout.ColumnCount = 1;
            layout.RowCount = 2;

            // Row 0: 编辑字段行 (分两行排布防止溢出)
            TableLayoutPanel fieldsGrid = new TableLayoutPanel();
            fieldsGrid.AutoSize = true;
            fieldsGrid.ColumnCount = 1;
            fieldsGrid.RowCount = 2;

            FlowLayoutPanel fieldsRow1 = new FlowLayoutPanel();
            fieldsRow1.AutoSize = true;
            fieldsRow1.WrapContents = false;

            tbEditCard = AddLabeledTextBox(fieldsRow1, "卡号:", 105, true);
            tbEditName = AddLabeledTextBox(fieldsRow1, "姓名:", 96, false);
            tbEditStuId = AddLabeledTextBox(fieldsRow1, "学号/工号:", 110, false);
            cbEditSex = AddLabeledCombo(fieldsRow1, "性别:", 64, new string[] { "男", "女" }, false);
            cbEditRole = AddLabeledCombo(fieldsRow1, "身份:", 96, CampusRoles.All, false);

            FlowLayoutPanel fieldsRow2 = new FlowLayoutPanel();
            fieldsRow2.AutoSize = true;
            fieldsRow2.WrapContents = false;

            cbEditDept = AddLabeledCombo(fieldsRow2, "院系:", 160, CampusDepartments.All, true);
            cbEditZone = AddLabeledCombo(fieldsRow2, "门区:", 148, CampusZones.All, false);
            cbEditStatus = AddLabeledCombo(fieldsRow2, "状态:", 76, CardStatuses.All, false);
            tbEditValid = AddLabeledTextBox(fieldsRow2, "有效期:", 104, false);

            fieldsGrid.Controls.Add(fieldsRow1, 0, 0);
            fieldsGrid.Controls.Add(fieldsRow2, 0, 1);

            layout.Controls.Add(fieldsGrid, 0, 0);

            // Row 1: 操作按钮行
            FlowLayoutPanel actRow = new FlowLayoutPanel();
            actRow.AutoSize = true;
            actRow.Margin = new Padding(0, 6, 0, 0);

            btnSaveEdit = new Button();
            btnSaveEdit.Text = "保存信息修改";
            btnSaveEdit.Width = 112;
            btnSaveEdit.Height = 28;
            CampusTheme.StylePrimaryButton(btnSaveEdit, CampusTheme.SuccessGreen);
            btnSaveEdit.Click += BtnSaveEdit_Click;

            btnMarkLost = new Button();
            btnMarkLost.Text = "设为挂失";
            btnMarkLost.Width = 86;
            btnMarkLost.Height = 28;
            CampusTheme.StylePrimaryButton(btnMarkLost, CampusTheme.WarningAmber);
            btnMarkLost.Click += delegate { ChangeSelectedStatus(CardStatuses.Lost); };

            btnMarkNormal = new Button();
            btnMarkNormal.Text = "恢复正常";
            btnMarkNormal.Width = 86;
            btnMarkNormal.Height = 28;
            CampusTheme.StylePrimaryButton(btnMarkNormal, CampusTheme.NavyAccent);
            btnMarkNormal.Click += delegate { ChangeSelectedStatus(CardStatuses.Normal); };

            btnMarkDisabled = new Button();
            btnMarkDisabled.Text = "禁用卡片";
            btnMarkDisabled.Width = 86;
            btnMarkDisabled.Height = 28;
            CampusTheme.StyleSecondaryButton(btnMarkDisabled);
            btnMarkDisabled.Click += delegate { ChangeSelectedStatus(CardStatuses.Disabled); };

            btnDelete = new Button();
            btnDelete.Text = "注销删除";
            btnDelete.Width = 86;
            btnDelete.Height = 28;
            CampusTheme.StylePrimaryButton(btnDelete, CampusTheme.DangerRed);
            btnDelete.Click += BtnDelete_Click;

            actRow.Controls.Add(btnSaveEdit);
            actRow.Controls.Add(btnMarkLost);
            actRow.Controls.Add(btnMarkNormal);
            actRow.Controls.Add(btnMarkDisabled);
            actRow.Controls.Add(btnDelete);
            layout.Controls.Add(actRow, 0, 1);

            panel.Controls.Add(layout);
            return panel;
        }

        private static TextBox AddLabeledTextBox(FlowLayoutPanel parent, string labelText, int width, bool readOnly)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Font = CampusTheme.SmallFont;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(2, 6, 2, 0);

            TextBox tb = new TextBox();
            tb.Font = CampusTheme.BodyFont;
            tb.Width = width;
            tb.ReadOnly = readOnly;
            if (readOnly) tb.BackColor = Color.FromArgb(248, 250, 252);
            tb.Margin = new Padding(0, 2, 8, 2);

            parent.Controls.Add(lbl);
            parent.Controls.Add(tb);
            return tb;
        }

        private static ComboBox AddLabeledCombo(FlowLayoutPanel parent, string labelText, int width, string[] items, bool editable)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Font = CampusTheme.SmallFont;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(2, 6, 2, 0);

            ComboBox cb = new ComboBox();
            cb.DropDownStyle = editable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            cb.Font = CampusTheme.BodyFont;
            cb.Width = width;
            cb.Items.AddRange(items);
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
            cb.Margin = new Padding(0, 2, 8, 2);

            parent.Controls.Add(lbl);
            parent.Controls.Add(cb);
            return cb;
        }

        public void RefreshGrid()
        {
            _allCards = JsonFormat.LoadCards();
            UpdateSummaryCounts();
            ApplyFilter();
        }

        private void UpdateSummaryCounts()
        {
            int total = _allCards.Count;
            int normal = 0, lost = 0, disabled = 0;
            for (int i = 0; i < _allCards.Count; i++)
            {
                string st = _allCards[i].status;
                if (st == CardStatuses.Normal) normal++;
                else if (st == CardStatuses.Lost) lost++;
                else if (st == CardStatuses.Disabled) disabled++;
            }
            lblTotalCount.Text = "总注册卡：" + total;
            lblNormalCount.Text = "正常：" + normal;
            lblLostCount.Text = "挂失：" + lost;
            lblDisabledCount.Text = "禁用：" + disabled;
        }

        private void ApplyFilter()
        {
            string kw = tbSearch.Text.Trim().ToLower();
            string roleFilter = cbFilterRole.Text;
            string statusFilter = cbFilterStatus.Text;

            dgvCards.Rows.Clear();
            for (int i = 0; i < _allCards.Count; i++)
            {
                CampusCardRecord c = _allCards[i];
                if (roleFilter != "全部" && c.role != roleFilter) continue;
                if (statusFilter != "全部" && c.status != statusFilter) continue;

                if (!string.IsNullOrEmpty(kw))
                {
                    bool match = (c.cardnum != null && c.cardnum.ToLower().Contains(kw))
                        || (c.name != null && c.name.ToLower().Contains(kw))
                        || (c.stuid != null && c.stuid.ToLower().Contains(kw))
                        || (c.department != null && c.department.ToLower().Contains(kw));
                    if (!match) continue;
                }

                int rowIdx = dgvCards.Rows.Add(
                    c.cardnum, c.name, c.stuid, c.sex, c.role,
                    c.department, c.gateZone, c.status, c.validUntil, c.updatedAt);
                DataGridViewRow row = dgvCards.Rows[rowIdx];
                if (c.status == CardStatuses.Lost)
                {
                    row.DefaultCellStyle.BackColor = CampusTheme.WarningSoft;
                    row.DefaultCellStyle.ForeColor = CampusTheme.WarningAmber;
                }
                else if (c.status == CardStatuses.Disabled)
                {
                    row.DefaultCellStyle.BackColor = CampusTheme.DangerSoft;
                    row.DefaultCellStyle.ForeColor = CampusTheme.DangerRed;
                }
            }
        }

        private void DgvCards_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCards.SelectedRows.Count == 0) return;
            string cardNum = Convert.ToString(dgvCards.SelectedRows[0].Cells[0].Value);
            CampusCardRecord rec = JsonFormat.FindCard(cardNum);
            if (rec == null) return;

            tbEditCard.Text = rec.cardnum;
            tbEditName.Text = rec.name;
            tbEditStuId.Text = rec.stuid;
            SelectCombo(cbEditSex, rec.sex);
            SelectCombo(cbEditRole, rec.role);
            cbEditDept.Text = rec.department;
            SelectCombo(cbEditZone, rec.gateZone);
            SelectCombo(cbEditStatus, rec.status);
            tbEditValid.Text = rec.validUntil;
        }

        private static void SelectCombo(ComboBox cb, string val)
        {
            int idx = cb.Items.IndexOf(val);
            cb.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void BtnSaveEdit_Click(object sender, EventArgs e)
        {
            string cardNum = tbEditCard.Text.Trim();
            if (string.IsNullOrEmpty(cardNum))
            {
                MessageBox.Show("请先在表格中选中一条人员卡记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrEmpty(tbEditName.Text.Trim()) || string.IsNullOrEmpty(tbEditStuId.Text.Trim()))
            {
                MessageBox.Show("姓名和学号/工号不能为空。", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CampusCardRecord rec = new CampusCardRecord
            {
                cardnum = cardNum,
                name = tbEditName.Text.Trim(),
                stuid = tbEditStuId.Text.Trim(),
                sex = cbEditSex.Text,
                role = cbEditRole.Text,
                department = string.IsNullOrEmpty(cbEditDept.Text.Trim()) ? CampusDepartments.All[0] : cbEditDept.Text.Trim(),
                gateZone = cbEditZone.Text,
                status = cbEditStatus.Text,
                validUntil = string.IsNullOrEmpty(tbEditValid.Text.Trim()) ? "长期有效" : tbEditValid.Text.Trim()
            };

            if (JsonFormat.UpsertCard(rec))
            {
                RefreshGrid();
                NotifyCardsChanged(string.Format("已更新人员档案：{0}（{1}）", rec.name, rec.cardnum));
            }
        }

        private void ChangeSelectedStatus(string newStatus)
        {
            string cardNum = tbEditCard.Text.Trim();
            if (string.IsNullOrEmpty(cardNum))
            {
                MessageBox.Show("请先选中一张门禁卡。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (JsonFormat.UpdateCardStatus(cardNum, newStatus))
            {
                RefreshGrid();
                NotifyCardsChanged(string.Format("卡号 {0} 状态已变更为【{1}】", cardNum, newStatus));
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            string cardNum = tbEditCard.Text.Trim();
            if (string.IsNullOrEmpty(cardNum))
            {
                MessageBox.Show("请先选中要注销的记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult res = MessageBox.Show(
                string.Format("确定要注销并删除卡号 {0}（{1}）吗？", cardNum, tbEditName.Text),
                "确认注销",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            if (JsonFormat.DeleteCard(cardNum))
            {
                tbEditCard.Text = string.Empty;
                tbEditName.Text = string.Empty;
                tbEditStuId.Text = string.Empty;
                RefreshGrid();
                NotifyCardsChanged("已注销卡片：" + cardNum);
            }
        }

        private void BtnImportDemo_Click(object sender, EventArgs e)
        {
            int added = JsonFormat.ImportDemoCampusCards();
            RefreshGrid();
            string msg = added > 0
                ? string.Format("已导入 {0} 条校园演示测试卡（含学生、教师、挂失卡与禁用卡）。", added)
                : "校园演示卡已全部存在，未重复导入。";
            NotifyCardsChanged(msg);
            MessageBox.Show(msg, "导入完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV 文件 (*.csv)|*.csv";
                dlg.FileName = "校园门禁人员档案_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    JsonFormat.ExportCardsCsv(dlg.FileName, _allCards);
                    NotifyCardsChanged("已导出人员档案 CSV：" + dlg.FileName);
                    MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void NotifyCardsChanged(string msg)
        {
            if (StatusMessage != null) StatusMessage(this, msg);
            if (CardsChanged != null) CardsChanged(this, EventArgs.Empty);
        }
    }
}
