namespace OpenMedStack.Infrastructure.Tailf.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandLine;
    using Tailf;
    using Xunit;

    public class TailfParameterParserTests
    {
        [Fact]
        public void CanGroupHeadersIntoLookup()
        {
            var result = Parser.Default.ParseArguments<TailfParameters>(
                new[]
                {
                    "-d",
                    "http://localhost",
                    "-f",
                    "a.b",
                    "-h",
                    "X-Header:Something",
                    "X-Header:Else",
                    "\'X-Arg:Foo Foo\'",
                    "\"X-Arg:Bar Bar\""
                });
            var prms = (result as Parsed<TailfParameters>).Value;
            var headers = prms.Headers.Select(
                    h =>
                    {
                        var x = h.Trim('\"', '\'').Split(':', StringSplitOptions.TrimEntries);
                        return KeyValuePair.Create(x[0], x[1]);
                    })
                .ToLookup(x => x.Key, x => x.Value);

            Assert.Equal(2, headers.Count);
            Assert.All(headers, g => Assert.Equal(2, g.Count()));
        }

        [Fact]
        public void FindsAllFilenamesFromSingleParameter()
        {
            var result = Parser.Default.ParseArguments<TailfParameters>("-d http://localhost -f file1.txt file2.txt".Split(' '));

            Assert.IsType<Parsed<TailfParameters>>(result);

            Assert.Equal(2, (result as Parsed<TailfParameters>)!.Value.FileNames!.Count());
        }

        //[Fact] // For v2.9
        //public void FindsAllFilenamesFromMultipleParameters()
        //{
        //    var result = Parser.Default.ParseArguments<TailfParameters>("-f file1.txt -f file2.txt".Split(' '));

        //    Assert.IsType<Parsed<TailfParameters>>(result);

        //    Assert.Equal(2, (result as Parsed<TailfParameters>)!.Value.FileNames!.Count());
        //}
    }
}
