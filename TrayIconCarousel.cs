using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IconCarousel
{
    public class TrayIconCarousel : ApplicationContext
    {
        private NotifyIcon _notifyIcon = null!;
        private Timer _carouselTimer = null!;
        private List<string> _iconPaths = null!;
        private int _currentIconIndex;
        private ConfigModel _config = null!;
        private readonly string _configPath = "config.json";

        public TrayIconCarousel()
        {
            InitializeComponent();
            LoadConfiguration();
            SetupCarousel();
        }

        private void InitializeComponent()
        {
            _notifyIcon = new NotifyIcon();
            _carouselTimer = new Timer();
            _iconPaths = new List<string>();
            _currentIconIndex = 0;

            _notifyIcon.Text = "托盘图标轮播";
            _notifyIcon.Visible = true;

            _carouselTimer.Tick += CarouselTimer_Tick;

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            var settingsItem = new ToolStripMenuItem("设置");
            settingsItem.Click += SettingsItem_Click;
            
            var separator1 = new ToolStripSeparator();
            
            var startItem = new ToolStripMenuItem("开始轮播");
            startItem.Click += StartItem_Click;
            
            var stopItem = new ToolStripMenuItem("停止轮播");
            stopItem.Click += StopItem_Click;
            
            var separator2 = new ToolStripSeparator();
            
            var aboutItem = new ToolStripMenuItem("关于");
            aboutItem.Click += AboutItem_Click;
            
            var exitItem = new ToolStripMenuItem("退出");
            exitItem.Click += ExitItem_Click;

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                settingsItem,
                separator1,
                startItem,
                stopItem,
                separator2,
                aboutItem,
                exitItem
            });

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();
                }
                else
                {
                    _config = new ConfigModel();
                    SaveConfiguration();
                }
            }
            catch
            {
                _config = new ConfigModel();
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置文件失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupCarousel()
        {
            LoadIcons();
            
            if (_iconPaths.Count > 0)
            {
                SetCurrentIcon();
                _carouselTimer.Interval = _config.IntervalSeconds * 1000;
                
                if (_config.AutoStart)
                {
                    _carouselTimer.Start();
                }
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
                MessageBox.Show("未找到图标文件，请通过设置添加图标。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadIcons()
        {
            _iconPaths.Clear();
            
            foreach (var iconPath in _config.IconPaths)
            {
                if (File.Exists(iconPath))
                {
                    _iconPaths.Add(iconPath);
                }
            }

            if (_iconPaths.Count == 0)
            {
                var iconsDir = Path.Combine(Application.StartupPath, "Icons");
                if (Directory.Exists(iconsDir))
                {
                    var iconFiles = Directory.GetFiles(iconsDir, "*.ico")
                        .Concat(Directory.GetFiles(iconsDir, "*.png"))
                        .Concat(Directory.GetFiles(iconsDir, "*.jpg"))
                        .Concat(Directory.GetFiles(iconsDir, "*.jpeg"))
                        .Concat(Directory.GetFiles(iconsDir, "*.bmp"));
                    
                    _iconPaths.AddRange(iconFiles);
                }
            }
        }

        private void SetCurrentIcon()
        {
            if (_iconPaths.Count == 0) return;

            try
            {
                var iconPath = _iconPaths[_currentIconIndex];
                
                if (Path.GetExtension(iconPath).ToLower() == ".ico")
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    using (var bitmap = new Bitmap(iconPath))
                    {
                        var resized = new Bitmap(bitmap, new Size(16, 16));
                        _notifyIcon.Icon = Icon.FromHandle(resized.GetHicon());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图标失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CarouselTimer_Tick(object? sender, EventArgs e)
        {
            if (_iconPaths.Count <= 1) return;

            _currentIconIndex = (_currentIconIndex + 1) % _iconPaths.Count;
            SetCurrentIcon();
        }

        private void SettingsItem_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_config);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _config = settingsForm.Config;
                SaveConfiguration();
                
                _carouselTimer.Stop();
                SetupCarousel();
            }
        }

        private void StartItem_Click(object? sender, EventArgs e)
        {
            if (_iconPaths.Count > 1)
            {
                _carouselTimer.Start();
                _notifyIcon.ShowBalloonTip(2000, "托盘图标轮播", "轮播已开始", ToolTipIcon.Info);
            }
            else
            {
                MessageBox.Show("需要至少2个图标才能开始轮播", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void StopItem_Click(object? sender, EventArgs e)
        {
            _carouselTimer.Stop();
            _notifyIcon.ShowBalloonTip(2000, "托盘图标轮播", "轮播已停止", ToolTipIcon.Info);
        }

        private void AboutItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("托盘图标轮播软件 v1.0\n\n功能特点：\n• 定时切换托盘图标\n• 可自定义图标和时间间隔\n• 支持多种图片格式\n• 配置自动保存", 
                          "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitItem_Click(object? sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _carouselTimer?.Stop();
            _carouselTimer?.Dispose();
            _notifyIcon?.Dispose();
            base.ExitThreadCore();
        }
    }
}
