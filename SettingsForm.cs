using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IconCarousel
{
    public partial class SettingsForm : Form
    {
        public ConfigModel Config { get; private set; }
        
        private ListBox _iconListBox = null!;
        private NumericUpDown _intervalNumeric = null!;
        private CheckBox _autoStartCheckBox = null!;
        private Button _addIconButton = null!;
        private Button _removeIconButton = null!;
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        public SettingsForm(ConfigModel config)
        {
            Config = new ConfigModel
            {
                IconPaths = new List<string>(config.IconPaths),
                IntervalMilliseconds = config.IntervalMilliseconds,
                AutoStart = config.AutoStart
            };
            
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Text = "托盘图标轮播设置";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var iconLabel = new Label
            {
                Text = "图标列表：",
                Location = new Point(12, 15),
                Size = new Size(100, 23),
                AutoSize = true
            };

            _iconListBox = new ListBox
            {
                Location = new Point(12, 40),
                Size = new Size(350, 150),
                HorizontalScrollbar = true
            };

            _addIconButton = new Button
            {
                Text = "添加图标",
                Location = new Point(370, 40),
                Size = new Size(100, 30)
            };
            _addIconButton.Click += AddIconButton_Click;

            _removeIconButton = new Button
            {
                Text = "删除图标",
                Location = new Point(370, 80),
                Size = new Size(100, 30)
            };
            _removeIconButton.Click += RemoveIconButton_Click;

            _moveUpButton = new Button
            {
                Text = "上移",
                Location = new Point(370, 120),
                Size = new Size(100, 30)
            };
            _moveUpButton.Click += MoveUpButton_Click;

            _moveDownButton = new Button
            {
                Text = "下移",
                Location = new Point(370, 160),
                Size = new Size(100, 30)
            };
            _moveDownButton.Click += MoveDownButton_Click;

            var intervalLabel = new Label
            {
                Text = "切换间隔（毫秒）：",
                Location = new Point(12, 210),
                Size = new Size(120, 23),
                AutoSize = true
            };

            _intervalNumeric = new NumericUpDown
            {
                Location = new Point(140, 207),
                Size = new Size(80, 23),
                Minimum = 50,
                Maximum = 3600000,
                Value = 3000
            };

            _autoStartCheckBox = new CheckBox
            {
                Text = "程序启动时自动开始轮播",
                Location = new Point(12, 240),
                Size = new Size(200, 23),
                AutoSize = true
            };

            _okButton = new Button
            {
                Text = "确定",
                Location = new Point(315, 320),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(395, 320),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[]
            {
                iconLabel,
                _iconListBox,
                _addIconButton,
                _removeIconButton,
                _moveUpButton,
                _moveDownButton,
                intervalLabel,
                _intervalNumeric,
                _autoStartCheckBox,
                _okButton,
                _cancelButton
            });

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void LoadSettings()
        {
            _iconListBox.Items.Clear();
            foreach (var iconPath in Config.IconPaths)
            {
                _iconListBox.Items.Add(Path.GetFileName(iconPath) + " - " + iconPath);
            }

            _intervalNumeric.Value = Config.IntervalMilliseconds;
            _autoStartCheckBox.Checked = Config.AutoStart;
        }

        private void AddIconButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "选择图标文件",
                Filter = "图标文件|*.ico;*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (!Config.IconPaths.Contains(fileName))
                    {
                        Config.IconPaths.Add(fileName);
                        _iconListBox.Items.Add(Path.GetFileName(fileName) + " - " + fileName);
                    }
                }
            }
        }

        private void RemoveIconButton_Click(object? sender, EventArgs e)
        {
            if (_iconListBox.SelectedIndex >= 0)
            {
                var index = _iconListBox.SelectedIndex;
                Config.IconPaths.RemoveAt(index);
                _iconListBox.Items.RemoveAt(index);
                
                if (_iconListBox.Items.Count > 0)
                {
                    if (index >= _iconListBox.Items.Count)
                        _iconListBox.SelectedIndex = _iconListBox.Items.Count - 1;
                    else
                        _iconListBox.SelectedIndex = index;
                }
            }
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            var selectedIndex = _iconListBox.SelectedIndex;
            if (selectedIndex > 0)
            {
                var temp = Config.IconPaths[selectedIndex];
                Config.IconPaths[selectedIndex] = Config.IconPaths[selectedIndex - 1];
                Config.IconPaths[selectedIndex - 1] = temp;

                LoadIconList();
                _iconListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            var selectedIndex = _iconListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _iconListBox.Items.Count - 1)
            {
                var temp = Config.IconPaths[selectedIndex];
                Config.IconPaths[selectedIndex] = Config.IconPaths[selectedIndex + 1];
                Config.IconPaths[selectedIndex + 1] = temp;

                LoadIconList();
                _iconListBox.SelectedIndex = selectedIndex + 1;
            }
        }

        private void LoadIconList()
        {
            _iconListBox.Items.Clear();
            foreach (var iconPath in Config.IconPaths)
            {
                _iconListBox.Items.Add(Path.GetFileName(iconPath) + " - " + iconPath);
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            Config.IntervalMilliseconds = (int)_intervalNumeric.Value;
            Config.AutoStart = _autoStartCheckBox.Checked;
        }
    }
}
