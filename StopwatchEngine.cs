using System;
using System.Diagnostics;

namespace Stopwatch
{
    /// <summary>
    /// Core engine for stopwatch functionality with precise timing operations
    /// </summary>
    public class StopwatchEngine
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch;
        private TimeSpan _pausedTime;
        private bool _isPaused;

        /// <summary>
        /// Initializes a new instance of the StopwatchEngine class
        /// </summary>
        public StopwatchEngine()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;
        }

        /// <summary>
        /// Gets the current elapsed time in the format 00:00:00 (hh:mm:ss)
        /// </summary>
        public string ElapsedTime
        {
            get
            {
                var totalElapsed = _stopwatch.Elapsed + _pausedTime;
                return $"{totalElapsed.Hours:00}:{totalElapsed.Minutes:00}:{totalElapsed.Seconds:00}";
            }
        }

        /// <summary>
        /// Gets the total elapsed time as a TimeSpan for clock hand calculations
        /// </summary>
        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                return _stopwatch.Elapsed + _pausedTime;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stopwatch is currently running
        /// </summary>
        public bool IsRunning => _stopwatch.IsRunning;

        /// <summary>
        /// Gets a value indicating whether the stopwatch is currently paused
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Starts the stopwatch from 00:00:00
        /// </summary>
        public void Start()
        {
            if (!_stopwatch.IsRunning && !_isPaused)
            {
                _stopwatch.Start();
            }
        }

        /// <summary>
        /// Pauses the stopwatch and preserves the current time
        /// </summary>
        public void Pause()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                _pausedTime += _stopwatch.Elapsed;
                _stopwatch.Reset();
                _isPaused = true;
            }
        }

        /// <summary>
        /// Resumes the stopwatch from the last paused time
        /// </summary>
        public void Resume()
        {
            if (_isPaused)
            {
                _stopwatch.Start();
                _isPaused = false;
            }
        }

        /// <summary>
        /// Resets the stopwatch back to 00:00:00
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;
        }

        /// <summary>
        /// Stops the stopwatch completely and returns the final time
        /// </summary>
        /// <returns>The final elapsed time as a formatted string</returns>
        public string Stop()
        {
            var finalTime = ElapsedTime;
            _stopwatch.Stop();
            _stopwatch.Reset();
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;
            return finalTime;
        }
    }
}