using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Stopwatch
{
    public partial class MainForm : Form
    {
        private readonly StopwatchEngine _stopwatchEngine;
        private readonly System.Windows.Forms.Timer _uiTimer;
        private Label _timeLabel;
        private Button _startButton;
        private Button _pauseButton;
        private Button _resumeButton;
        private Button _resetButton;
        private Button _stopButton;
        private Label _statusLabel;
        private Panel _timePanel;

        public MainForm()
        {
            _stopwatchEngine = new StopwatchEngine();
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 100;
            _uiTimer.Tick += UpdateDisplay;
            
            InitializeComponent();
            UpdateButtonStates();
        }

        private void InitializeComponent()
        {
            this.Text = "⌚ Watch Stopwatch";
            this.Size = new Size(400, 450);
            this.MinimumSize = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.Paint += MainForm_Paint;
            this.Resize += MainForm_Resize;

            _timePanel = new Panel
            {
                BackColor = Color.Transparent
            };
            _timePanel.Paint += WatchFace_Paint;

            _timeLabel = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _statusLabel = new Label
            {
                Text = "Ready to Start",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 170),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _timePanel.Controls.AddRange(new Control[] { _timeLabel, _statusLabel });

            _startButton = CreateWatchButton("▶", Color.FromArgb(46, 204, 113), StartButton_Click);
            _pauseButton = CreateWatchButton("⏸", Color.FromArgb(241, 196, 15), PauseButton_Click);
            _resumeButton = CreateWatchButton("▶", Color.FromArgb(52, 152, 219), ResumeButton_Click);
            _resetButton = CreateWatchButton("↻", Color.FromArgb(155, 89, 182), ResetButton_Click);
            _stopButton = CreateWatchButton("⏹", Color.FromArgb(231, 76, 60), StopButton_Click);

            PositionControls();

            this.Controls.AddRange(new Control[] 
            { 
                _timePanel, _startButton, _pauseButton, _resumeButton, 
                _resetButton, _stopButton 
            });
        }

        private Button CreateWatchButton(string text, Color color, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(60, 35),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            return button;
        }

        private void PositionControls()
        {
            var centerX = this.ClientSize.Width / 2;
            var centerY = this.ClientSize.Height / 2;
            var watchSize = Math.Min(this.ClientSize.Width - 100, this.ClientSize.Height - 150);
            
            _timePanel.Location = new Point(centerX - watchSize/2, 50);
            _timePanel.Size = new Size(watchSize, watchSize);
            
            _timeLabel.Location = new Point(0, watchSize/2 - 25);
            _timeLabel.Size = new Size(watchSize, 50);
            _statusLabel.Location = new Point(0, watchSize/2 + 30);
            _statusLabel.Size = new Size(watchSize, 20);
            
            var topButtonY = _timePanel.Bottom + 15;
            var bottomButtonY = topButtonY + 50;
            
            _startButton.Location = new Point(centerX - 95, topButtonY);
            _pauseButton.Location = new Point(centerX - 30, topButtonY);
            _resumeButton.Location = new Point(centerX + 35, topButtonY);
            
            _resetButton.Location = new Point(centerX - 65, bottomButtonY);
            _stopButton.Location = new Point(centerX + 5, bottomButtonY);
        }
        
        private void MainForm_Resize(object sender, EventArgs e)
        {
            PositionControls();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(this.ClientRectangle, 
                Color.FromArgb(25, 25, 35), Color.FromArgb(45, 45, 65), 45f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void WatchFace_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var size = Math.Min(_timePanel.Width, _timePanel.Height) - 20;
            var rect = new Rectangle(10, 10, size, size);
            
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(70, 70, 90), Color.FromArgb(30, 30, 50), 45f))
            {
                e.Graphics.FillEllipse(brush, rect);
            }
            
            using (var pen = new Pen(Color.FromArgb(100, 100, 120), 4))
            {
                e.Graphics.DrawEllipse(pen, rect);
            }
            
            var center = new PointF(rect.X + rect.Width/2, rect.Y + rect.Height/2);
            var radius = rect.Width / 2;
            for (int i = 0; i < 12; i++)
            {
                var angle = i * 30 * Math.PI / 180;
                var x1 = center.X + (radius - 10) * Math.Cos(angle - Math.PI / 2);
                var y1 = center.Y + (radius - 10) * Math.Sin(angle - Math.PI / 2);
                var x2 = center.X + (radius - 20) * Math.Cos(angle - Math.PI / 2);
                var y2 = center.Y + (radius - 20) * Math.Sin(angle - Math.PI / 2);
                
                using (var pen = new Pen(Color.FromArgb(150, 150, 170), 2))
                {
                    e.Graphics.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
            }
            
            var glowColor = _stopwatchEngine.IsRunning ? 
                Color.FromArgb(120, 46, 204, 113) :
                _stopwatchEngine.IsPaused ?
                Color.FromArgb(100, 241, 196, 15) :
                Color.FromArgb(80, 52, 152, 219);
                
            using (var pen = new Pen(glowColor, 4))
            {
                e.Graphics.DrawEllipse(pen, rect);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Start();
            _uiTimer.Start();
            _statusLabel.Text = "⚡ Running...";
            _statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            UpdateButtonStates();
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Pause();
            _uiTimer.Stop();
            _statusLabel.Text = $"⏸ Paused at {_stopwatchEngine.ElapsedTime}";
            _statusLabel.ForeColor = Color.FromArgb(241, 196, 15);
            UpdateButtonStates();
        }

        private void ResumeButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Resume();
            _uiTimer.Start();
            _statusLabel.Text = "⚡ Running...";
            _statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            UpdateButtonStates();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Reset();
            _uiTimer.Stop();
            _timeLabel.Text = "00:00:00";
            _statusLabel.Text = "🔄 Reset Complete";
            _statusLabel.ForeColor = Color.FromArgb(155, 89, 182);
            UpdateButtonStates();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            var finalTime = _stopwatchEngine.Stop();
            _uiTimer.Stop();
            _statusLabel.Text = $"⏹ Stopped at {finalTime}";
            _statusLabel.ForeColor = Color.FromArgb(231, 76, 60);
            UpdateButtonStates();
        }

        private void UpdateDisplay(object sender, EventArgs e)
        {
            _timeLabel.Text = _stopwatchEngine.ElapsedTime;
            _timePanel.Invalidate();
        }

        private void UpdateButtonStates()
        {
            _startButton.Enabled = !_stopwatchEngine.IsRunning && !_stopwatchEngine.IsPaused;
            _pauseButton.Enabled = _stopwatchEngine.IsRunning;
            _resumeButton.Enabled = _stopwatchEngine.IsPaused;
            _resetButton.Enabled = true;
            _stopButton.Enabled = _stopwatchEngine.IsRunning || _stopwatchEngine.IsPaused;
        }
    }
}