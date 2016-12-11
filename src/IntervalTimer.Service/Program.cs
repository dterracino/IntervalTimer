using System;
using System.Threading.Tasks;
using log4net;
using Topshelf;

namespace IntervalTimer.Service
{
    internal class Program
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {

            var interval = TimeSpan.FromSeconds(1);
            var job = new SillyJob();

            Logger.Info($"Running {nameof(job)} every {interval}");

            var host = HostFactory.New(c =>
            {
                c.Service<IntervalTimer>(s =>
                {
                    s.ConstructUsing(x => new IntervalTimer(interval, job.Callback));
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                c.RunAsLocalSystem();

                c.SetDescription("Runs Callback on Intervals.");
                c.SetDisplayName("CallbackService");
                c.SetServiceName("CallbackService");
            });

            host.Run();
        }


    }

    internal class SillyJob
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SillyJob));
        public void Callback()
        {
            var now = DateTime.Now;

            _logger?.Info($"Callback at {now.ToLongTimeString()}");

            if (now.Second%5 == 0)
            {
                Task.Delay(TimeSpan.FromSeconds(6)).Wait();
            }
        }
    }
}
