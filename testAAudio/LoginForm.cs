using System;
using System.IO;
using System.Windows.Forms;


namespace testAAudio
{
    public partial class LoginForm : Form
    {
        private SettingsManager settingsManager; // 添加 SettingsManager 实例
        string storedUsername = "newlite"; // 本地储存的用户名
        string storedPassword = "dkl"; // 本地储存的密码
        private bool exitRequested = false;


        // 更新构造函数以接收 SettingsManager 实例
        public LoginForm(SettingsManager settingsManager)
        {
            InitializeComponent();
            this.settingsManager = settingsManager; // 初始化 SettingsManager 实例
            this.Text = "Newlite音效助手"; // 设置窗口标题
            // 设置窗口启动位置为屏幕中心
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += LoginForm_FormClosing;

            txtPassword.PasswordChar = '*';
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == storedUsername && txtPassword.Text == storedPassword)
            {
                // 保存登录时间
                SaveLoginTime();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("账号或密码错误，请重试。");
            }
        }

        private void SaveLoginTime()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "settings.json");
            SettingsManager settingsManager = new SettingsManager(settingsPath);

            var settings = settingsManager.LoadSettings(); // 从文件中加载现有设置
            settings.LastLoginTime = DateTime.Now; // 设置新的登录时间
            settingsManager.SaveSettings(settings); // 保存更新后的设置
        }




        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exitRequested && this.DialogResult != DialogResult.OK)
            {
                // 防止用户在未登录的情况下关闭窗口
                e.Cancel = true;
                //MessageBox.Show("请点击“退出”按钮来退出本程序");
                exitRequested = true;
                Application.Exit();
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            exitRequested = true;
            Application.Exit();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnLogin_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
