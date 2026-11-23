using System;
using System.Drawing;
using System.Windows.Forms;

namespace _125GateApp.UI
{
    /// <summary>
    /// 校园门禁系统统一配色、字体与控件样式辅助类。
    /// </summary>
    public static class CampusTheme
    {
        public static readonly Color NavyHeader = Color.FromArgb(24, 48, 82);
        public static readonly Color NavyAccent = Color.FromArgb(37, 99, 235);
        public static readonly Color PageBg = Color.FromArgb(243, 246, 250);
        public static readonly Color CardBg = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(219, 226, 236);
        public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);

        public static readonly Color SuccessGreen = Color.FromArgb(22, 163, 74);
        public static readonly Color SuccessSoft = Color.FromArgb(236, 253, 245);
        public static readonly Color WarningAmber = Color.FromArgb(217, 119, 6);
        public static readonly Color WarningSoft = Color.FromArgb(255, 251, 235);
        public static readonly Color DangerRed = Color.FromArgb(220, 38, 38);
        public static readonly Color DangerSoft = Color.FromArgb(254, 242, 242);
        public static readonly Color InfoSoft = Color.FromArgb(239, 246, 255);

        public static readonly Font TitleFont = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
        public static readonly Font BodyBoldFont = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
        public static readonly Font MonoFont = new Font("Consolas", 9.5F, FontStyle.Regular);

        public static void StylePrimaryButton(Button btn, Color bgColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = bgColor;
            btn.ForeColor = Color.White;
            btn.Font = BodyBoldFont;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.BackColor = Color.White;
            btn.ForeColor = TextPrimary;
            btn.Font = BodyFont;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(232, 237, 244);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = BodyBoldFont;
            dgv.ColumnHeadersHeight = 32;
            dgv.RowTemplate.Height = 28;
            dgv.DefaultCellStyle.Font = BodyFont;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
