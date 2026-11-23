using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _125GateApp
{
    public partial class Form125GateApp
    {
        private ComboBox portBox;
        private Label connectionStatus;
        private DataGridView users;
        private TextBox searchBox;
        private string DatabasePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugin", "125GateApp", "db.json"); } }

        private void SetupExtraPages()
        {
            tabPage4.Text = "卡片注册";
            tabPage5.Text = "门禁验证";
            var device = new TabPage("设备连接");
            var manager = new TabPage("用户管理");
            tabControl2.TabPages.Insert(0, device);
            tabControl2.TabPages.Add(manager);
            var portLabel = new Label { Text = "串口", Location = new Point(25, 30), AutoSize = true };
            portBox = new ComboBox { Location = new Point(110, 26), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            var refresh = new Button { Text = "刷新串口", Location = new Point(275, 24), AutoSize = true };
            var baud = new Label { Text = "波特率：57600", Location = new Point(25, 75), AutoSize = true };
            var connect = new Button { Text = "连接", Location = new Point(25, 115), AutoSize = true };
            var disconnect = new Button { Text = "断开", Location = new Point(125, 115), AutoSize = true };
            connectionStatus = new Label { Text = "未连接", Location = new Point(25, 160), AutoSize = true };
            device.Controls.AddRange(new Control[] { portLabel, portBox, refresh, baud, connect, disconnect, connectionStatus });
            Action refreshPorts = () => { var previous = portBox.Text; portBox.Items.Clear(); portBox.Items.AddRange(SerialPort.GetPortNames()); if (portBox.Items.Contains(previous)) portBox.SelectedItem = previous; else if (portBox.Items.Count > 0) portBox.SelectedIndex = 0; };
            refresh.Click += (s, e) => refreshPorts();
            connect.Click += (s, e) => {
                try {
                    if (portBox.SelectedItem == null) { MessageBox.Show("请先选择串口"); return; }
                    if (ComHelper.sp != null && ComHelper.sp.IsOpen) { MessageBox.Show("请先断开当前连接"); return; }
                    if (!ComHelper.openPort(portBox.Text, 57600)) { connectionStatus.Text = "连接失败，请检查串口占用和设备"; return; }
                    ComHelper.sp.DataReceived += sp_DataReceived;
                    connectionStatus.Text = "已连接 " + portBox.Text + "，57600";
                } catch (Exception ex) { connectionStatus.Text = "连接失败"; MessageBox.Show(ex.Message); }
            };
            disconnect.Click += (s, e) => DisconnectDevice();
            FormClosed += (s, e) => DisconnectDevice();
            refreshPorts();

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            searchBox = new TextBox { Width = 150 };
            var search = new Button { Text = "查询" };
            var reload = new Button { Text = "刷新" };
            var save = new Button { Text = "保存修改" };
            var delete = new Button { Text = "删除选中" };
            toolbar.Controls.AddRange(new Control[] { new Label { Text = "卡号 / 姓名 / 学号", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, searchBox, search, reload, save, delete });
            users = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            users.Columns.Add("cardnum", "卡号"); users.Columns["cardnum"].ReadOnly = true;
            users.Columns.Add("name", "姓名"); users.Columns.Add("stuid", "学号"); users.Columns.Add("sex", "性别");
            users.Columns.Add(new DataGridViewCheckBoxColumn { Name = "enabled", HeaderText = "启用" });
            manager.Controls.Add(users); manager.Controls.Add(toolbar);
            search.Click += (s, e) => LoadUsers(); reload.Click += (s, e) => { searchBox.Clear(); LoadUsers(); };
            manager.Enter += (s, e) => LoadUsers();
            save.Click += (s, e) => SaveSelectedUser(false);
            delete.Click += (s, e) => SaveSelectedUser(true);
        }

        private void DisconnectDevice()
        {
            try { if (ComHelper.sp != null && ComHelper.sp.IsOpen) { ComHelper.sp.DataReceived -= sp_DataReceived; ComHelper.sp.Close(); } if (connectionStatus != null) connectionStatus.Text = "未连接"; }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private bool DeviceReady()
        {
            if (ComHelper.sp != null && ComHelper.sp.IsOpen) return true;
            MessageBox.Show("请先在设备连接页面选择串口并连接。"); return false;
        }

        private JArray ReadUsers()
        {
            if (!File.Exists(DatabasePath)) return new JArray();
            var data = File.ReadAllText(DatabasePath);
            return string.IsNullOrWhiteSpace(data) ? new JArray() : JArray.Parse(data);
        }

        private void LoadUsers()
        {
            try {
                users.Rows.Clear(); string term = searchBox.Text.Trim();
                foreach (JObject user in ReadUsers()) {
                    string card = (string)user["cardnum"] ?? "", name = (string)user["name"] ?? "", id = (string)user["stuid"] ?? "";
                    if (term.Length > 0 && !card.Contains(term) && !name.Contains(term) && !id.Contains(term)) continue;
                    users.Rows.Add(card, name, id, (string)user["sex"] ?? "", (bool?)user["enabled"] != false);
                }
            } catch (Exception ex) { MessageBox.Show("读取用户数据失败：" + ex.Message); }
        }

        private void SaveSelectedUser(bool remove)
        {
            if (users.CurrentRow == null) { MessageBox.Show("请先选择用户"); return; }
            users.EndEdit(); var row = users.CurrentRow; string card = Convert.ToString(row.Cells["cardnum"].Value);
            if (remove && MessageBox.Show("确定删除选中的用户？", "确认删除", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            string name = Convert.ToString(row.Cells["name"].Value).Trim(), id = Convert.ToString(row.Cells["stuid"].Value).Trim(), sex = Convert.ToString(row.Cells["sex"].Value).Trim();
            if (!remove && (name.Length == 0 || id.Length == 0 || sex.Length == 0)) { MessageBox.Show("姓名、学号和性别不能为空"); return; }
            try {
                var all = ReadUsers(); JObject found = null;
                foreach (JObject item in all) if ((string)item["cardnum"] == card) { found = item; break; }
                if (found == null) { MessageBox.Show("用户不存在，请刷新列表"); return; }
                if (remove) found.Remove(); else { found["name"] = name; found["stuid"] = id; found["sex"] = sex; found["enabled"] = Convert.ToBoolean(row.Cells["enabled"].Value); }
                string temp = DatabasePath + ".tmp";
                File.WriteAllText(temp, JsonConvert.SerializeObject(all));
                File.Replace(temp, DatabasePath, DatabasePath + ".bak");
                LoadUsers(); MessageBox.Show(remove ? "已删除" : "已保存");
            } catch (Exception ex) { MessageBox.Show("保存失败：" + ex.Message); }
        }
    }
}
