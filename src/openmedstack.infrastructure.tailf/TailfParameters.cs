namespace OpenMedStack.Infrastructure.Tailf
{
    using System;
    using System.Collections.Generic;
    using CommandLine;

    public class TailfParameters
    {
        [Option('f', HelpText = "Files to monitor", Required = true, Min = 1)]
        public IEnumerable<string>? FileNames { get; set; }

        [Option('d', HelpText = "Url of the upload destination", Required = true)]
        public string Destination { get; set; } = null!;

        [Option('n', Default = 5, HelpText = "Lines to show at the end of the files", Required = false)]
        public int NOfLines { get; set; } = 5;

        [Option('r', HelpText = "Regular expression that match a portion of the line to accept it", Required = false)]
        public string? Filter { get; set; }

        [Option('h', HelpText = "Request header to set. Separate key from value with colon, ex key:value", Required = false)]
        public IEnumerable<string> Headers { get; set; } = Array.Empty<string>();

        [Option('p', Default = 500, HelpText = "The interval in milliseconds between checking the files for content", Required = false)]
        public int PollingInterval { get; set; }
    }
}
