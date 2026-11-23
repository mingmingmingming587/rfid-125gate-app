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

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel2 = new System.Windows.Forms.Panel();
            this.llRecClear = new System.Windows.Forms.LinkLabel();
            this.llSendClear = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSend = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.llZCGetCard = new System.Windows.Forms.Button();
            this.lbCardZC = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbZCState = new System.Windows.Forms.Label();
            this.btnZC = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbSex = new System.Windows.Forms.ComboBox();
            this.tbNum = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.llDoorGetCard = new System.Windows.Forms.Button();
            this.lbDoorCard = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbDoorState = new System.Windows.Forms.Label();
            this.lbSex = new System.Windows.Forms.Label();
            this.lbNum = new System.Windows.Forms.Label();
            this.lbName = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.llRecClear);
            this.panel2.Controls.Add(this.llSendClear);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.tbSend);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.tbMsg);
            this.panel2.Location = new System.Drawing.Point(2, 492);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1030, 107);
            this.panel2.TabIndex = 11;
            // 
            // llRecClear
            // 
            this.llRecClear.AutoSize = true;
            this.llRecClear.Location = new System.Drawing.Point(975, 7);
            this.llRecClear.Name = "llRecClear";
            this.llRecClear.Size = new System.Drawing.Size(29, 12);
            this.llRecClear.TabIndex = 19;
            this.llRecClear.TabStop = true;
            this.llRecClear.Text = "清空";
            this.llRecClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llRecClear_LinkClicked);
            // 
            // llSendClear
            // 
            this.llSendClear.AutoSize = true;
            this.llSendClear.Location = new System.Drawing.Point(450, 7);
            this.llSendClear.Name = "llSendClear";
            this.llSendClear.Size = new System.Drawing.Size(29, 12);
            this.llSendClear.TabIndex = 18;
            this.llSendClear.TabStop = true;
            this.llSendClear.Text = "清空";
            this.llSendClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llSendClear_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(515, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "接收通信协议";
            // 
            // tbSend
            // 
            this.tbSend.Location = new System.Drawing.Point(5, 25);
            this.tbSend.Multiline = true;
            this.tbSend.Name = "tbSend";
            this.tbSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbSend.Size = new System.Drawing.Size(500, 79);
            this.tbSend.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "发送通信协议";
            // 
            // tbMsg
            // 
            this.tbMsg.Location = new System.Drawing.Point(512, 25);
            this.tbMsg.Multiline = true;
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMsg.Size = new System.Drawing.Size(515, 79);
            this.tbMsg.TabIndex = 14;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(2, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1034, 492);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Window;
            this.tabPage3.Controls.Add(this.tabControl2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1026, 466);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "动手操作";
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1026, 466);
            this.tabControl2.TabIndex = 0;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.panel1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1018, 440);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "注册界面";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.llZCGetCard);
            this.panel1.Controls.Add(this.lbCardZC);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1018, 440);
            this.panel1.TabIndex = 0;
            // 
            // llZCGetCard
            // 
            this.llZCGetCard.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.llZCGetCard.Location = new System.Drawing.Point(592, 94);
            this.llZCGetCard.Name = "llZCGetCard";
            this.llZCGetCard.Size = new System.Drawing.Size(75, 23);
            this.llZCGetCard.TabIndex = 3;
            this.llZCGetCard.Text = "获取卡号";
            this.llZCGetCard.UseVisualStyleBackColor = true;
            this.llZCGetCard.Click += new System.EventHandler(this.llZCGetCard_Click);
            // 
            // lbCardZC
            // 
            this.lbCardZC.AutoSize = true;
            this.lbCardZC.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCardZC.Location = new System.Drawing.Point(343, 97);
            this.lbCardZC.Name = "lbCardZC";
            this.lbCardZC.Size = new System.Drawing.Size(28, 14);
            this.lbCardZC.TabIndex = 1;
            this.lbCardZC.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(260, 97);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 14);
            this.label1.TabIndex = 1;
            this.label1.Text = "卡号信息：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbZCState);
            this.groupBox1.Controls.Add(this.btnZC);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbSex);
            this.groupBox1.Controls.Add(this.tbNum);
            this.groupBox1.Controls.Add(this.tbName);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(260, 137);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 296);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "个人信息";
            // 
            // lbZCState
            // 
            this.lbZCState.AutoSize = true;
            this.lbZCState.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbZCState.Location = new System.Drawing.Point(123, 221);
            this.lbZCState.Name = "lbZCState";
            this.lbZCState.Size = new System.Drawing.Size(35, 14);
            this.lbZCState.TabIndex = 4;
            this.lbZCState.Text = "....";
            // 
            // btnZC
            // 
            this.btnZC.Location = new System.Drawing.Point(161, 258);
            this.btnZC.Name = "btnZC";
            this.btnZC.Size = new System.Drawing.Size(75, 23);
            this.btnZC.TabIndex = 4;
            this.btnZC.Text = "注册";
            this.btnZC.UseVisualStyleBackColor = true;
            this.btnZC.Click += new System.EventHandler(this.btnZC_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(42, 218);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "状态：";
            // 
            // cbSex
            // 
            this.cbSex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSex.FormattingEnabled = true;
            this.cbSex.Items.AddRange(new object[] {
            "男",
            "女"});
            this.cbSex.Location = new System.Drawing.Point(122, 176);
            this.cbSex.Name = "cbSex";
            this.cbSex.Size = new System.Drawing.Size(166, 20);
            this.cbSex.TabIndex = 3;
            // 
            // tbNum
            // 
            this.tbNum.Location = new System.Drawing.Point(122, 118);
            this.tbNum.Name = "tbNum";
            this.tbNum.Size = new System.Drawing.Size(166, 21);
            this.tbNum.TabIndex = 2;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(122, 61);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(166, 21);
            this.tbName.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(42, 176);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 14);
            this.label10.TabIndex = 1;
            this.label10.Text = "性别：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(42, 119);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 14);
            this.label8.TabIndex = 1;
            this.label8.Text = "学号：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(42, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 14);
            this.label4.TabIndex = 1;
            this.label4.Text = "姓名：";
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.panel3);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1018, 440);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "门禁卡功能";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.llDoorGetCard);
            this.panel3.Controls.Add(this.lbDoorCard);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.groupBox2);
            this.panel3.Location = new System.Drawing.Point(1, 1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1018, 440);
            this.panel3.TabIndex = 1;
            // 
            // llDoorGetCard
            // 
            this.llDoorGetCard.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.llDoorGetCard.Location = new System.Drawing.Point(638, 71);
            this.llDoorGetCard.Name = "llDoorGetCard";
            this.llDoorGetCard.Size = new System.Drawing.Size(75, 23);
            this.llDoorGetCard.TabIndex = 6;
            this.llDoorGetCard.Text = "获取卡号";
            this.llDoorGetCard.UseVisualStyleBackColor = true;
            this.llDoorGetCard.Click += new System.EventHandler(this.llDoorGetCard_Click);
            // 
            // lbDoorCard
            // 
            this.lbDoorCard.AutoSize = true;
            this.lbDoorCard.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbDoorCard.Location = new System.Drawing.Point(389, 71);
            this.lbDoorCard.Name = "lbDoorCard";
            this.lbDoorCard.Size = new System.Drawing.Size(28, 14);
            this.lbDoorCard.TabIndex = 4;
            this.lbDoorCard.Text = "...";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(306, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 14);
            this.label6.TabIndex = 5;
            this.label6.Text = "卡号信息：";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbDoorState);
            this.groupBox2.Controls.Add(this.lbSex);
            this.groupBox2.Controls.Add(this.lbNum);
            this.groupBox2.Controls.Add(this.lbName);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Location = new System.Drawing.Point(306, 111);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(407, 259);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "个人信息";
            // 
            // lbDoorState
            // 
            this.lbDoorState.AutoSize = true;
            this.lbDoorState.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbDoorState.Location = new System.Drawing.Point(141, 227);
            this.lbDoorState.Name = "lbDoorState";
            this.lbDoorState.Size = new System.Drawing.Size(28, 14);
            this.lbDoorState.TabIndex = 2;
            this.lbDoorState.Text = "...";
            // 
            // lbSex
            // 
            this.lbSex.AutoSize = true;
            this.lbSex.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbSex.Location = new System.Drawing.Point(141, 176);
            this.lbSex.Name = "lbSex";
            this.lbSex.Size = new System.Drawing.Size(28, 14);
            this.lbSex.TabIndex = 2;
            this.lbSex.Text = "...";
            // 
            // lbNum
            // 
            this.lbNum.AutoSize = true;
            this.lbNum.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbNum.Location = new System.Drawing.Point(141, 119);
            this.lbNum.Name = "lbNum";
            this.lbNum.Size = new System.Drawing.Size(28, 14);
            this.lbNum.TabIndex = 3;
            this.lbNum.Text = "...";
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbName.Location = new System.Drawing.Point(141, 62);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(28, 14);
            this.lbName.TabIndex = 4;
            this.lbName.Text = "...";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(42, 227);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(49, 14);
            this.label12.TabIndex = 1;
            this.label12.Text = "状态：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(42, 176);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 14);
            this.label7.TabIndex = 1;
            this.label7.Text = "性别：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(42, 119);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 14);
            this.label9.TabIndex = 1;
            this.label9.Text = "学号：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(42, 62);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(49, 14);
            this.label11.TabIndex = 1;
            this.label11.Text = "姓名：";
            // 
            // Form125GateApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 601);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form125GateApp";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form125GateApp_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel llRecClear;
        private System.Windows.Forms.LinkLabel llSendClear;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSend;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lbCardZC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.ComboBox cbSex;
        private System.Windows.Forms.TextBox tbNum;
        private System.Windows.Forms.Button btnZC;
        private System.Windows.Forms.Label lbDoorCard;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lbSex;
        private System.Windows.Forms.Label lbNum;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button llDoorGetCard;
        private System.Windows.Forms.Button llZCGetCard;
        private System.Windows.Forms.Label lbZCState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lbDoorState;
    }
}

