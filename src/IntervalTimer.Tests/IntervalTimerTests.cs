using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace IntervalTimer.Tests
{
    public class IntervalTimerTests
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly int _precision = 30;

        public TimeSpan Time(Action action)
        {
            var result = TimeSpan.Zero;
            Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            })
            .ContinueWith(r => result = r.Result)
                .Wait();

            return result;
        }

        public void Wait(TimeSpan duration)
        {
            Task.Delay(duration)
                    .Wait(_timeout);
        }

        public void Run(Action action)
        {
            Task.Run(action)
                    .Wait(_timeout);
        }

        [Fact]
        public void TimeHelperTest()
        {
            var runtime = Time(() => Wait(TimeSpan.FromSeconds(2)));

            runtime.Should()
                .BeCloseTo(TimeSpan.FromSeconds(2), _precision);
        }

        [Fact]
        public void ZeroTimeSpanShouldWillExplode()
        {
            var sut = new IntervalTimer(TimeSpan.Zero, () => { });

            Assert.Throws<ArgumentException>(() => sut.Start());
        }

        [Fact]
        public void SecondTimeSpanShouldExecuteAfterASecond()
        {
            var counter = 0;
            var interval = TimeSpan.FromSeconds(1);

            var sut = new IntervalTimer(interval, () => counter++);

            var runtime = Time(() =>
            {
                Run(() =>
                {
                    sut.Start();
                    while (counter < 1) { }
                    sut.Stop();
                });
            });


            counter.ShouldBeEquivalentTo(1);
            runtime.Should()
                .BeCloseTo(TimeSpan.FromSeconds(1), _precision);
        }

        [Fact]
        public void TwoExecutionsOnSecondTimeSpanShouldExecuteTwice()
        {
            var counter = 0;
            var interval = TimeSpan.FromSeconds(1);
            var sut = new IntervalTimer(interval, () => counter++);

            var runtime = Time(() =>
            {
                Run(() =>
                {
                    sut.Start();
                    while (counter < 2) { }
                    sut.Stop();
                });
            });

            counter.ShouldBeEquivalentTo(2);
            runtime.Should()
                .BeCloseTo(TimeSpan.FromSeconds(2), _precision);
        }

        [Fact]
        public void LongRunningExecutionsOnSecondTimeSpanShouldExecuteOnce()
        {
            var interval = TimeSpan.FromSeconds(1);
            var counter = 0;
            Action callback = () =>
            {
                Wait(TimeSpan.FromSeconds(2));
                counter++;
            };

            var sut = new IntervalTimer(interval, callback);

            var runtime = Time(() =>
            {
                Run(() =>
                {
                    sut.Start();
                    while (counter < 1) { }
                    sut.Stop();
                });
            });

            counter.ShouldBeEquivalentTo(1);
            runtime.Should()
                .BeCloseTo(TimeSpan.FromSeconds(3), _precision);
        }
    }
}