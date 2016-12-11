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

        public Task<TimeSpan> Time(Action action)
        {
            return Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            });
        }

        [Fact]
        public async Task TimeHelperTest()
        {
            await Time(() =>
                {
                    Task.Delay(TimeSpan.FromSeconds(3)).Wait(_timeout);
                })
                .ContinueWith(runtime =>
                {
                    runtime.Result.Should()
                        .BeCloseTo(TimeSpan.FromSeconds(3), _precision);
                });
        }

        [Fact]
        public void ZeroTimeSpanShouldWillExplode()
        {
            var sut = new IntervalTimer(TimeSpan.Zero, () => { });

            Assert.Throws<ArgumentException>(() => sut.Start());
        }

        [Fact]
        public async void SecondTimeSpanShouldExecuteAfterASecond()
        {
            var counter = 0;
            var sut = new IntervalTimer(TimeSpan.FromSeconds(1), () => counter++);

            await Time(() =>
                {

                    Task.Run(() =>
                        {
                            sut.Start();
                            while (counter < 1) { }
                            sut.Stop();
                        })
                        .Wait(_timeout);
                })
                .ContinueWith(runtime =>
                {
                    counter.ShouldBeEquivalentTo(1);
                    runtime.Result.Should()
                        .BeCloseTo(TimeSpan.FromSeconds(1), _precision);
                });
        }

        [Fact]
        public async void TwoExecutionsOnSecondTimeSpanShouldExecuteTwice()
        {
            var counter = 0;
            var sut = new IntervalTimer(TimeSpan.FromSeconds(1), () => counter++);

            await Time(() => Task.Run(() =>
                    {
                        sut.Start();
                        while (counter < 2) { }
                        sut.Stop();
                    }).Wait(_timeout))
                .ContinueWith(runtime =>
                {
                    counter.ShouldBeEquivalentTo(2);
                    runtime.Result.Should()
                        .BeCloseTo(TimeSpan.FromSeconds(2), _precision);
                });
        }

        [Fact]
        public async void LongRunningExecutionsOnSecondTimeSpanShouldExecuteOnce()
        {
            var counter = 0;
            var sut = new IntervalTimer(TimeSpan.FromSeconds(1), () =>
            {
                counter++;
                Task.Delay(TimeSpan.FromSeconds(3)).Wait(_timeout);
            });

            await Time(() => Task.Run(() =>
            {
                sut.Start();
                while (counter < 1) { }
                sut.Stop();
            }).Wait(_timeout))
                .ContinueWith(runtime =>
                {
                    counter.ShouldBeEquivalentTo(1);
                    runtime.Result.Should()
                        .BeCloseTo(TimeSpan.FromSeconds(2), _precision);
                });
        }
    }
}