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
    public class AccessLogsPage : UserControl
    {
        private Label lblTodayCount;
        private Label lblTotalSwipes;
        private Label lblGrantCount;
        private Label lblDenyCount;
        private Label lblPassRate;

        private Panel pnlChart;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private ComboBox cbFilterResult;
        private TextBox tbSearch;
        private DataGridView dgvLogs;

        private List<AccessLogEntry> _allLogs = new List<AccessLogEntry>();
        private List<AccessLogEntry> _filteredLogs = new List<AccessLogEntry>();

        public event StatusTextEventHandler StatusMessage;

        public AccessLogsPage()
        {
            InitializeLayout();
            RefreshData();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = CampusTheme.PageBg;
            this.Padding = new Padding(10);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 152F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(root);

            // Row 0: 5个统计KPI卡片
            TableLayoutPanel kpiRow = new TableLayoutPanel();
            kpiRow.Dock = DockStyle.Fill;
            kpiRow.ColumnCount = 5;
            kpiRow.RowCount = 1;
            for (int i = 0; i < 5; i++)
            {
                kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }
            lblTodayCount = CreateKpiCard(kpiRow, 0, "今日刷卡人次", CampusTheme.NavyAccent);
            lblTotalSwipes = CreateKpiCard(kpiRow, 1, "累计事件总数", CampusTheme.NavyHeader);
            lblGrantCount = CreateKpiCard(kpiRow, 2, "合法放行次数", CampusTheme.SuccessGreen);
            lblDenyCount = CreateKpiCard(kpiRow, 3, "异常拦截报警", CampusTheme.DangerRed);
            lblPassRate = CreateKpiCard(kpiRow, 4, "刷卡通过率", Color.FromArgb(124, 58, 237));
            root.Controls.Add(kpiRow, 0, 0);

            // Row 1: GDI+ 统计图表卡片
            Panel chartWrap = new Panel();
            chartWrap.Dock = DockStyle.Fill;
            chartWrap.BackColor = CampusTheme.CardBg;
            chartWrap.BorderStyle = BorderStyle.FixedSingle;
            chartWrap.Padding = new Padding(8, 4, 8, 4);
            chartWrap.Margin = new Padding(0, 6, 0, 6);

            pnlChart = new Panel();
            pnlChart.Dock = DockStyle.Fill;
            pnlChart.Paint += PnlChart_Paint;
            chartWrap.Controls.Add(pnlChart);
            root.Controls.Add(chartWrap, 0, 1);

            // Row 2: 筛选工具条
            Panel filterBar = CreateFilterBar();
            filterBar.Dock = DockStyle.Top;
            filterBar.Margin = new Padding(0, 0, 0, 6);
            root.Controls.Add(filterBar, 0, 2);

            // Row 3: 历史日志列表
            Panel gridWrap = new Panel();
            gridWrap.Dock = DockStyle.Fill;
            gridWrap.BackColor = CampusTheme.CardBg;
            gridWrap.BorderStyle = BorderStyle.FixedSingle;
            gridWrap.Padding = new Padding(8);

            dgvLogs = new DataGridView();
            dgvLogs.Dock = DockStyle.Fill;
            CampusTheme.StyleDataGridView(dgvLogs);
            dgvLogs.Columns.Add("timestamp", "时间");
            dgvLogs.Columns.Add("cardnum", "卡号");
            dgvLogs.Columns.Add("name", "姓名");
            dgvLogs.Columns.Add("stuid", "学号/工号");
            dgvLogs.Columns.Add("role", "身份");
            dgvLogs.Columns.Add("department", "院系/位置");
            dgvLogs.Columns.Add("eventType", "事件类型");
            dgvLogs.Columns.Add("result", "判定结果");
            dgvLogs.Columns.Add("source", "触发来源");
            dgvLogs.Columns.Add("detail", "详情");
            gridWrap.Controls.Add(dgvLogs);
            root.Controls.Add(gridWrap, 0, 3);
        }

        private static Label CreateKpiCard(TableLayoutPanel parent, int col, string title, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = CampusTheme.CardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Margin = col < 4 ? new Padding(0, 0, 6, 0) : new Padding(0);
            card.Padding = new Padding(10, 8, 10, 6);

            Panel stripe = new Panel();
            stripe.Dock = DockStyle.Left;
            stripe.Width = 4;
            stripe.BackColor = accent;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = CampusTheme.SmallFont;
            lblTitle.ForeColor = CampusTheme.TextMuted;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 18;

            Label lblValue = new Label();
            lblValue.Text = "0";
            lblValue.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
            lblValue.ForeColor = accent;
            lblValue.Dock = DockStyle.Fill;
            lblValue.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(stripe);
            parent.Controls.Add(card, col, 0);
            return lblValue;
        }

        private Panel CreateFilterBar()
        {
            Panel bar = new Panel();
            bar.BackColor = CampusTheme.CardBg;
            bar.BorderStyle = BorderStyle.FixedSingle;
            bar.Padding = new Padding(8, 5, 8, 5);
            bar.AutoSize = true;

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoSize = true;
            flow.WrapContents = true;

            Label lblDate = new Label();
            lblDate.Text = "日期范围:";
            lblDate.Font = CampusTheme.BodyFont;
            lblDate.AutoSize = true;
            lblDate.Margin = new Padding(0, 6, 4, 0);

            dtpStart = new DateTimePicker();
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Width = 96;
            dtpStart.Value = DateTime.Today.AddDays(-7);

            Label lblTo = new Label();
            lblTo.Text = "至";
            lblTo.Font = CampusTheme.BodyFont;
            lblTo.AutoSize = true;
            lblTo.Margin = new Padding(2, 6, 2, 0);

            dtpEnd = new DateTimePicker();
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Width = 96;
            dtpEnd.Value = DateTime.Today;

            Label lblRes = new Label();
            lblRes.Text = "结果:";
            lblRes.Font = CampusTheme.BodyFont;
            lblRes.AutoSize = true;
            lblRes.Margin = new Padding(6, 6, 3, 0);

            cbFilterResult = new ComboBox();
            cbFilterResult.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilterResult.Font = CampusTheme.BodyFont;
            cbFilterResult.Width = 96;
            cbFilterResult.Items.AddRange(new object[] { "全部", "允许通行", "拒绝并报警", "设备动作" });
            cbFilterResult.SelectedIndex = 0;

            Label lblKw = new Label();
            lblKw.Text = "关键词:";
            lblKw.Font = CampusTheme.BodyFont;
            lblKw.AutoSize = true;
            lblKw.Margin = new Padding(6, 6, 3, 0);

            tbSearch = new TextBox();
            tbSearch.Font = CampusTheme.BodyFont;
            tbSearch.Width = 108;

            Button btnQuery = new Button();
            btnQuery.Text = "筛选";
            btnQuery.Width = 60;
            btnQuery.Height = 26;
            CampusTheme.StylePrimaryButton(btnQuery, CampusTheme.NavyAccent);
            btnQuery.Click += delegate { ApplyFilter(); };

            Button btnReset = new Button();
            btnReset.Text = "重置";
            btnReset.Width = 58;
            btnReset.Height = 26;
            CampusTheme.StyleSecondaryButton(btnReset);
            btnReset.Click += delegate
            {
                dtpStart.Value = DateTime.Today.AddDays(-7);
                dtpEnd.Value = DateTime.Today;
                cbFilterResult.SelectedIndex = 0;
                tbSearch.Text = string.Empty;
                RefreshData();
            };

            Button btnExport = new Button();
            btnExport.Text = "导出 CSV";
            btnExport.Width = 86;
            btnExport.Height = 26;
            CampusTheme.StyleSecondaryButton(btnExport);
            btnExport.Click += BtnExport_Click;

            Button btnClear = new Button();
            btnClear.Text = "清空日志";
            btnClear.Width = 78;
            btnClear.Height = 26;
            CampusTheme.StylePrimaryButton(btnClear, CampusTheme.DangerRed);
            btnClear.Click += BtnClear_Click;

            flow.Controls.Add(lblDate);
            flow.Controls.Add(dtpStart);
            flow.Controls.Add(lblTo);
            flow.Controls.Add(dtpEnd);
            flow.Controls.Add(lblRes);
            flow.Controls.Add(cbFilterResult);
            flow.Controls.Add(lblKw);
            flow.Controls.Add(tbSearch);
            flow.Controls.Add(btnQuery);
            flow.Controls.Add(btnReset);
            flow.Controls.Add(btnExport);
            flow.Controls.Add(btnClear);

            bar.Controls.Add(flow);
            return bar;
        }

        public void RefreshData()
        {
            _allLogs = JsonFormat.LoadLogs();
            UpdateKpiCards();
            ApplyFilter();
            pnlChart.Invalidate();
        }

        private void UpdateKpiCards()
        {
            string todayPrefix = DateTime.Now.ToString("yyyy-MM-dd");
            int todaySwipes = 0;
            int total = _allLogs.Count;
            int grants = 0;
            int denies = 0;

            for (int i = 0; i < _allLogs.Count; i++)
            {
                AccessLogEntry log = _allLogs[i];
                if (log.result == "允许通行")
                {
                    grants++;
                    if (log.timestamp.StartsWith(todayPrefix)) todaySwipes++;
                }
                else if (log.result == "拒绝并报警")
                {
                    denies++;
                    if (log.timestamp.StartsWith(todayPrefix)) todaySwipes++;
                }
            }

            int swipeTotal = grants + denies;
            double rate = swipeTotal > 0 ? (grants * 100.0 / swipeTotal) : 100.0;

            lblTodayCount.Text = todaySwipes.ToString();
            lblTotalSwipes.Text = total.ToString();
            lblGrantCount.Text = grants.ToString();
            lblDenyCount.Text = denies.ToString();
            lblPassRate.Text = rate.ToString("0.0") + "%";
        }

        private void ApplyFilter()
        {
            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);
            string resFilter = cbFilterResult.Text;
            string kw = tbSearch.Text.Trim().ToLower();

            _filteredLogs.Clear();
            dgvLogs.Rows.Clear();

            for (int i = 0; i < _allLogs.Count; i++)
            {
                AccessLogEntry log = _allLogs[i];
                DateTime ts;
                if (DateTime.TryParse(log.timestamp, out ts))
                {
                    if (ts < start || ts > end) continue;
                }
                if (resFilter != "全部" && log.result != resFilter) continue;

                if (!string.IsNullOrEmpty(kw))
                {
                    bool hit = (log.cardnum != null && log.cardnum.ToLower().Contains(kw))
                        || (log.name != null && log.name.ToLower().Contains(kw))
                        || (log.stuid != null && log.stuid.ToLower().Contains(kw))
                        || (log.eventType != null && log.eventType.ToLower().Contains(kw));
                    if (!hit) continue;
                }

                _filteredLogs.Add(log);
                int idx = dgvLogs.Rows.Add(
                    log.timestamp, log.cardnum, log.name, log.stuid,
                    log.role, log.department, log.eventType, log.result, log.source, log.detail);
                DataGridViewRow row = dgvLogs.Rows[idx];
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

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV 报表 (*.csv)|*.csv";
                dlg.FileName = "校园门禁通行日志_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    JsonFormat.ExportLogsCsv(dlg.FileName, _filteredLogs);
                    if (StatusMessage != null) StatusMessage(this, "已导出通行报表：" + dlg.FileName);
                    MessageBox.Show("导出通行报表成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("确定要清空全部历史通行日志吗？", "确认清空", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            JsonFormat.ClearLogs();
            RefreshData();
            if (StatusMessage != null) StatusMessage(this, "历史通行日志已清空");
        }

        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = pnlChart.ClientSize.Width;
            int h = pnlChart.ClientSize.Height;
            int midX = w / 2;

            // Left chart: Event category horizontal bars
            int passCnt = 0, unregCnt = 0, statusBlockCnt = 0, ctrlCnt = 0;
            for (int i = 0; i < _allLogs.Count; i++)
            {
                AccessLogEntry log = _allLogs[i];
                if (log.eventType == "合法通行") passCnt++;
                else if (log.eventType == "未注册拦截") unregCnt++;
                else if (log.result == "拒绝并报警") statusBlockCnt++;
                else ctrlCnt++;
            }

            Font titleFont = CampusTheme.BodyBoldFont;
            Font labelFont = CampusTheme.SmallFont;
            using (SolidBrush textBrush = new SolidBrush(CampusTheme.TextPrimary))
            using (SolidBrush mutedBrush = new SolidBrush(CampusTheme.TextMuted))
            {
                g.DrawString("通行事件类型分布统计", titleFont, textBrush, 8, 6);

                string[] names = new string[] { "合法通行", "未注册卡拦截", "挂失/禁用/过期", "手动/自动控制" };
                int[] vals = new int[] { passCnt, unregCnt, statusBlockCnt, ctrlCnt };
                Color[] colors = new Color[]
                {
                    CampusTheme.SuccessGreen,
                    CampusTheme.DangerRed,
                    CampusTheme.WarningAmber,
                    CampusTheme.NavyAccent
                };

                int maxVal = Math.Max(1, Math.Max(Math.Max(passCnt, unregCnt), Math.Max(statusBlockCnt, ctrlCnt)));
                int barAreaMaxW = Math.Max(60, midX - 180);

                for (int i = 0; i < 4; i++)
                {
                    int y = 32 + i * 26;
                    g.DrawString(names[i], labelFont, textBrush, 10, y + 2);
                    int bw = (int)(vals[i] * (double)barAreaMaxW / maxVal);
                    if (vals[i] > 0 && bw < 6) bw = 6;

                    Rectangle bgRect = new Rectangle(118, y + 2, barAreaMaxW, 16);
                    using (SolidBrush bgB = new SolidBrush(Color.FromArgb(241, 245, 249)))
                    {
                        g.FillRectangle(bgB, bgRect);
                    }
                    if (bw > 0)
                    {
                        Rectangle valRect = new Rectangle(118, y + 2, bw, 16);
                        using (SolidBrush vB = new SolidBrush(colors[i]))
                        {
                            g.FillRectangle(vB, valRect);
                        }
                    }
                    g.DrawString(vals[i].ToString() + " 次", labelFont, mutedBrush, 124 + barAreaMaxW, y + 2);
                }

                // Divider
                using (Pen divPen = new Pen(CampusTheme.BorderColor, 1f))
                {
                    g.DrawLine(divPen, midX, 8, midX, h - 8);
                }

                // Right chart: Recent 7 days trend
                g.DrawString("近 7 日门禁刷卡趋势（绿=放行 / 红=拦截）", titleFont, textBrush, midX + 14, 6);
                int[] dayGrants = new int[7];
                int[] dayDenies = new int[7];
                string[] dayLabels = new string[7];
                int maxDay = 1;

                for (int d = 0; d < 7; d++)
                {
                    DateTime targetDate = DateTime.Today.AddDays(d - 6);
                    string prefix = targetDate.ToString("yyyy-MM-dd");
                    dayLabels[d] = targetDate.ToString("MM-dd");
                    for (int k = 0; k < _allLogs.Count; k++)
                    {
                        if (_allLogs[k].timestamp.StartsWith(prefix))
                        {
                            if (_allLogs[k].result == "允许通行") dayGrants[d]++;
                            else if (_allLogs[k].result == "拒绝并报警") dayDenies[d]++;
                        }
                    }
                    if (dayGrants[d] > maxDay) maxDay = dayGrants[d];
                    if (dayDenies[d] > maxDay) maxDay = dayDenies[d];
                }

                int chartLeft = midX + 24;
                int chartW = Math.Max(140, w - chartLeft - 20);
                int baseLineY = h - 24;
                int maxBarH = Math.Max(30, baseLineY - 34);
                int slotW = chartW / 7;

                using (Pen axisPen = new Pen(CampusTheme.BorderColor, 1f))
                {
                    g.DrawLine(axisPen, chartLeft, baseLineY, chartLeft + chartW, baseLineY);
                }

                for (int d = 0; d < 7; d++)
                {
                    int x0 = chartLeft + d * slotW + 6;
                    int barW = Math.Max(8, (slotW - 16) / 2);

                    int gh = (int)(dayGrants[d] * (double)maxBarH / maxDay);
                    int dh = (int)(dayDenies[d] * (double)maxBarH / maxDay);

                    if (gh > 0)
                    {
                        using (SolidBrush gb = new SolidBrush(CampusTheme.SuccessGreen))
                        {
                            g.FillRectangle(gb, x0, baseLineY - gh, barW, gh);
                        }
                    }
                    if (dh > 0)
                    {
                        using (SolidBrush db = new SolidBrush(CampusTheme.DangerRed))
                        {
                            g.FillRectangle(db, x0 + barW + 2, baseLineY - dh, barW, dh);
                        }
                    }
                    g.DrawString(dayLabels[d], labelFont, mutedBrush, x0 - 2, baseLineY + 3);
                }
            }
        }
    }
}
