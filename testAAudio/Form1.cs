using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace testAAudio
{
    public partial class Form1 : Form
    {
        private const int WH_KEYBOARD_LL = 13; // 钩子类型
        private const int WM_KEYDOWN = 0x0100; // 按键按下消息
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private AudioDeviceManager audioDeviceManager;
        
        private WaveOutEvent waveOut;
        private AudioFileReader audioFileReader;
        private WaveStream waveStream;
        
        private string currentFilePath;
        private bool isPlaying;
        private bool isTrackBarDragging;
        private Keys shortcutKey;
        private bool isSettingShortcutKey; // 标志是否正在设置快捷键
        private bool isShortcutKeyEnabled; // 控制快捷键是否启用
        private System.Windows.Forms.Timer timer;
        private Dictionary<Button, MemoryStream> buttonAudioMapping;
        private Dictionary<Button, Label> buttonLabelMapping;
        public Dictionary<Keys, Button> shortcutKeyMapping;
        private AppSettings appSettings;
        private SettingsManager settingsManager;


        public Form1()
        {
            // 加载设置
            // 更改为用户文档目录
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "settings.json");
            settingsManager = new SettingsManager(settingsPath);

            appSettings = settingsManager.LoadSettings(); // 先加载设置

            InitializeComponent();
            InitializeButtonAudioMapping();
            InitializeButtons();
            InitializeButtonLabelMapping();
            InitializeShortcutKeyMapping();

            ApplySettings();// 然后应用设置

            ShowLoginForm();

            // 设置窗口启动位置为屏幕中心
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Newlite音效助手"; // 设置窗口标题
            this.KeyPreview = true;
            audioDeviceManager = new AudioDeviceManager();
            LoadAudioDevices();
            openFileDialog = new OpenFileDialog();
            ButtonPlay.Click -= ButtonPlay_Click;
            ButtonPlay.Click += ButtonPlay_Click;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();


            this.ActiveControl = null;
            ButtonPlay.BackgroundImage = Properties.Resources.play;

            isShortcutKeyEnabled = true;
            ButtonKey.Text = "快捷键：开启";

            this.FormClosing += Form1_FormClosing; // 在窗口关闭时保存设置
        }

        private void SetHook()
        {
            _proc = HookCallback;
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);
        }
                
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);

                // 检查快捷键映射字典
                if (shortcutKeyMapping.TryGetValue(key, out Button button))
                {
                    // 触发与快捷键关联的按钮点击事件
                    Button_Click(button); // 替换为你的功能调用
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void UnhookWindowsHookEx()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetHook();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookWindowsHookEx();
            base.OnFormClosing(e);
        }


        private void ApplySettings()
        {
            // 设置快捷键
            foreach (var kvp in appSettings.ShortcutKeys)
            {
                var buttonName = kvp.Key;
                var key = kvp.Value;

                // 查找与按钮名称对应的按钮控件
                var button = this.Controls.Find(buttonName, true).FirstOrDefault() as Button;
                if (button != null)
                {
                    if (shortcutKeyMapping.ContainsKey(key))
                    {
                        shortcutKeyMapping.Remove(key); // 移除旧的键映射
                    }
                    shortcutKeyMapping[key] = button;
                    if (buttonLabelMapping.TryGetValue(button, out Label label))
                    {
                        label.Text = $"快捷键: {key}";
                        label.Visible = true;
                    }
                }
            }

            // 设置音量
            volumeSlider.Volume = appSettings.Volume;

            // 加载按钮和音频文件路径映射
            foreach (var kvp in appSettings.ButtonAudioFilePaths)
            {
                var buttonName = kvp.Key;
                var filePath = kvp.Value;

                var button = this.Controls.Find(buttonName, true).FirstOrDefault() as Button;
                if (button != null && File.Exists(filePath))
                {
                    byte[] audioBytes = File.ReadAllBytes(filePath);
                    MemoryStream audioStream = new MemoryStream(audioBytes);
                    buttonAudioMapping[button] = audioStream;

                    // 更新按钮文本为文件名（不包括扩展名）
                    button.Text = Path.GetFileNameWithoutExtension(filePath);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存音量
            appSettings.Volume = volumeSlider.Volume;

            // 保存快捷键设置
            appSettings.ShortcutKeys.Clear();
            foreach (var kvp in shortcutKeyMapping)
            {
                var key = kvp.Key;
                var button = kvp.Value;
                appSettings.ShortcutKeys[button.Name] = key;
            }

            // 保存按钮和音频文件路径映射
            SaveButtonAudioPaths();

            // 更新 LastLoginTime
            appSettings.LastLoginTime = DateTime.Now;

            // 保存设置
            settingsManager.SaveSettings(appSettings);
        }

        private void SaveButtonAudioPaths()
        {
            foreach (var kvp in buttonAudioMapping)
            {
                var button = kvp.Key;
                if (appSettings.ButtonAudioFilePaths.TryGetValue(button.Name, out var existingFilePath))
                {
                    // 如果已经有文件路径存在，使用现有路径
                    appSettings.ButtonAudioFilePaths[button.Name] = existingFilePath;
                }
            }
        }


        private void ShowLoginForm()
        {
            DateTime lastLoginTime = DateTime.MinValue; // 初始化变量

            // 从 SettingsManager 加载设置
            AppSettings settings = settingsManager.LoadSettings();

            // 检查 LastLoginTime 是否存在和非空
            if (settings.LastLoginTime.HasValue && DateTime.Now - settings.LastLoginTime.Value < TimeSpan.FromDays(1))
            {
                return; // 如果上次登录时间在一天内，则不显示登录窗口
            }

            // 创建 LoginForm 实例并传递 SettingsManager
            LoginForm loginForm = new LoginForm(settingsManager);
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                // 如果登录不成功，关闭主窗口
                this.Close();
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 如果正在设置快捷键，并且尝试设置 <space> 键
            if (isSettingShortcutKey)
            {
                // 取消快捷键设置状态
                if (keyData == Keys.Space)
                {
                    MessageBox.Show("不能将 <space> 键设置为快捷键。");
                    isSettingShortcutKey = false; // 取消设置状态
                    return true; // 表示事件已处理
                }

                // 设置其他快捷键
                shortcutKey = keyData;
                MessageBox.Show($"快捷键已设置为: {shortcutKey}");
                isSettingShortcutKey = false;

                // 检查快捷键是否已被使用
                if (shortcutKeyMapping.ContainsKey(shortcutKey))
                {
                    MessageBox.Show("此快捷键已被其他按钮使用，请选择其他快捷键。");
                    return true;
                }

                // 将快捷键映射到按钮上
                if (buttonLabelMapping.TryGetValue(ButtonPlay, out Label label))
                {
                    // 移除旧的快捷键
                    if (shortcutKeyMapping.ContainsKey(shortcutKey))
                    {
                        shortcutKeyMapping.Remove(shortcutKey);
                    }

                    label.Text = $"快捷键: {shortcutKey}";
                    label.Visible = true;
                    shortcutKeyMapping[shortcutKey] = ButtonPlay; // 为 ButtonPlay 设置快捷键映射
                }
                return true; // 表示事件已处理
            }

            // 处理 <space> 键，调用 ButtonPlay_Click
            if (keyData == Keys.Space)
            {
                ButtonPlay_Click(ButtonPlay, EventArgs.Empty);
                return true; // 表示事件已处理
            }

            // 检查是否启用了快捷键功能
            if (isShortcutKeyEnabled && shortcutKeyMapping.TryGetValue(keyData, out Button button))
            {
                Button_Click(button); // 触发与快捷键关联的按钮点击事件
                return true; // 表示事件已处理
            }

            // 默认处理其他键
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeButtonLabelMapping()
        {
            buttonLabelMapping = new Dictionary<Button, Label>
            {
                { button1, label1 },
                { button2, label2 },
                { button3, label3 },
                { button4, label4 },
                { button5, label5 },
                { button6, label6 },
                { button7, label7 },
                { button8, label8 },
                { button9, label9 },
                { button10, label10 },
                { button11, label11 },
                { button12, label12 },
                { button13, label13 },
                { button14, label14 },
                { button15, label15 },
                { button16, label16 },
                { button17, label17 },
                { button18, label18 },
                { button19, label19 },
                { button20, label20 },
                { button21, label21 },
                { button22, label22 },
                { button23, label23 },
                { button24, label24 },
                { button25, label25 },
                { button26, label26 },
                { button27, label27 },
                { button28, label28 },
                { button29, label29 },
                { button30, label30 },
                { button31, label31 },
                { button32, label32 },
                { button33, label33 },
                { button34, label34 },
                { button35, label35 },
                { button36, label36 },
                { button37, label37 },
                { button38, label38 },
                { button39, label39 },
                { button40, label40 },
                { button41, label41 },
                { ButtonPlayPause,labelShortcutKey }
                // 继续为其他按钮和Label进行映射
             };
        }

        private void InitializeShortcutKeyMapping()
        {
            shortcutKeyMapping = new Dictionary<Keys, Button>();
        }

        private byte[] ReadStreamToByteArray(UnmanagedMemoryStream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void InitializeButtonAudioMapping()
        {
            buttonAudioMapping = new Dictionary<Button, MemoryStream>
            {
         //笑声
        { ButtonPlayPause, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_001)) },
        { button1, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_002)) },
        { button2, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_003)) },
        { button3, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_004)) },
        { button4, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_005)) },
        { button5, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_006)) },
        { button6, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_007)) },
        //哭声搞怪
        { button7, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_009)) },
        { button8, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_010)) },
        { button9, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_011)) },
        { button10, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_012)) },
        { button11, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_031)) },
        { button12, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_016)) },
        { button13, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_017)) },
        //气氛氛围
        { button14, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_018)) },
        { button15, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_019)) },
        { button16, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_020)) },
        { button17, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_021)) },
        { button18, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_022)) },
        { button19, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_023)) },
        { button20, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_024)) },
        //诗歌剧
        { button21, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_014)) },
        { button22, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_025)) },
        { button23, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_026)) },
        { button24, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_027)) },
        { button25, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_028)) },
        { button26, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_029)) },
        { button27, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_030)) },
        //XXXX
        { button28, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_001)) },
        { button29, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_002)) },
        { button30, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_003)) },
        { button31, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_004)) },
        { button32, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_005)) },
        { button33, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_006)) },
        { button34, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_007)) },
        //音乐
        { button35, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_008)) },
        { button36, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_009)) },
        { button37, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_010)) },
        { button38, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_011)) },
        { button39, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_012)) },
        { button40, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_013)) },
        { button41, new MemoryStream(ReadStreamToByteArray(Properties.Resources.Music_014)) },
        // 继续为其他按钮设置音频文件路径
    };
        }
                
        private void InitializeButtons()
        {
            foreach (var button in buttonAudioMapping.Keys)
            {
                if (button != ButtonPlay)
                {
                    //button.Click += Button_Click(Button button);
                    button.Click += (s, e) => Button_Click(button);
                    button.MouseUp += Button_MouseUp;
                }
            }
        }

        private void Button_Click(Button button)
        {
            HandleButtonClick(button);
        }

        private void HandleButtonClick(Button button)
        {
            if (!buttonAudioMapping.TryGetValue(button, out MemoryStream audioStream))
            {
                MessageBox.Show("音频文件未定义");
                return;
            }

            if (waveStream == null || waveOut == null || isPlaying)
            {
                StartPlayback(audioStream);
                ButtonPlay.BackgroundImage = Properties.Resources.pause;
            }
            else
            {
                StartPlayback(audioStream);
                ButtonPlay.BackgroundImage = Properties.Resources.pause;
            }
        }

        private void CleanUp()
        {
            if (waveOut != null)
            {
                // 确保事件处理程序被解绑
                waveOut.PlaybackStopped -= OnPlaybackStopped;
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        private void StartPlayback(MemoryStream audioStream)
        {
            CleanUp();

            waveOut = new WaveOutEvent
            {
               DeviceNumber = comboBoxDevices.SelectedIndex
            };

            // 重置流位置
            audioStream.Seek(0, SeekOrigin.Begin);

            // 使用 WaveFileReader
            waveStream = new WaveFileReader(audioStream);
            waveOut.Init(waveStream);
            waveOut.PlaybackStopped += OnPlaybackStopped;
            waveOut.Play();
            isPlaying = true;

            // 确保初始化时更新进度条
            trackBarProgress.Maximum = (int)waveStream.TotalTime.TotalSeconds;
        }

        private void ButtonPlay_Click(object sender, EventArgs e)
        {
            if (waveOut == null || waveStream == null)
            {
                if (buttonAudioMapping.TryGetValue(ButtonPlay, out MemoryStream audioStream))
                {
                    StartPlayback(audioStream);
                    ButtonPlay.BackgroundImage = Properties.Resources.pause;
                }
            }
            else
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    isPlaying = false;
                    ButtonPlay.BackgroundImage = Properties.Resources.play;
                }
                else
                {
                    waveOut.Play();
                    isPlaying = true;
                    ButtonPlay.BackgroundImage = Properties.Resources.pause;
                }
            }
        }

        private void ChangeAudioFile(Button button)
        {
            // 设置只允许选择 .wav 文件
            openFileDialog.Filter = "WAV files (*.wav)|*.wav";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string newFilePath = openFileDialog.FileName;

                // 检查文件扩展名是否为 .wav（如果需要进一步验证）
                if (System.IO.Path.GetExtension(newFilePath).ToLower() != ".wav")
                {
                    MessageBox.Show("只能选择 .wav 格式的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                button.Text = System.IO.Path.GetFileNameWithoutExtension(newFilePath);

                // 加载音频文件为 MemoryStream
                byte[] audioBytes = System.IO.File.ReadAllBytes(newFilePath);
                System.IO.MemoryStream audioStream = new System.IO.MemoryStream(audioBytes);

                // 更新 buttonAudioMapping 字典
                if (buttonAudioMapping.ContainsKey(button))
                {
                    buttonAudioMapping[button].Dispose(); // 释放旧的 MemoryStream
                    buttonAudioMapping[button] = audioStream;
                }
                else
                {
                    buttonAudioMapping.Add(button, audioStream);
                }

                // 更新 appSettings 中的音频文件路径
                appSettings.ButtonAudioFilePaths[button.Name] = newFilePath;

                // 自动保存设置
                settingsManager.SaveSettings(appSettings);
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            isPlaying = false;

            // 检查是否正在播放其他音频
            if (waveOut == null || waveOut.PlaybackState != PlaybackState.Playing)
            {
                ButtonPlay.BackgroundImage = Properties.Resources.play; // 播放图片
            }
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Button button = sender as Button;
                if (button == null) return;

                ContextMenuStrip contextMenu = new ContextMenuStrip();
                ToolStripMenuItem changeAudioFileMenuItem = new ToolStripMenuItem("更改音频文件");
                ToolStripMenuItem setShortcutKeyMenuItem = new ToolStripMenuItem("设置快捷键");

                changeAudioFileMenuItem.Click += (s, args) => ChangeAudioFile(button);
                setShortcutKeyMenuItem.Click += SetShortcutKeyMenuItem_Click;

                contextMenu.Items.Add(changeAudioFileMenuItem);
                contextMenu.Items.Add(setShortcutKeyMenuItem);
                contextMenu.Show(button, e.Location);
            }
        }

        private void SetShortcutKeyMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    Button button = owner.SourceControl as Button;
                    if (button != null)
                    {
                        using (var shortcutKeyForm = new ShortcutKeyForm())
                        {
                            isSettingShortcutKey = true; // 设置状态为正在设置快捷键
                            if (shortcutKeyForm.ShowDialog() == DialogResult.OK)
                            {
                                Keys selectedKey = shortcutKeyForm.SelectedKey;

                                // 检查快捷键是否已被其他按钮使用
                                if (shortcutKeyMapping.ContainsKey(selectedKey))
                                {
                                    MessageBox.Show("此快捷键已被其他按钮使用，请选择其他快捷键。");
                                    isSettingShortcutKey = false; // 重置状态
                                    return;
                                }

                                // 如果当前按钮已经设置了快捷键，将其从映射中移除
                                var existingKey = shortcutKeyMapping.FirstOrDefault(x => x.Value == button).Key;
                                if (existingKey != Keys.None)
                                {
                                    shortcutKeyMapping.Remove(existingKey);
                                }

                                // 将新的快捷键映射到按钮
                                shortcutKeyMapping[selectedKey] = button;

                                // 更新按钮对应的 Label 文本
                                if (buttonLabelMapping.TryGetValue(button, out Label label))
                                {
                                    label.Text = $"快捷键: {selectedKey}";
                                    label.Visible = true;
                                }
                            }
                            isSettingShortcutKey = false; // 重置状态
                        }
                    }
                }
            }
        }

        private void ButtonKey_Click(object sender, EventArgs e)
        {
            isShortcutKeyEnabled = !isShortcutKeyEnabled;
            ButtonKey.Text = isShortcutKeyEnabled ? "快捷键：开启" : "快捷键：关闭";
        }

        private void comboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }

            if (!string.IsNullOrEmpty(currentFilePath))
            {
                waveOut = new WaveOutEvent
                {
                    DeviceNumber = comboBoxDevices.SelectedIndex,
                    Volume = volumeSlider.Volume
                };
                audioFileReader = new AudioFileReader(currentFilePath);
                waveOut.Init(audioFileReader);

                if (isPlaying)
                {
                    waveOut.Play();
                }
            }
        }

        private void volumeSlider_VolumeChanged(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Volume = volumeSlider.Volume;
            }
        }
                
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (waveStream != null && !isTrackBarDragging)
            {
                trackBarProgress.Maximum = (int)waveStream.TotalTime.TotalSeconds;
                trackBarProgress.Value = (int)waveStream.CurrentTime.TotalSeconds;
            }
        }

        private void trackBarProgress_MouseDown(object sender, MouseEventArgs e)
        {
            isTrackBarDragging = true;
        }

        private void trackBarProgress_MouseUp(object sender, MouseEventArgs e)
        {
            isTrackBarDragging = false;
            if (waveStream != null)
            {
                waveStream.CurrentTime = TimeSpan.FromSeconds(trackBarProgress.Value);
            }
        }

        private void trackBarProgress_ValueChanged(object sender, EventArgs e)
        {
            if (waveStream != null && !isTrackBarDragging)
            {
                waveStream.CurrentTime = TimeSpan.FromSeconds(trackBarProgress.Value);
            }
        }

        private void LoadAudioDevices()
        {
            var devices = audioDeviceManager.GetAudioDevices();
            foreach (var device in devices)
            {
                comboBoxDevices.Items.Add(device.ProductName);
            }
            if (comboBoxDevices.Items.Count > 0)
            {
                comboBoxDevices.SelectedIndex = 0;
            }
        }
    }
}
