using System.Collections.Generic;

namespace dir2;
static class Config
{
    static public string GetFilename()
    {
        var pathHome = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(pathHome))
        {
            pathHome = Environment.GetEnvironmentVariable("UserProfile");
        }
        return Path.Join(pathHome, ".local", "dir2.opt");
    }

    static public IEnumerable<string[]> ParseFile()
    {
        try
        {
            var cfgFilename = GetFilename();
            using var fs = File.OpenText(cfgFilename);
            var lines = fs.ReadToEnd()
                .Split('\n', '\r')
                .Select((it) => it.Trim())
                .Select((it) => string
                    .Join(" ", it
                        .Split(' ')
                        .Where((it2) => it2.Length > 0))
                    .Split(new char[] { ' ' }, 2))
                .Where((it) => it.Length > 0);
            try
            {
                return MyOptions.ConfigParsers
                    .Aggregate(lines, (acc, opt) => opt.Parse2(acc))
                    .Where((it) => it.Length == 2)
                    .Join(MyOptions.ExclFileDirParsers,
                    outerKeySelector: (line) => line[0],
                    innerKeySelector: (opt) => opt.Name,
                    resultSelector: (line, opt) => line);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(
                    $"Config file {cfgFilename} [{ee.GetType()}] {ee.Message}");
                Console.Error.WriteLine();
                return Enumerable.Empty<string[]>();
            }
        }
        catch
        {
            return Enumerable.Empty<string[]>();
        }
    }

    static internal IEnumerable<string> GetEnvirOpts()
    {
        var envirOld = Environment.GetEnvironmentVariable(
            nameof(dir2));
        if (string.IsNullOrEmpty(envirOld))
        {
            return Array.Empty<string>();
        }

        return (" " + envirOld)
            .Split(" -")
            .Select((it) => it.Trim())
            .Where((it) => it.Length > 0)
            .Select((it) => "-" + it)
            .Distinct()
            ;
    }
}
