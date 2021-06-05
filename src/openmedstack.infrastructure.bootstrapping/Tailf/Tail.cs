namespace OpenMedStack.Infrastructure.Bootstrapping.Tailf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Tail
    {
        private readonly TimeSpan _pollInterval;
        private readonly CancellationTokenSource _requestForExit = new();
        private long _prevLen = -1;
        private readonly string _path;
        private readonly int _nLines;
        private readonly Func<string, Task>[] _handlers;

        public Regex? LineFilter { get; }

        public Tail(string path, int nLines, Regex? lineFilter = null, TimeSpan pollingInterval = default, params Func<string, Task>[] handlers)
        {
            _pollInterval = pollingInterval == default ? TimeSpan.FromMilliseconds(500) : pollingInterval;
            LineFilter = lineFilter;
            _path = path;
            _nLines = nLines;
            _handlers = handlers;
        }

        public void Stop()
        {
            _requestForExit.Cancel();
        }

        public async Task Run()
        {
            if (!File.Exists(_path))
            {
                throw new FileNotFoundException($"File does not exist:{_path}");
            }
            var fi = new FileInfo(_path);
            _prevLen = fi.Length;
            await MakeTail(_nLines, _path).ConfigureAwait(false);
            while (!_requestForExit.IsCancellationRequested)
            {
                await Fw_Changed().ConfigureAwait(false);
                await Task.Delay(_pollInterval).ConfigureAwait(false);
            }
        }

        private async Task Fw_Changed()
        {
            var fi = new FileInfo(_path);
            if (!fi.Exists)
            {
                return;
            }

            if (fi.Length != _prevLen)
            {
                if (fi.Length < _prevLen)
                {
                    //assume truncated!
                    _prevLen = 0;
                }

                await using var stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                stream.Seek(_prevLen, SeekOrigin.Begin);
                using var sr = new StreamReader(stream);

                string? line;
                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (LineFilter is not null && !LineFilter.IsMatch(line))
                    {
                        continue;
                    }

                    var line1 = line;
                    var tasks = _handlers.Select(h => h(line1.TrimEnd('\r', '\n')));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }

                _prevLen = stream.Position;
            }
        }

        private async Task MakeTail(int nLines, string path)
        {
            var lines = new List<string>();
            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            using (var sr = new StreamReader(stream))
            {
                string? line;
                while (null != (line = await sr.ReadLineAsync().ConfigureAwait(false)))
                {
                    if (LineFilter != null)
                    {
                        if (LineFilter.IsMatch(line!))
                        {
                            EnqueueLine(nLines, lines, line!);
                        }
                    }
                    else
                    {
                        EnqueueLine(nLines, lines, line!);
                    }
                }
            }

            var tasks = lines.SelectMany(l => _handlers.Select(h => h(l.Trim('\r', '\n'))));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static void EnqueueLine(int nLines, List<string> lines, string line)
        {
            if (lines.Count >= nLines)
            {
                lines.RemoveAt(0);
            }
            lines.Add(string.Concat(Environment.NewLine, line));
        }
    }
}
