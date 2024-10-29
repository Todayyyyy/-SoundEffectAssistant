using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testAAudio
{
    public partial class ShortcutKeyForm : Form
    {
        public Keys SelectedKey { get; private set; }

        public ShortcutKeyForm()
        {
            InitializeComponent();
            this.KeyPreview = true; // 使 Form 可以接收按键事件
            this.Text = "快捷键绑定"; // 设置窗口标题
            // 设置窗口启动位置为屏幕中心
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void ShortcutKeyForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否是 <space> 键
            if (e.KeyCode == Keys.Space)
            {
                MessageBox.Show("不能将 <space> 键设置为快捷键。");
                e.SuppressKeyPress = true; // 防止默认行为
                return;
            }
            SelectedKey = e.KeyCode;
            lblInstruction.Text = $"按下的键: {SelectedKey}";
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ShortcutKeyForm_Load(object sender, EventArgs e)
        {

        }
    }

}
