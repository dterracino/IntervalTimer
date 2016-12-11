using System;
using System.Timers;
using log4net;

namespace IntervalTimer
{
    public class IntervalTimer
    {
        private Timer _stateTimer;
        private readonly TimeSpan _interval;
        private readonly Action _callback;

        public ILog Logger = LogManager.GetLogger(typeof(IntervalTimer));

        public IntervalTimer(TimeSpan interval, Action callback)
        {
            _interval = interval;
            _callback = callback;
        }

        public void Start()
        {
            Logger?.Debug("Starting");
            _stateTimer = new Timer(_interval.TotalMilliseconds)
            {
                AutoReset = false
            };
            _stateTimer.Elapsed += Elapsed;
            _stateTimer.Start();
            Logger?.Debug("Started");

        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger?.Debug($"Elapsed: {e.SignalTime.ToLongTimeString()}");
            _callback();
            _stateTimer.Start();
        }

        public void Stop()
        {
            Logger?.Debug("Stopping");
            _stateTimer.Stop();
            _stateTimer.Dispose();
            Logger?.Debug("Stopped");
        }

    }
}
