namespace OpenMedStack.Infrastructure.Tailf.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bootstrapping.Tailf;
    using Xunit;

    public class TailMonitorTests : IDisposable
    {
        private readonly string _path;

        public TailMonitorTests()
        {
            _path = Path.GetRandomFileName();
        }

        [Fact]
        public async Task MonitorReadsAllLinesInFile()
        {
            var waitHandle = new ManualResetEventSlim(false);
            var log = new[] { "line 1", "line 2" };
            await File.WriteAllLinesAsync(_path, log).ConfigureAwait(false);
            var received = 0;
            await using var monitor = new TailMonitor(
                     new[] { _path },
                     handlers: l =>
                     {
                         if (Interlocked.Increment(ref received) == 2)
                         {
                             waitHandle.Set();
                         }
                         return Task.CompletedTask;
                     });

            var success = waitHandle.Wait(TimeSpan.FromSeconds(1));

            Assert.True(success);
        }

        [Fact]
        public async Task MonitorReadsAllLinesWhileLogsAreWritten()
        {
            var waitHandle = new ManualResetEventSlim(false);
            var log = new[] { "line 1", "line 2" };
            await File.WriteAllLinesAsync(_path, log).ConfigureAwait(false);
            var received = 0;
            await using var monitor = new TailMonitor(
                     new[] { _path },
                     handlers: l =>
                     {
                         if (Interlocked.Increment(ref received) == 4)
                         {
                             waitHandle.Set();
                         }
                         return Task.CompletedTask;
                     });
            await Task.Delay(Debugger.IsAttached ? 10000 : 250).ConfigureAwait(false);

            await File.AppendAllLinesAsync(_path, new[] { "Line 3", "Line 4" }).ConfigureAwait(false);

            var success = waitHandle.Wait(TimeSpan.FromSeconds(Debugger.IsAttached ? 100 : 1));

            Assert.True(success);
        }


        [Fact]
        public async Task MonitorReadsAllLinesWhileLogsAreWrittenInSteps()
        {
            var waitHandle = new ManualResetEventSlim(false);
            var log = new[] { "line 1", "line 2" };
            await File.WriteAllLinesAsync(_path, log).ConfigureAwait(false);
            var received = 0;
            await using var monitor = new TailMonitor(
                     new[] { _path },
                     handlers: l =>
                     {
                         if (Interlocked.Increment(ref received) == 3)
                         {
                             waitHandle.Set();
                         }
                         return Task.CompletedTask;
                     });
            await Task.Delay(Debugger.IsAttached ? 10000 : 250).ConfigureAwait(false);

            await File.AppendAllTextAsync(_path, "Lin").ConfigureAwait(false);
            await File.AppendAllTextAsync(_path, "e 3\r\n").ConfigureAwait(false);

            var success = waitHandle.Wait(TimeSpan.FromSeconds(Debugger.IsAttached ? 100 : 1));

            Assert.True(success);
            Assert.Equal(3, received);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            File.Delete(_path);
        }
    }
}