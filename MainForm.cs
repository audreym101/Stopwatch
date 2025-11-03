using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Stopwatch
{
    /// <summary>
    /// Main form for the Stopwatch application providing user interface
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly StopwatchEngine _stopwatchEngine;
        private readonly System.Windows.Forms.Timer _uiTimer;
        private readonly System.Windows.Forms.Timer _animationTimer;
        private Label _timeLabel;
        private RoundButton _startButton;
        private RoundButton _pauseButton;
        private RoundButton _resumeButton;
        private RoundButton _resetButton;
        private RoundButton _stopButton;
        private Label _statusLabel;
        private Panel _timePanel;
        private int _pulseDirection = 1;
        private float _pulseAlpha = 0.5f;

        /// <summary>
        /// Initializes a new instance of the MainForm class
        /// </summary>
        public MainForm()
        {
            _stopwatchEngine = new StopwatchEngine();
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 100;
            _uiTimer.Tick += UpdateDisplay;
            
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 50;
            _animationTimer.Tick += AnimationTick;
            _animationTimer.Start();
            
            InitializeComponent();
            UpdateButtonStates();
        }

        /// <summary>
        /// Initializes the form components and layout
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "⏱️ Modern Stopwatch";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.Paint += MainForm_Paint;

            // Time panel with gradient background
            _timePanel = new Panel
            {
                Location = new Point(50, 40),
                Size = new Size(400, 120),
                BackColor = Color.Transparent
            };
            _timePanel.Paint += TimePanel_Paint;

            // Time display label
            _timeLabel = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 30),
                Size = new Size(400, 60)
            };

            // Status label
            _statusLabel = new Label
            {
                Text = "Ready to Start",
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 170),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 85),
                Size = new Size(400, 25)
            };

            _timePanel.Controls.AddRange(new Control[] { _timeLabel, _statusLabel });

            // Create modern rounded buttons
            _startButton = CreateModernButton("▶ START", new Point(80, 200), Color.FromArgb(46, 204, 113), StartButton_Click);
            _pauseButton = CreateModernButton("⏸ PAUSE", new Point(200, 200), Color.FromArgb(241, 196, 15), PauseButton_Click);
            _resumeButton = CreateModernButton("▶ RESUME", new Point(320, 200), Color.FromArgb(52, 152, 219), ResumeButton_Click);
            _resetButton = CreateModernButton("🔄 RESET", new Point(140, 260), Color.FromArgb(155, 89, 182), ResetButton_Click);
            _stopButton = CreateModernButton("⏹ STOP", new Point(260, 260), Color.FromArgb(231, 76, 60), StopButton_Click);

            // Add close button
            var closeButton = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(231, 76, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(460, 10)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] 
            { 
                _timePanel, _startButton, _pauseButton, _resumeButton, 
                _resetButton, _stopButton, closeButton 
            });
        }

        /// <summary>
        /// Creates a modern rounded button with gradient styling
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="location">Button location</param>
        /// <param name="color">Button color</param>
        /// <param name="clickHandler">Click event handler</param>
        /// <returns>Configured modern button control</returns>
        private RoundButton CreateModernButton(string text, Point location, Color color, EventHandler clickHandler)
        {
            var button = new RoundButton
            {
                Text = text,
                Size = new Size(100, 45),
                Location = location,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            return button;
        }

        /// <summary>
        /// Paints the main form with gradient background
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(this.ClientRectangle, 
                Color.FromArgb(25, 25, 35), Color.FromArgb(45, 45, 65), 45f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Paints the time panel with animated glow effect
        /// </summary>
        private void TimePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = _timePanel.ClientRectangle;
            rect.Inflate(-2, -2);
            
            using (var path = GetRoundedRectPath(rect, 20))
            {
                // Gradient background
                using (var brush = new LinearGradientBrush(rect, 
                    Color.FromArgb(60, 60, 80), Color.FromArgb(40, 40, 60), 90f))
                {
                    e.Graphics.FillPath(brush, path);
                }
                
                // Smooth glow border
                var baseAlpha = _stopwatchEngine.IsRunning ? _pulseAlpha : 0.5f;
                var glowColor = _stopwatchEngine.IsRunning ? 
                    Color.FromArgb((int)(255 * baseAlpha), 46, 204, 113) :
                    Color.FromArgb(128, 52, 152, 219);
                    
                using (var pen = new Pen(glowColor, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// Creates a rounded rectangle path
        /// </summary>
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Handles animation timer tick for glow effects
        /// </summary>
        private void AnimationTick(object sender, EventArgs e)
        {
            _pulseAlpha += 0.01f * _pulseDirection;
            if (_pulseAlpha >= 0.7f || _pulseAlpha <= 0.4f)
                _pulseDirection *= -1;
                
            if (_stopwatchEngine.IsRunning)
                _timePanel?.Invalidate();
        }

        /// <summary>
        /// Handles the Start button click event
        /// </summary>
        private void StartButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Start();
            _uiTimer.Start();
            _statusLabel.Text = "⚡ Running...";
            _statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Pause button click event
        /// </summary>
        private void PauseButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Pause();
            _uiTimer.Stop();
            _statusLabel.Text = $"⏸ Paused at {_stopwatchEngine.ElapsedTime}";
            _statusLabel.ForeColor = Color.FromArgb(241, 196, 15);
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Resume button click event
        /// </summary>
        private void ResumeButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Resume();
            _uiTimer.Start();
            _statusLabel.Text = "⚡ Running...";
            _statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Reset button click event
        /// </summary>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Reset();
            _uiTimer.Stop();
            _timeLabel.Text = "00:00:00";
            _statusLabel.Text = "🔄 Reset Complete";
            _statusLabel.ForeColor = Color.FromArgb(155, 89, 182);
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the Stop button click event
        /// </summary>
        private void StopButton_Click(object sender, EventArgs e)
        {
            var finalTime = _stopwatchEngine.Stop();
            _uiTimer.Stop();
            _statusLabel.Text = $"⏹ Stopped at {finalTime}";
            _statusLabel.ForeColor = Color.FromArgb(231, 76, 60);
            UpdateButtonStates();
        }

        /// <summary>
        /// Updates the display with current time
        /// </summary>
        private void UpdateDisplay(object sender, EventArgs e)
        {
            _timeLabel.Text = _stopwatchEngine.ElapsedTime;
            _timePanel?.Invalidate(); // Trigger repaint for glow effect
        }

        /// <summary>
        /// Updates button enabled states based on stopwatch status
        /// </summary>
        private void UpdateButtonStates()
        {
            _startButton.Enabled = !_stopwatchEngine.IsRunning && !_stopwatchEngine.IsPaused;
            _pauseButton.Enabled = _stopwatchEngine.IsRunning;
            _resumeButton.Enabled = _stopwatchEngine.IsPaused;
            _resetButton.Enabled = true;
            _stopButton.Enabled = _stopwatchEngine.IsRunning || _stopwatchEngine.IsPaused;
            
            // Update button opacity based on state
            _startButton.Enabled = _startButton.Enabled;
            _pauseButton.Enabled = _pauseButton.Enabled;
            _resumeButton.Enabled = _resumeButton.Enabled;
            _stopButton.Enabled = _stopButton.Enabled;
        }
    }

    /// <summary>
    /// Custom rounded button control with modern styling
    /// </summary>
    public class RoundButton : Button
    {
        protected override void OnPaint(PaintEventArgs pevent)
        {
            var rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            
            using (var path = GetRoundedRectPath(rect, 15))
            {
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Button background with gradient
                var color1 = this.Enabled ? this.BackColor : Color.FromArgb(100, 100, 100);
                var color2 = this.Enabled ? 
                    Color.FromArgb(Math.Max(0, color1.R - 30), Math.Max(0, color1.G - 30), Math.Max(0, color1.B - 30)) :
                    Color.FromArgb(80, 80, 80);
                    
                using (var brush = new LinearGradientBrush(rect, color1, color2, 90f))
                {
                    pevent.Graphics.FillPath(brush, path);
                }
                
                // Button border
                using (var pen = new Pen(Color.FromArgb(50, Color.White), 1))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }
                
                // Button text
                var textColor = this.Enabled ? this.ForeColor : Color.FromArgb(150, 150, 150);
                using (var brush = new SolidBrush(textColor))
                {
                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    pevent.Graphics.DrawString(this.Text, this.Font, brush, rect, stringFormat);
                }
            }
        }
        
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}