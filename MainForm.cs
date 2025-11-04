using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Stopwatch
{
    /// <summary>
    /// Double-buffered panel for smooth rendering
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);
        }
    }

    /// <summary>
    /// Main form for the Stopwatch application providing user interface
    /// </summary>
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
        private DoubleBufferedPanel _clockPanel;
        private Panel _timeDisplayPanel;

        /// <summary>
        /// Initializes a new instance of the MainForm class
        /// </summary>
        public MainForm()
        {
            _stopwatchEngine = new StopwatchEngine();
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 16; // ~60fps for smooth animation
            _uiTimer.Tick += UpdateDisplay;
            
            InitializeComponent();
            UpdateButtonStates();
        }

        /// <summary>
        /// Initializes the form components and layout
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "⌚ Watch Stopwatch";
            this.Size = new Size(500, 650);
            this.MinimumSize = new Size(450, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Paint += MainForm_Paint;
            this.Resize += MainForm_Resize;

            // Time display panel above the clock
            _timeDisplayPanel = new Panel
            {
                BackColor = Color.Transparent
            };

            _timeLabel = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 20, 147), // Pink/Magenta color
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            // textual label visible by default (simple digits)
            _timeLabel.Visible = true;

            _statusLabel = new Label
            {
                Text = "Ready to Start",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            _timeDisplayPanel.Controls.AddRange(new Control[] { _timeLabel, _statusLabel });

            // Clock panel for analog stopwatch with double buffering
            _clockPanel = new DoubleBufferedPanel
            {
                BackColor = Color.Transparent
            };
            _clockPanel.Paint += WatchFace_Paint;

            // Pink buttons with text labels
            var pinkColor = Color.FromArgb(255, 20, 147); // Pink/Magenta color
            _startButton = CreateWatchButton("Start", pinkColor, StartButton_Click);
            _pauseButton = CreateWatchButton("Pause", pinkColor, PauseButton_Click);
            _resumeButton = CreateWatchButton("Resume", pinkColor, ResumeButton_Click);
            _resetButton = CreateWatchButton("Reset", pinkColor, ResetButton_Click);
            _stopButton = CreateWatchButton("Stop", pinkColor, StopButton_Click);

            PositionControls();

            this.Controls.AddRange(new Control[] 
            { 
                _timeDisplayPanel, _clockPanel, _startButton, _pauseButton, _resumeButton, 
                _resetButton, _stopButton 
            });
        }

        /// <summary>
        /// Creates a pink oval button with rounded corners matching the reference design
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="color">Button color</param>
        /// <param name="clickHandler">Click event handler</param>
        /// <returns>Configured button control</returns>
        private Button CreateWatchButton(string text, Color color, EventHandler clickHandler)
        {
            var button = new Button
            {
                // keep actual Button.Text empty so the system doesn't render any default text
                Text = string.Empty,
                Size = new Size(110, 48),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            // store the logical label in Tag for our custom paint
            button.Tag = text;
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;

            // Make buttons square-oval (rounded rectangle with small radius)
            void UpdateRegion()
            {
                var radius = 8;
                var path = new GraphicsPath();
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(button.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(button.Width - radius * 2, button.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, button.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                button.Region = new Region(path);
            }

            UpdateRegion();
            // update region on resize
            button.SizeChanged += (s, e) => UpdateRegion();

            // Custom paint to draw icon above text and handle disabled state
            button.Paint += (s, e) =>
            {
                var btn = s as Button;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);

                // Draw button background with disabled state handling
                var bgColor = btn.Enabled ? btn.BackColor : Color.FromArgb(200, btn.BackColor);
                using (var brush = new SolidBrush(bgColor))
                {
                    using (var path = new GraphicsPath())
                    {
                        var radius = 8;
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        path.CloseFigure();
                        e.Graphics.FillPath(brush, path);
                    }
                }

                // Draw icon above text. Use the stored label from Tag so Button.Text remains empty.
                var label = (btn.Tag as string) ?? string.Empty;
                var icon = GetButtonIcon(label);
                using (var iconFont = new Font("Segoe UI Symbol", 16, FontStyle.Regular))
                using (var textFont = new Font(btn.Font.FontFamily, 10, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    var iconColor = btn.Enabled ? Color.White : Color.FromArgb(220, 220, 220);
                    var textColor = btn.Enabled ? Color.White : Color.FromArgb(220, 220, 220);

                    var iconRect = new RectangleF(rect.X, rect.Y + 4, rect.Width, rect.Height * 0.55f);
                    e.Graphics.DrawString(icon, iconFont, new SolidBrush(iconColor), iconRect, sf);

                    var labelRect = new RectangleF(rect.X, rect.Y + rect.Height * 0.5f, rect.Width, rect.Height * 0.45f);
                    // draw only the white text (no background/secondary text)
                    e.Graphics.DrawString(label, textFont, new SolidBrush(textColor), labelRect, sf);
                }
            };

            return button;
        }

        private string GetButtonIcon(string buttonText)
        {
            return buttonText switch
            {
                "Start" => "▶",
                "Pause" => "⏸",
                "Resume" => "▶",
                "Reset" => "⟲",
                "Stop" => "⏹",
                _ => ""
            };
        }

        /// <summary>
        /// Positions all controls based on current form size
        /// </summary>
        private void PositionControls()
        {
            var centerX = this.ClientSize.Width / 2;
            var padding = 40;
            
            // Time display panel at the top
            _timeDisplayPanel.Location = new Point(0, padding);
            _timeDisplayPanel.Size = new Size(this.ClientSize.Width, 100);
            
            _timeLabel.Location = new Point(0, 10);
            _timeLabel.Size = new Size(this.ClientSize.Width, 60);
            _statusLabel.Location = new Point(0, 70);
            _statusLabel.Size = new Size(this.ClientSize.Width, 25);
            
            // Clock panel below time display
            var clockSize = Math.Min(this.ClientSize.Width - 80, 280);
            var clockY = _timeDisplayPanel.Bottom + 30;
            _clockPanel.Location = new Point(centerX - clockSize/2, clockY);
            _clockPanel.Size = new Size(clockSize, clockSize);
            
            // Buttons below the clock
            var buttonY = _clockPanel.Bottom + 30;
            var buttonSpacing = 15;
            var totalButtonWidth = (_startButton.Width * 3) + (buttonSpacing * 2);
            var buttonStartX = centerX - totalButtonWidth / 2;
            
            _startButton.Location = new Point(buttonStartX, buttonY);
            _pauseButton.Location = new Point(buttonStartX + _startButton.Width + buttonSpacing, buttonY);
            _resumeButton.Location = new Point(buttonStartX + (_startButton.Width + buttonSpacing) * 2, buttonY);
            
            var bottomButtonY = buttonY + _startButton.Height + 15;
            var bottomTotalWidth = (_resetButton.Width * 2) + buttonSpacing;
            var bottomButtonStartX = centerX - bottomTotalWidth / 2;
            
            _resetButton.Location = new Point(bottomButtonStartX, bottomButtonY);
            _stopButton.Location = new Point(bottomButtonStartX + _resetButton.Width + buttonSpacing, bottomButtonY);
        }
        
        /// <summary>
        /// Handles form resize events
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            PositionControls();
        }

        /// <summary>
        /// Paints the main form with light background
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // Light gray at top transitioning to white
            using (var brush = new LinearGradientBrush(this.ClientRectangle, 
                Color.FromArgb(240, 240, 240), Color.White, 90f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Paints the analog stopwatch face with translucent red trail matching the reference image
        /// </summary>
        private void WatchFace_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            
            var panelSize = Math.Min(_clockPanel.Width, _clockPanel.Height);
            var padding = 20;
            var clockSize = panelSize - (padding * 2);
            var rect = new Rectangle(padding, padding, clockSize, clockSize);
            
            var center = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
            var radius = clockSize / 2f;
            
            // Draw silver metallic casing with gradient
            var bezelPath = new GraphicsPath();
            bezelPath.AddEllipse(rect);
            using (var pathBrush = new PathGradientBrush(bezelPath))
            {
                pathBrush.CenterPoint = new PointF(center.X - clockSize * 0.15f, center.Y - clockSize * 0.15f);
                pathBrush.CenterColor = Color.FromArgb(255, 240, 240, 240);
                pathBrush.SurroundColors = new[] { Color.FromArgb(255, 170, 170, 170) };
                e.Graphics.FillEllipse(pathBrush, rect);
            }
            
            // Add metallic highlight
            var highlightRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height / 2);
            using (var highlightBrush = new LinearGradientBrush(highlightRect,
                Color.FromArgb(100, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 45f))
            {
                e.Graphics.FillEllipse(highlightBrush, rect);
            }
            
            // Draw white dial face
            var dialRect = new Rectangle(rect.X + 10, rect.Y + 10, rect.Width - 20, rect.Height - 20);
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillEllipse(brush, dialRect);
            }
            
            var dialRadius = dialRect.Width / 2f - 15;
            
            // Calculate elapsed time first for red shaded area
            var elapsed = _stopwatchEngine.ElapsedTimeSpan;
            var totalSeconds = elapsed.TotalSeconds;
            var seconds = totalSeconds % 60;
            var minutes = elapsed.TotalMinutes;
            
            // Draw translucent light pink shaded area from "60" (top) clockwise to current second position
            // The shaded area should fill based on total elapsed time (seconds + minutes)
            var totalElapsedSeconds = totalSeconds % 60; // Current position within the minute
            if (totalElapsedSeconds > 0 || totalSeconds > 0)
            {
                var shadedAngle = (float)(totalElapsedSeconds * 6); // 6 degrees per second
                
                // Create a pie slice from top (60) to current second position
                var shadedRect = new Rectangle(dialRect.X, dialRect.Y, dialRect.Width, dialRect.Height);
                using (var shadedBrush = new SolidBrush(Color.FromArgb(120, 255, 192, 203))) // Translucent light pink
                {
                    e.Graphics.FillPie(shadedBrush, shadedRect, -90f, shadedAngle);
                }
            }
            
            // Draw stopwatch numbers (60, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55)
            // Positioned further from tick marks to avoid touching
            var stopwatchNumbers = new[] { "60", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55" };
            var fontSize = Math.Max(11, clockSize * 0.08f);
            using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            {
                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < 12; i++)
                {
                    var angle = (i * 30 - 90) * Math.PI / 180; // Start from top (60 at 12 o'clock)
                    // Position numbers further inward to avoid touching tick marks
                    var numberRadius = dialRadius - 18; // Further from edge
                    var x = center.X + numberRadius * Math.Cos(angle);
                    var y = center.Y + numberRadius * Math.Sin(angle);
                    var textRect = new RectangleF((float)x - 22, (float)y - 11, 44, 22);
                    e.Graphics.DrawString(stopwatchNumbers[i], font, brush, textRect, format);
                }
            }
            
            // Draw tick marks for seconds
            var tickOuterRadius = dialRadius + 6;
            for (int i = 0; i < 60; i++)
            {
                var angle = (i * 6 - 90) * Math.PI / 180;
                var tickLength = i % 5 == 0 ? 7 : 3.5f;
                var tickWidth = i % 5 == 0 ? 1.5f : 1f;
                var innerRadius = tickOuterRadius - tickLength;
                var x1 = center.X + tickOuterRadius * Math.Cos(angle);
                var y1 = center.Y + tickOuterRadius * Math.Sin(angle);
                var x2 = center.X + innerRadius * Math.Cos(angle);
                var y2 = center.Y + innerRadius * Math.Sin(angle);
                
                using (var pen = new Pen(Color.Black, tickWidth))
                {
                    e.Graphics.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
            }
            
            // Draw crown/ring at top
            var crownWidth = clockSize * 0.16f;
            var crownHeight = clockSize * 0.09f;
            var crownRect = new RectangleF(
                center.X - crownWidth / 2f,
                rect.Y - crownHeight * 0.8f,
                crownWidth,
                crownHeight);
            
            using (var brush = new LinearGradientBrush(crownRect,
                Color.FromArgb(230, 230, 230), Color.FromArgb(150, 150, 150), 90f))
            {
                e.Graphics.FillRectangle(brush, crownRect);
            }
            
            // Draw minute hand (black, shorter, thicker) - points to minutes on main dial
            var minuteAngle = (minutes * 6 - 90) * Math.PI / 180;
            var minuteHandLength = dialRadius * 0.6f;
            var minuteHandEndX = center.X + minuteHandLength * Math.Cos(minuteAngle);
            var minuteHandEndY = center.Y + minuteHandLength * Math.Sin(minuteAngle);
            var minuteHandWidth = Math.Max(3, clockSize * 0.025f);

            using (var pen = new Pen(Color.Black, minuteHandWidth))
            {
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLine(pen, center, new PointF((float)minuteHandEndX, (float)minuteHandEndY));
            }
            
            // Draw second hand (red, long, thin) - points to seconds on main dial
            var secondAngle = (seconds * 6 - 90) * Math.PI / 180;
            var secondHandLength = dialRadius * 0.92f;
            var secondHandEndX = center.X + secondHandLength * Math.Cos(secondAngle);
            var secondHandEndY = center.Y + secondHandLength * Math.Sin(secondAngle);
            var secondHandWidth = Math.Max(1.8f, clockSize * 0.013f);
            
            using (var pen = new Pen(Color.Red, secondHandWidth))
            {
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLine(pen, center, new PointF((float)secondHandEndX, (float)secondHandEndY));
            }
            
            // Draw center pivot point
            var pivotSize = clockSize * 0.04f;
            var pivotRect = new RectangleF(center.X - pivotSize / 2, center.Y - pivotSize / 2, pivotSize, pivotSize);
            using (var brush = new SolidBrush(Color.Black))
            {
                e.Graphics.FillEllipse(brush, pivotRect);
            }
        }

        /// <summary>
        /// Handles the Start button click event
        /// </summary>
        private void StartButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Start();
            _uiTimer.Start();
            _statusLabel.Text = "Running...";
            _statusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            UpdateButtonStates();
            _clockPanel.Invalidate();
        }

        /// <summary>
        /// Handles the Pause button click event
        /// </summary>
        private void PauseButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Pause();
            _uiTimer.Stop();
            _statusLabel.Text = $"Paused at {_stopwatchEngine.ElapsedTime}";
            _statusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            UpdateButtonStates();
            _clockPanel.Invalidate();
        }

        /// <summary>
        /// Handles the Resume button click event
        /// </summary>
        private void ResumeButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Resume();
            _uiTimer.Start();
            _statusLabel.Text = "Running...";
            _statusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            UpdateButtonStates();
            _clockPanel.Invalidate();
        }

        /// <summary>
        /// Handles the Reset button click event
        /// </summary>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            _stopwatchEngine.Reset();
            _uiTimer.Stop();
            _timeLabel.Text = "00:00:00";
            _statusLabel.Text = "Reset Complete";
            _statusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            UpdateButtonStates();
            _clockPanel.Invalidate();
        }

        /// <summary>
        /// Handles the Stop button click event
        /// </summary>
        private void StopButton_Click(object sender, EventArgs e)
        {
            var finalTime = _stopwatchEngine.Stop();
            _uiTimer.Stop();
            _statusLabel.Text = $"Stopped at {finalTime}";
            _statusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            UpdateButtonStates();
            _clockPanel.Invalidate();
        }

        /// <summary>
        /// Updates the display with current time and redraws the clock
        /// </summary>
        private void UpdateDisplay(object sender, EventArgs e)
        {
            _timeLabel.Text = _stopwatchEngine.ElapsedTime;
            _clockPanel.Invalidate(); // Redraw clock with animated hands
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
        }

        
    }
}