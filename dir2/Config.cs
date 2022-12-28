using System.Collections.Immutable;
using System.Text;

namespace dir2;

public enum ArgType
{
    CommandLine,
    ConfigFile,
    Environment,
};

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

    static IEnumerable<(ArgType, string)> SelectArgsFromLines( ArgType type,
        IEnumerable<string> args)
    {
        foreach (var arg in args
            .Select((it) => it.Trim())
            .Where((it) => it.StartsWith("--")))
        {
            var bb = arg.Split(new char[] { ' ', '\t' }, 2);
            if (bb.Length == 2)
            {
                yield return (type, $"{bb[0].Trim()}");
                var b2 = bb[1].Trim();
                if (false == string.IsNullOrEmpty(b2))
                {
                    yield return (type, $"{b2}");
                }
            }
            else if (bb.Length == 1)
            {
                yield return (type, $"{bb[0].Trim()}");
            }
        }
    }

    static public IEnumerable<(ArgType, string)> ParseConfigFile()
    {
        try
        {
            var cfgFilename = GetFilename();
            using var fs = File.OpenText(cfgFilename);
            var lines = fs.ReadToEnd()
                .Split('\n', '\r');
            var args = SelectArgsFromLines(ArgType.ConfigFile, lines)
                .Select((it) => (false, it.Item1, it.Item2));
            try
            {
                var tmp = MyOptions.ConfigParsers
                    .Aggregate(args, (acc, opt) => opt.Parse(acc))
                    .Select((it) => (it.Item2, it.Item3));
                return Wild.SelectExclFeatures(
                    MyOptions.ExclFileDirParsers,
                    tmp);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(
                    $"Config file {cfgFilename} [{ee.GetType()}] {ee.Message}");
                Console.Error.WriteLine();
                return Enumerable.Empty<(ArgType, string)>();
            }
        }
        catch
        {
            return Enumerable.Empty<(ArgType, string)>();
        }
    }

    static internal (bool, IEnumerable<(ArgType, string)>) GetEnvirOpts()
    {
        var envirOld = Environment.GetEnvironmentVariable(nameof(dir2));
        if (string.IsNullOrEmpty(envirOld))
        {
            // >>> debug
            const string envrFilename = "obj/envr.txt";
            if (File.Exists(envrFilename))
            {
                return (false, SelectArgsFromLines( ArgType.Environment,
                    File.ReadAllLines(envrFilename)));
            }
            // <<< debug
            return (false, Enumerable.Empty<(ArgType, string)>());
        }

        try
        {
            var aa = envirOld.Split("--")
                .Select((it) => it.Trim())
                .Where((it) => it.Length > 0)
                .Select((it) => "--" + it)
                .GroupBy((it) => it.Equals(Program.CfgOffOpt))
                .ToImmutableDictionary((grp) => grp.Key,
                (grp) => grp.AsEnumerable());
            var isCfgOff = aa.ContainsKey(true);
            if (aa.ContainsKey(false))
            {
                return (isCfgOff, SelectArgsFromLines(
                    ArgType.Environment, aa[false]));
            }
            return (isCfgOff, Enumerable.Empty<(ArgType, string)>());
        }
        catch (Exception ee)
        {
            Console.Error.WriteLine(
                $"Enivr {nameof(dir2)}: [{ee.GetType()}] {ee.Message}");
            Console.Error.WriteLine();
            return (false, Enumerable.Empty<(ArgType, string)>());
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
