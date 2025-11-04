using System;
using System.Diagnostics;

namespace Stopwatch
{
    public class StopwatchEngine
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch;
        private TimeSpan _pausedTime;
        private bool _isPaused;

        public StopwatchEngine()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;
        }

        public string ElapsedTime
        {
            get
            {
                var totalElapsed = _stopwatch.Elapsed + _pausedTime;
                return $"{totalElapsed.Hours:00}:{totalElapsed.Minutes:00}:{totalElapsed.Seconds:00}";
            }
        }

        public bool IsRunning => _stopwatch.IsRunning;

        public bool IsPaused => _isPaused;

        public void Start()
        {
            if (!_stopwatch.IsRunning && !_isPaused)
            {
                _stopwatch.Start();
            }
        }

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

        public void Resume()
        {
            if (_isPaused)
            {
                _stopwatch.Start();
                _isPaused = false;
            }
        }

        public void Reset()
        {
            _stopwatch.Reset();
            _pausedTime = TimeSpan.Zero;
            _isPaused = false;
        }

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