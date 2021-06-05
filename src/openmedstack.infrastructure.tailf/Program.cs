using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("openmedstack.infrastructure.tailf.tests")]

namespace OpenMedStack.Infrastructure.Tailf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Bootstrapping.Tailf;
    using CommandLine;
    using CommandLine.Text;

    internal static class Program
    {
        private static readonly ManualResetEventSlim WaitHandle = new(false);

        private static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<TailfParameters>(args);
            if (result is NotParsed<TailfParameters>)
            {
                Console.WriteLine(
                    HelpText.RenderParsingErrorsTextAsLines(
                        result,
                        error => error.Tag.ToString(),
                        errors => string.Join(", ", errors.Select(e => e.SetName)),
                        0));
                Console.WriteLine(HelpText.RenderUsageText(result));
                Environment.Exit(1);
                return;
            }

            var parameters = (result as Parsed<TailfParameters>)!.Value;

            if (parameters.FileNames is null || !parameters.FileNames.Any())
            {
                return;
            }

            var headers = parameters.Headers.Select(
                    h =>
                    {
                        var x = h.Trim('\"', '\'').Split(':', StringSplitOptions.TrimEntries);
                        return KeyValuePair.Create(x[0], x[1]);
                    })
                .ToLookup(x => x.Key, x => x.Value);
            using var client = new HttpClient();
            async Task UploadLine(string line)
            {
                var msg = new HttpRequestMessage(HttpMethod.Post, parameters.Destination)
                {
                    Content = new StringContent(line, Encoding.UTF8, "text/plain")
                };
                foreach (var header in headers)
                {
                    msg.Headers.Add(header.Key, header);
                }

                var response = await client.SendAsync(msg).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine(s);
                }
            }

            try
            {
                var monitor = new TailMonitor(
                    parameters.FileNames,
                    string.IsNullOrWhiteSpace(parameters.Filter) ? null : new Regex(parameters.Filter, RegexOptions.Compiled),
                    parameters.NOfLines,
                    TimeSpan.FromMilliseconds(parameters.PollingInterval),
                   UploadLine);

                Console.CancelKeyPress += Console_CancelKeyPress;
                WaitHandle.Wait();
                Console.CancelKeyPress -= Console_CancelKeyPress;

                await monitor.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                await Console.Error.WriteLineAsync(HelpText.RenderUsageText(result)).ConfigureAwait(false);
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            WaitHandle.Set();
        }
    }
}
