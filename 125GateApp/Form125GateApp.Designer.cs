namespace _125GateApp
{
    partial class Form125GateApp
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblAppSub = new System.Windows.Forms.Label();
            this.flowHeaderRight = new System.Windows.Forms.FlowLayoutPanel();
            this.lblConnBadge = new System.Windows.Forms.Label();
            this.btnQuickSimToggle = new System.Windows.Forms.Button();
            this.btnToggleProtocol = new System.Windows.Forms.Button();

            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageRegister = new System.Windows.Forms.TabPage();
            this.tabPageMonitor = new System.Windows.Forms.TabPage();
            this.tabPageManage = new System.Windows.Forms.TabPage();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.tabPageDebug = new System.Windows.Forms.TabPage();

            this.panel2 = new System.Windows.Forms.Panel();
            this.tblProtocol = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSendHeader = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.llSendClear = new System.Windows.Forms.LinkLabel();
            this.tbSend = new System.Windows.Forms.TextBox();
            this.pnlRecHeader = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.llRecClear = new System.Windows.Forms.LinkLabel();
            this.tbMsg = new System.Windows.Forms.TextBox();

            this.panelHeader.SuspendLayout();
            this.flowHeaderRight.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tblProtocol.SuspendLayout();
            this.pnlSendHeader.SuspendLayout();
            this.pnlRecHeader.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(24, 48, 82);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 64;
            this.panelHeader.Padding = new System.Windows.Forms.Padding(14, 8, 14, 6);
            this.panelHeader.Controls.Add(this.flowHeaderRight);
            this.panelHeader.Controls.Add(this.lblAppSub);
            this.panelHeader.Controls.Add(this.lblAppTitle);

            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.Location = new System.Drawing.Point(14, 8);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Text = "125K 低频 RFID 校园智能门禁综合实验系统";

            // 
            // lblAppSub
            // 
            this.lblAppSub.AutoSize = true;
            this.lblAppSub.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Regular);
            this.lblAppSub.ForeColor = System.Drawing.Color.FromArgb(191, 219, 254);
            this.lblAppSub.Location = new System.Drawing.Point(15, 36);
            this.lblAppSub.Name = "lblAppSub";
            this.lblAppSub.Text = "F-Table 实验平台综合应用  |  支持 EM4100/T5557 读卡 · 步进电机道闸 · 声光报警 · 无硬件仿真测试";

            // 
            // flowHeaderRight
            // 
            this.flowHeaderRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowHeaderRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowHeaderRight.WrapContents = false;
            this.flowHeaderRight.AutoSize = true;
            this.flowHeaderRight.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowHeaderRight.Controls.Add(this.lblConnBadge);
            this.flowHeaderRight.Controls.Add(this.btnQuickSimToggle);
            this.flowHeaderRight.Controls.Add(this.btnToggleProtocol);

            // 
            // lblConnBadge
            // 
            this.lblConnBadge.AutoSize = true;
            this.lblConnBadge.BackColor = System.Drawing.Color.FromArgb(30, 64, 114);
            this.lblConnBadge.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConnBadge.ForeColor = System.Drawing.Color.FromArgb(253, 224, 71);
            this.lblConnBadge.Margin = new System.Windows.Forms.Padding(0, 3, 10, 0);
            this.lblConnBadge.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblConnBadge.Text = "正在初始化...";

            // 
            // btnQuickSimToggle
            // 
            this.btnQuickSimToggle.BackColor = System.Drawing.Color.FromArgb(79, 70, 229);
            this.btnQuickSimToggle.FlatAppearance.BorderSize = 0;
            this.btnQuickSimToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuickSimToggle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnQuickSimToggle.ForeColor = System.Drawing.Color.White;
            this.btnQuickSimToggle.Size = new System.Drawing.Size(116, 28);
            this.btnQuickSimToggle.Text = "切换仿真模式";
            this.btnQuickSimToggle.UseVisualStyleBackColor = false;
            this.btnQuickSimToggle.Click += new System.EventHandler(this.btnQuickSimToggle_Click);

            // 
            // btnToggleProtocol
            // 
            this.btnToggleProtocol.BackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnToggleProtocol.FlatAppearance.BorderSize = 0;
            this.btnToggleProtocol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleProtocol.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Regular);
            this.btnToggleProtocol.ForeColor = System.Drawing.Color.White;
            this.btnToggleProtocol.Margin = new System.Windows.Forms.Padding(6, 3, 0, 0);
            this.btnToggleProtocol.Size = new System.Drawing.Size(96, 28);
            this.btnToggleProtocol.Text = "折叠协议区";
            this.btnToggleProtocol.UseVisualStyleBackColor = false;
            this.btnToggleProtocol.Click += new System.EventHandler(this.btnToggleProtocol_Click);

            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPageRegister);
            this.tabMain.Controls.Add(this.tabPageMonitor);
            this.tabMain.Controls.Add(this.tabPageManage);
            this.tabMain.Controls.Add(this.tabPageLogs);
            this.tabMain.Controls.Add(this.tabPageDebug);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Regular);
            this.tabMain.ItemSize = new System.Drawing.Size(136, 32);
            this.tabMain.Name = "tabMain";
            this.tabMain.Padding = new System.Drawing.Point(10, 4);
            this.tabMain.SelectedIndex = 0;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);

            this.tabPageRegister.Name = "tabPageRegister";
            this.tabPageRegister.Text = "1. 发卡注册界面";
            this.tabPageRegister.UseVisualStyleBackColor = true;

            this.tabPageMonitor.Name = "tabPageMonitor";
            this.tabPageMonitor.Text = "2. 门禁实时监控";
            this.tabPageMonitor.UseVisualStyleBackColor = true;

            this.tabPageManage.Name = "tabPageManage";
            this.tabPageManage.Text = "3. 人员与卡务管理";
            this.tabPageManage.UseVisualStyleBackColor = true;

            this.tabPageLogs.Name = "tabPageLogs";
            this.tabPageLogs.Text = "4. 通行日志与统计";
            this.tabPageLogs.UseVisualStyleBackColor = true;

            this.tabPageDebug.Name = "tabPageDebug";
            this.tabPageDebug.Text = "5. 设备调试与发卡设置";
            this.tabPageDebug.UseVisualStyleBackColor = true;

            // 
            // panel2 (底部通信协议监控区)
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Height = 124;
            this.panel2.Padding = new System.Windows.Forms.Padding(8, 4, 8, 6);
            this.panel2.Controls.Add(this.tblProtocol);

            this.tblProtocol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblProtocol.ColumnCount = 2;
            this.tblProtocol.RowCount = 2;
            this.tblProtocol.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblProtocol.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblProtocol.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tblProtocol.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            this.pnlSendHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(2, 4);
            this.label2.Text = "发送通信协议 (Host -> F-Table)";
            this.llSendClear.AutoSize = true;
            this.llSendClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.llSendClear.Padding = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.llSendClear.Text = "清空";
            this.llSendClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llSendClear_LinkClicked);
            this.pnlSendHeader.Controls.Add(this.label2);
            this.pnlSendHeader.Controls.Add(this.llSendClear);

            this.pnlRecHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(2, 4);
            this.label5.Text = "接收通信协议 (F-Table -> Host)";
            this.llRecClear.AutoSize = true;
            this.llRecClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.llRecClear.Padding = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.llRecClear.Text = "清空";
            this.llRecClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llRecClear_LinkClicked);
            this.pnlRecHeader.Controls.Add(this.label5);
            this.pnlRecHeader.Controls.Add(this.llRecClear);

            this.tbSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSend.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.tbSend.Multiline = true;
            this.tbSend.ReadOnly = true;
            this.tbSend.BackColor = System.Drawing.Color.White;
            this.tbSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbSend.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);

            this.tbMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMsg.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.tbMsg.Multiline = true;
            this.tbMsg.ReadOnly = true;
            this.tbMsg.BackColor = System.Drawing.Color.White;
            this.tbMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMsg.Margin = new System.Windows.Forms.Padding(6, 2, 2, 2);

            this.tblProtocol.Controls.Add(this.pnlSendHeader, 0, 0);
            this.tblProtocol.Controls.Add(this.pnlRecHeader, 1, 0);
            this.tblProtocol.Controls.Add(this.tbSend, 0, 1);
            this.tblProtocol.Controls.Add(this.tbMsg, 1, 1);

            // 
            // Form125GateApp
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular);
            this.ClientSize = new System.Drawing.Size(1200, 790);
            this.MinimumSize = new System.Drawing.Size(1020, 680);
            this.Name = "Form125GateApp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "125K 低频 RFID 校园智能门禁综合实验系统";
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panel2);
            this.Load += new System.EventHandler(this.Form125GateApp_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form125GateApp_FormClosing);

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.flowHeaderRight.ResumeLayout(false);
            this.flowHeaderRight.PerformLayout();
            this.tabMain.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tblProtocol.ResumeLayout(false);
            this.tblProtocol.PerformLayout();
            this.pnlSendHeader.ResumeLayout(false);
            this.pnlSendHeader.PerformLayout();
            this.pnlRecHeader.ResumeLayout(false);
            this.pnlRecHeader.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblAppSub;
        private System.Windows.Forms.FlowLayoutPanel flowHeaderRight;
        private System.Windows.Forms.Label lblConnBadge;
        private System.Windows.Forms.Button btnQuickSimToggle;
        private System.Windows.Forms.Button btnToggleProtocol;

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageRegister;
        private System.Windows.Forms.TabPage tabPageMonitor;
        private System.Windows.Forms.TabPage tabPageManage;
        private System.Windows.Forms.TabPage tabPageLogs;
        private System.Windows.Forms.TabPage tabPageDebug;

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tblProtocol;
        private System.Windows.Forms.Panel pnlSendHeader;
        private System.Windows.Forms.Panel pnlRecHeader;
        private System.Windows.Forms.LinkLabel llRecClear;
        private System.Windows.Forms.LinkLabel llSendClear;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSend;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMsg;
    }
}
