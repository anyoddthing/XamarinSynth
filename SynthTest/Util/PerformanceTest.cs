using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace SynthTest
{
    // Stolen from here
    // http://stackoverflow.com/questions/969290/exact-time-measurement-for-performance-testing

    public static class StatisticsExtensions
    {
        public static double NormalizedMean(this IEnumerable<double> values)
        {
            var length = values.Count();
            if (length == 0)
                return double.NaN;

            var deviations = values.Deviations().ToArray();
            var meanDeviation = deviations.Sum(t => Math.Abs(t.Item2)) / length;
            return deviations.Where(t => t.Item2 > 0 || Math.Abs(t.Item2) <= meanDeviation).Average(t => t.Item1);
        }

        public static IEnumerable<Tuple<double, double>> Deviations(this IEnumerable<double> values)
        {
            var length = values.Count();
            if (length == 0)
                yield break;

            var avg = values.Average();
            foreach (var d in values)
                yield return Tuple.Create(d, avg - d);
        }
    }

    public class Clock
    {
        interface IStopwatch
        {
            bool IsRunning { get; }
            TimeSpan Elapsed { get; }

            void Start();
            void Stop();
            void Reset();
        }

        class TimeWatch : IStopwatch
        {
            Stopwatch stopwatch = new Stopwatch();

            public TimeSpan Elapsed
            {
                get { return stopwatch.Elapsed; }
            }

            public bool IsRunning
            {
                get { return stopwatch.IsRunning; }
            }
                
            public void Start()
            {
                stopwatch.Start();
            }

            public void Stop()
            {
                stopwatch.Stop();
            }

            public void Reset()
            {
                stopwatch.Reset();
            }
        }

        class CpuWatch : IStopwatch
        {
            TimeSpan startTime;
            TimeSpan endTime;
            bool isRunning;

            public TimeSpan Elapsed
            {
                get
                {
                    if (IsRunning)
                        throw new NotImplementedException("Getting elapsed span while watch is running is not implemented");

                    return endTime - startTime;
                }
            }

            public bool IsRunning
            {
                get { return isRunning; }
            }



            public void Start()
            {
                startTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = true;
            }

            public void Stop()
            {
                endTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = false;
            }

            public void Reset()
            {
                startTime = TimeSpan.Zero;
                endTime = TimeSpan.Zero;
            }
        }

        public static double BenchmarkTime(Action action, int iterations = 10000)
        {
            return Benchmark<TimeWatch>(action, iterations);
        }

        public static double BenchmarkCpu(Action action, int iterations = 10000)
        {
            return Benchmark<CpuWatch>(action, iterations);
        }

        static double Benchmark<T>(Action action, int iterations) where T : IStopwatch, new()
        {
            //clean Garbage
            GC.Collect();

            //wait for the finalizer queue to empty
            GC.WaitForPendingFinalizers();

            //clean Garbage
            GC.Collect();

            //warm up
            action();

            var stopwatch = new T();
            var timings = new double[5];
            for (int i = 0; i < timings.Length; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                for (int j = 0; j < iterations; j++)
                    action();
                stopwatch.Stop();
                timings[i] = stopwatch.Elapsed.TotalMilliseconds;
            }

            return StatisticsExtensions.NormalizedMean(timings);
        }
    }
}

