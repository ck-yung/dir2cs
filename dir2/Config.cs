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
    static public string GetFilename(string fileExt)
    {
        var pathHome = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(pathHome))
        {
            pathHome = Environment.GetEnvironmentVariable("UserProfile");
        }
        return Path.Join(pathHome, ".local", "dir2" + fileExt);
    }

    static IEnumerable<(ArgType, string)> SelectArgsFromLines( ArgType type,
        IEnumerable<string> args)
    {
        foreach (var arg in args
            .Select((it) => it.Trim())
            .Where((it) => it.StartsWith("--")))
        {
            var bb = arg.Split([' ', '\t'], 2);
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

    static readonly string FirstCfgExt = "-cfg.txt";
    static public readonly string ShortDateCfg = "-date-short.txt";
    static public IEnumerable<(ArgType, string)> ParseConfigFile()
    {
        var cfgFilename = "[undefined]";
        try
        {
            cfgFilename = new string[] { FirstCfgExt, ".opt" }
            .Select((it) => GetFilename(it))
            .FirstOrDefault((it) => File.Exists(it))
            ?? ":cfg-not-found";
            if (false == File.Exists(cfgFilename))
                return Enumerable.Empty<(ArgType, string)>();
            var buf2 = new byte[4096];
            int readCnt = 0;
            using (var inpFp = File.OpenRead(cfgFilename))
            {
                readCnt = inpFp.Read(buf2);
            }
            var lines = SummaryInfo.Init(Encoding.UTF8.GetString(buf2, 0, readCnt)
                .Split('\n', '\r'));
            var args = SelectArgsFromLines(ArgType.ConfigFile, lines)
                .Select((it) => (false, it.Item1, it.Item2));
            var tmp = MyOptions.ConfigParsers
                .Aggregate(args, (acc, opt) => opt.Parse(acc))
                .Select((it) => (it.Item2, it.Item3));
            return Wild.SelectExclFeatures(
                MyOptions.ExclFileDirParsers,
                tmp);
        }
        catch (ConfigException ae)
        {
            ConfigException.Add(ArgType.ConfigFile, cfgFilename, ae);
            return Enumerable.Empty<(ArgType, string)>();
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
            return (false, Enumerable.Empty<(ArgType, string)>());
        }

        try
        {
            var aa = envirOld.Split("--")
                .Select((it) => it.Trim())
                .Select((it) => it.Trim(';'))
                .Where((it) => it.Length > 0)
                .Select((it) => "--" + it)
                .GroupBy((it) => it.Equals(Program.CfgOffOpt))
                .ToImmutableDictionary((grp) => grp.Key,
                (grp) => grp.AsEnumerable());
            var isCfgOff = aa.ContainsKey(true);
            if (aa.TryGetValue(false, out var notFounds))
            {
                return (isCfgOff, SelectArgsFromLines(
                    ArgType.Environment, notFounds));
            }
            return (isCfgOff, Enumerable.Empty<(ArgType, string)>());
        }
        catch (Exception ee)
        {
            ConfigException.Add(ArgType.Environment, nameof(dir2), ee);
            return (false, Enumerable.Empty<(ArgType, string)>());
        }
    }

    static public string GetHelp()
    {
        var rtn = new StringBuilder(
            $"Load following options from '{GetFilename(FirstCfgExt)}'");
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
