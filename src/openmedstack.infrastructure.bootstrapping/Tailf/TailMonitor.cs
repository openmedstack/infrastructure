namespace OpenMedStack.Infrastructure.Bootstrapping.Tailf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class TailMonitor : IAsyncDisposable
    {
        private readonly List<(Tail, ConfiguredTaskAwaitable)> _tails;

        public TailMonitor(
            IEnumerable<string> fileNames,
            Regex? filter = null,
            int numberOfLines = 5,
            TimeSpan pollingInterval = default,
            params Func<string, Task>[] handlers)
        {
            _tails = (from fileName in fileNames
                      let tail = new Tail(fileName, numberOfLines, filter, pollingInterval, handlers)
                      let task = tail.Run().ConfigureAwait(false)
                      select (tail, task)).ToList();
        }

        public async ValueTask Stop()
        {
            foreach (var (tail, configuredTaskAwaitable) in _tails)
            {
                tail.Stop();
                await configuredTaskAwaitable;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await Stop().ConfigureAwait(false);
            _tails.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
