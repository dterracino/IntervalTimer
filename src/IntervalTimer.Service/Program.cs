using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Topshelf;

namespace IntervalTimer.Service
{
    class Program
    {

        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {

            var interval = TimeSpan.FromSeconds(1);
            var job = new SillyJob();

            _logger.Info($"Running {nameof(job)} every {interval}");

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

    class SillyJob
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SillyJob));
        public void Callback()
        {
            _logger?.Info($"Callback at {DateTime.Now.ToLongTimeString()}");
            if (DateTime.Now.Second%5 == 0)
            {
                Thread.Sleep(6);
            }
        }
    }
}
