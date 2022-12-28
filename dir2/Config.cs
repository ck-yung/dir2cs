using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

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

    static IEnumerable<string> SelectArgsFromLines(IEnumerable<string> args)
    {
        var patternNameValue = new Regex(
            @"^\s*--(?<nameThe>\w.*)\s+(?<valueThe>\w.*)",
            RegexOptions.Compiled);
        foreach (var arg in args.Where((it) => it.Length > 0))
        {
            bool notFound = true;
            foreach (Match match in patternNameValue.Matches(arg))
            {
                var nameThe = match.Groups["nameThe"].Value;
                var valueThe = match.Groups["valueThe"].Value.TrimEnd();
                if (false == string.IsNullOrEmpty(valueThe))
                {
                    notFound = false;
                    yield return "--" + nameThe;
                    yield return valueThe;
                }
            }
            if (notFound)
            {
                if (arg.StartsWith("--"))
                {
                    yield return arg;
                }
            }
        }
    }

    static public IEnumerable<string> ParseFile()
    {
        try
        {
            var cfgFilename = GetFilename();
            using var fs = File.OpenText(cfgFilename);
            var lines = fs.ReadToEnd()
                .Split('\n', '\r')
                .Select((it) => it.Trim());
            var args = SelectArgsFromLines(lines)
                .Select((it) => (false, it));
            try
            {
                var tmp = MyOptions.ConfigParsers
                    .Aggregate(args, (acc, opt) => opt.Parse(acc))
                    .Select((it) => it.Item2);
                return Wild.SelectExclFeatures(
                    MyOptions.ExclFileDirParsers,
                    tmp);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(
                    $"Config file {cfgFilename} [{ee.GetType()}] {ee.Message}");
                Console.Error.WriteLine();
                return Enumerable.Empty<string>();
            }
        }
        catch
        {
            return Enumerable.Empty<string>();
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
