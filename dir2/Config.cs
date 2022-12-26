using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks.Dataflow;

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
                var aa = MyOptions.ConfigParsers
                    .Aggregate(lines, (acc, opt) => opt.Parse2(acc))
                    .ToArray();
                System.Diagnostics.Debug.WriteLine($"#aa={aa.Length}");
                return aa
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

    static internal (bool, IEnumerable<string>) GetEnvirOpts()
    {
        var envirOld = Environment.GetEnvironmentVariable(nameof(dir2));
        if (string.IsNullOrEmpty(envirOld))
        {
            return (false, Enumerable.Empty<string>());
        }

        try
        {
            var aa = envirOld.Split(" ")
                .Where((it) => it.Length > 0)
                .GroupBy((it) => it.Equals(Program.CfgOffOpt))
                .ToImmutableDictionary((grp) => grp.Key,
                (grp) => grp.AsEnumerable());
            var isCfgOff = aa.ContainsKey(true);
            if (aa.ContainsKey(false)) return (isCfgOff, aa[false]);
            return (isCfgOff, Enumerable.Empty<string>());
        }
        catch (Exception ee)
        {
            Console.Error.WriteLine(
                $"Enivr {nameof(dir2)}: [{ee.GetType()}] {ee.Message}");
            Console.Error.WriteLine();
            return (false, Enumerable.Empty<string>());
        }
    }

    static public string GetHelp()
    {
        var rtn = new StringBuilder(
            $"Load following options from '{GetFilename()}'");
        rtn.AppendLine();

        foreach (var optThe in MyOptions.ConfigParsers)
        {
            rtn.AppendLine($" {optThe.Name,16}  {optThe.Help}");
        }

        foreach (var optThe in MyOptions.ExclFileDirParsers)
        {
            rtn.AppendLine($" {optThe.Name,16}  {optThe.Help}");
        }

        return rtn.ToString();
    }

}
