using System.Collections.Immutable;
using static dir2.MyOptions;
using static dir2.Helper;
using System.Text;

namespace dir2;
public class Program
{
    static public void Main(string[] args)
    {
        try
        {
            Console.CancelKeyPress += (_, e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC
                || e.SpecialKey == ConsoleSpecialKey.ControlBreak)
                {
                    Console.ResetColor();
                }
            };
            RunMain(args);
            foreach (var ae3 in ConfigException.GetErrors())
            {
                switch (ae3.Type)
                {
                    case ArgType.ConfigFile:
                        Console.WriteLine(
                            $"Config file '{ae3.Source}': {ae3.Error.Message}");
                        break;
                    case ArgType.Environment:
                        Console.WriteLine(
                            $"Envir var '{ae3.Source}': {ae3.Error.Message}");
                        break;
                    default:
                        Console.WriteLine(
                            $"Unknown error '{ae3.Source}': {ae3.Error.Message}");
                        break;
                }
            }
        }
        catch (ShowSyntaxException se)
        {
            Console.WriteLine(se.Message);
        }
        catch (ConfigException aee)
        {
            if (GetExeEnvr().Contains(DumpExceptionOpt))
            {
                Console.WriteLine(aee);
            }
            else
            {
                Console.WriteLine(aee.Message);
            }
        }
		catch (Exception ee)
		{
            if (GetExeEnvr().Contains(DumpExceptionOpt))
            {
                Console.WriteLine(ee);
            }
            else if (ee.InnerException is ConfigException ae)
            {
                Console.WriteLine($"{ee.Message} {ae.Message}");
            }
            else
            {
                Console.WriteLine($"{ee.GetType()}: {ee.Message}");
            }
        }
    }

    internal const string CfgOffOpt = "--cfg-off";
    internal const string DumpExceptionOpt = "--dump-exception-stack";
    internal const string DumpArgsIfNothingOpt = "--dump-args-if-nothing";
    internal const string DebugOpt = "--debug";
    static internal readonly string[] EnvrSkipOpts = new string[]
    {
        CfgOffOpt
        , DumpExceptionOpt
        , DumpArgsIfNothingOpt
        , DebugOpt
    };

    static public Action<string> MyDebugWrite { get; set; }
        = (msg) => System.Diagnostics.Debug.WriteLine(msg);

    static bool RunMain(string[] mainArgs)
	{
        if (GetExeEnvr().Contains("--debug:on"))
        {
            MyDebugWrite = (msg) => Console.Error.WriteLine(msg);
        }

		if (mainArgs.Contains("--version") ||
            mainArgs.Contains("-v"))
		{
			WriteLine(GetVersion());
            WriteLine($"Native: {NativeLib.LibraryName()}");
			return false;
		}

        if (mainArgs.Contains("--HELP") || mainArgs.Contains("-??"))
        {
            ((IParse) Show.PauseOpt).Parse(new List<(bool, ArgType, string)>
            {(true, ArgType.CommandLine, "on")});

            ((IParse)Show.PauseOpt).Parse(
                mainArgs.Select((it) => (false, ArgType.CommandLine, it)));

            foreach (var line in GetSyntax().Split(Environment.NewLine))
            {
                Console.WriteLine(line);
                Show.PauseOpt.Invoke(false);
            }
            return false;
        }

        var ndxHelp = Array.FindIndex(mainArgs,
            (it) => it == "-?" || it == "-h" || it == "--help");
        if (-1 < ndxHelp)
        {
            ((IParse)Show.PauseOpt).Parse(new List<(bool, ArgType, string)>
            {(true, ArgType.CommandLine, "on")});

            ((IParse)Show.PauseOpt).Parse(
                mainArgs.Select((it) => (false, ArgType.CommandLine, it)));

            var helpThe = new List<string>();
            if (ndxHelp+1 < mainArgs.Length)
            {
                switch(mainArgs[ndxHelp+1])
                {
                    case "cfg":
                        helpThe = Config.GetHelp().Split(Environment.NewLine).ToList();
                        break;

                    case "config":
                        helpThe = Config.GetHelp().Split(Environment.NewLine).ToList();
                        break;

                    case "-":
                        helpThe.Add("Shortcut:");
                        foreach (var optThe in GetOptionHelps()
                            .Where((it) => false == string.IsNullOrEmpty(it.Shortcut.Trim())))
                        {
                            helpThe.Add($" {optThe.Name,16} {optThe.Shortcut}  {optThe.Help}");
                        }

                        foreach (var kvThe in MyOptions.ShortcutComplexOptions
                            .OrderBy((it) => it.Value.Item1)
                            .OrderBy((it) => it.Key))
                        {
                            var a2 = new StringBuilder();
                            // Append(".1234567890123456");
                            a2.Append("                 ");
                            a2.Append($" {kvThe.Key}");
                            var text2The = string.Join(" ", kvThe.Value.Item2);
                            a2.Append($"  => {text2The,-12}");
                            helpThe.Add(a2.ToString());
                        }
                        foreach (var kvThe in MyOptions.ShortcutExpandOptions
                            .OrderBy((it) => it.Value.Item1))
                        {
                            helpThe.Append($" {kvThe.Value.Item1,-16} {kvThe.Key}");
                        }

                        break;

                    case "+":
                        helpThe.Add("Quick help +? can be the following options.");
                        foreach (var optThe in GetOptionHelps()
                            .Where((it) => false == string.IsNullOrEmpty(it.Help)))
                        {
                            helpThe.Add($" {optThe.Name,16} {optThe.Shortcut}  {optThe.Help}");
                        }
                        helpThe.Add("For example:");
                        helpThe.Add("    dir2  --link  +?");
                        break;

                    default:
                        helpThe = ShortSyntax.Split(Environment.NewLine).ToList();
                        break;
                }
            }
            else
            {
                helpThe = ShortSyntax.Split(Environment.NewLine).ToList();
            }
            foreach (var line in helpThe)
            {
                Console.WriteLine(line);
                Show.PauseOpt.Invoke(false);
            }
            return false;
        }

        (bool envrCfgOff, IEnumerable<(ArgType, string)> cfgRest) =
            Config.GetEnvirOpts();

        if (false == envrCfgOff &&
            false == mainArgs.Contains(CfgOffOpt))
        {
            cfgRest = cfgRest.Concat( Config.ParseConfigFile());
        }

        try
        {
            cfgRest = Parsers.Resolve(cfgRest,
                isIncludeExclNameOptions: false);
        }
        catch (ConfigException ae2)
        {
            cfgRest = Enumerable.Empty<(ArgType, string)>();
            ConfigException.Add(ArgType.Environment, nameof(dir2), ae2);
        }

        var tmp2 = cfgRest
            .Concat(ExpandFromShortCut(ExpandForHomeDir(mainArgs))
            .Select((it) => (ArgType.CommandLine, it)));
        var tmp3 = Parsers.Resolve(
            tmp2.Where((it) => false == EnvrSkipOpts.Contains(it.Item2)),
            isIncludeExclNameOptions: true)
            .GroupBy((it) => it.Item1 == ArgType.CommandLine)
            .ToImmutableDictionary(
            (grp) => grp.Key,
            (grp) => grp.AsEnumerable());

        string[] args = Array.Empty<string>();

        if (tmp3.TryGetValue(true, out var matches))
        {
            args = Wild.Parse_ExclNone(matches
                .Select((it) => it.Item2));
        }

        if (tmp3.TryGetValue(false, out var notMatches))
        {
            foreach (var tmp4 in notMatches
                .GroupBy((it) => it.Item1))
            {
                var tmp5 = string.Join(" ",
                    tmp4.Select((it) => it.Item2).ToArray());
                if (!tmp5.StartsWith("--debug:"))
                {
                    Console.Error.WriteLine(
                        $"Unknown {tmp4.Key} options: {tmp5}");
                }
            }
        }

        Show.EncodeConsoleOutput();

        var dd = args
            .Select((it) => new
            {
                hasColon = (it.Length > 1 && it[1] == ':'),
                path = EnsureDirSep(it),
            })
            .GroupBy((it) => it.hasColon)
            .ToImmutableDictionary((grp) => grp.Key, (grp) => grp.ToArray());

        if (dd.TryGetValue(true, out var hasColons) && hasColons.Length == 1)
        {
            (var path2, var wild2) = ScanTheDir(hasColons.First().path);
            if (dd.TryGetValue(false, out var noColons))
            {
                var wild3 = noColons.Select((it) => it.path).ToList();
                if (false == string.IsNullOrEmpty(wild2))
                {
                    wild3.Add(wild2);
                }
                return ScanSubDir(path: path2,
                    wilds: wild3.ToArray(),
                    mark: "mark-11");
            }
            else
            {
                return ScanSubDir(path: path2,
                    wilds: new[] { wild2 },
                    mark: "mark-20");
            }
        }

        var aa = args
            .Select((it) => EnsureDirSep(it))
            .GroupBy((it) => it.Contains(Path.DirectorySeparatorChar))
            .ToImmutableDictionary((grp) => grp.Key,
            (grp) => grp.ToList());

        if (aa.TryGetValue(true, out var strings))
        {
            var bb = strings
                .Select((it) => new
                {
                    DirName = Path.GetDirectoryName(it) ?? string.Empty,
                    FileName = Path.GetFileName(it) ?? string.Empty,
                })
                .GroupBy((it) => it.DirName)
                .ToImmutableDictionary((grp) => grp.Key,
                (grp) => grp.Select((it) => it.FileName).ToList());
            var b9 = bb.Keys.First();
            if (string.IsNullOrEmpty(b9))
                b9 = Path.DirectorySeparatorChar.ToString();
            (var path4, var _) = ScanTheDir(Path.Combine(b9, "_ignore_"));
            var bb2 = bb.Values.First()
                .Where((it) => false == string.IsNullOrEmpty(it))
                .ToList();

            return (bb.Count(), aa.TryGetValue(false, out var notFound)) switch
            {
                (1, false) => ScanSubDir(path: path4,
                wilds: bb.Values.First()
                .Where((it) => false == string.IsNullOrEmpty(it))
                .ToArray(),
                mark: "mark-30"),

                (1, true) => ScanSubDir(path: path4,
                wilds: bb.Values.First().Concat(notFound)
                .Where((it) => false == string.IsNullOrEmpty(it))
                .ToArray(),
                mark: "mark-40"),

                _ => ListInfo(args, mark: "mark-50")
            };
        }

        var cc = args
            .GroupBy((it) => Directory.Exists(it))
            .ToImmutableDictionary((grp) => grp.Key,
            (grp) => grp.ToList());
        if (cc.TryGetValue(true, out var dirnames))
        {
            if (dirnames.Count > 1)
            {
                return ListInfo(aa[false].ToArray(), mark: "mark-100");
            }
            var dirThe = dirnames.First();
            if (cc.TryGetValue(false, out var wilds))
            {
                return ScanSubDir(dirThe, wilds.ToArray(),
                mark: "mark-110");
            }
            return ScanSubDir(dirThe, Array.Empty<string>(),
                mark: "mark-120");
        }

        if (args.Length== 0)
        {
            return ScanSubDir(path: ".", wilds: Array.Empty<string>(),
                mark: "mark-130");
        }

        if (args[0].Length > 1 && args[0][1] == ':')
        {
            var aa2 = args.Skip(1).ToArray();
            if (args[0].Length > 2)
            {
                aa2 = new string[] { args[0].Substring(2) }
                .Concat(aa2)
                .ToArray();
            }
            var pathThe = args[0][0].ToString();
            pathThe += ":.";
            pathThe += Path.DirectorySeparatorChar;
            return ScanSubDir(pathThe,
                wilds: aa2, mark: "mark-140");
        }

        return ScanSubDir(path: ".", wilds: args,
            mark: "mark-900");
	}

    static bool ListInfo(string[] args, string mark)
    {
        MyDebugWrite($"** {nameof(ListInfo)}>Tip='{mark}'>'{string.Join("|",args)}'");
        foreach (var arg in args)
        {
            if (File.Exists(arg))
            {
                var infoFile = new FileInfo(arg);
                Write(Show.Date(DateFormatOpt.Invoke(infoFile.LastWriteTime)));
                Write(Show.Size(Show.LengthFormatOpt.Invoke(infoFile.Length)));
                WriteLine(arg);
            }
            else if (Directory.Exists(arg))
            {
                var infoDir = new DirectoryInfo(arg);
                Write(Show.Date(DateFormatOpt.Invoke(infoDir.LastWriteTime)));
                Write("[DIR] ");
                WriteLine(arg);
            }
            else
            {
                WriteLine($"'{arg}' is NOT found, or, wild-card.");
            }
        }
        return true;
    }

    static bool ScanSubDir(string path, string[] wilds, string mark)
    {
        if (wilds.Length==0)
        {
            MyDebugWrite($"** {nameof(ScanSubDir)}>Tip='{mark}'>path={path}");
        }
        else
        {
            MyDebugWrite(
                $"** {nameof(ScanSubDir)}>Tip='{mark}'>path={path}>#wild={wilds.Length},'{wilds[0]}'");
        }

        if ((true!=Directory.Exists(path)) && (wilds.Length==0))
        {
            wilds = new string[1] { Path.GetFileName(path) ?? "*"};
            path = Path.GetDirectoryName(path);
        }

        Wild.InitMatchingNames(wilds);

        if (path.EndsWith(':'))
        {
            path += ".";
        }

        if (false == path.EndsWith(Path.DirectorySeparatorChar))
        {
            path += Path.DirectorySeparatorChar;
        }

        path = Io.GetFullPath(path);
        InfoSum sumThe = SubDirOpt.Invoke(path);
        PrintInfoTotal(path, wilds, sumThe);
        Show.Color.Reset();
        return true;
    }

    static IEnumerable<string> ExpandForHomeDir(string[] args)
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(homeDir))
        {
            homeDir = Environment.GetEnvironmentVariable("UserProfile");
        }
        var it = args.AsEnumerable().GetEnumerator();
        while (it.MoveNext())
        {
            var current = it.Current;

            if (1 == current.Length && '~' == current[0])
            {
                yield return homeDir + Path.DirectorySeparatorChar;
            }
            else if (1 < current.Length && '~' == current[0] &&
                Path.DirectorySeparatorChar == current[1])
            {
                if (current.Length == 2)
                {
                    yield return homeDir + Path.DirectorySeparatorChar;
                }
                else
                {
                    yield return Path.Combine(homeDir, current.Substring(2));
                }
            }
            else
            {
                yield return current;
            }
        }
    }

    static string EnsureDirSep(string arg)
    {
        if (Directory.Exists(arg) &&
            !arg.EndsWith(Path.DirectorySeparatorChar))
        {
            if (arg.EndsWith(":"))
            {
                arg += ".";
            }
            return arg + Path.DirectorySeparatorChar;
        }
        return arg;
    }

    static (string path, string wild) ScanTheDir(string dirname)
    {
        var ee = dirname.Split(Path.DirectorySeparatorChar).ToList();
        var e2 = string.Empty;
        if (ee.Count > 1)
        {
            var ndxLast = ee.Count - 1;
            if (true!=string.IsNullOrEmpty(ee[ndxLast]))
            {
                e2 = ee[ndxLast];
            }
            ee.RemoveAt(ndxLast);
        }

        // TODO: what if ee[0] is "" or "c:", "documents", .. ..
        var d3 = ee[0];
        if (string.IsNullOrEmpty(d3))
        {
            d3 = Path.DirectorySeparatorChar.ToString();
        }
        else
        {
            if (false == Directory.Exists(d3))
            {
                var ff = Directory.GetDirectories(".", searchPattern: d3);
                switch (ff.Length)
                {
                    case 0:
                        throw new ConfigException(
                                $"No dir matching  '{d3}'");
                    case 1:
                        d3 = ff[0];
                        break;
                    default:
                        throw new ConfigException(
                            $"Too many dir matching '{d3}'");
                }
            }
            d3 += Path.DirectorySeparatorChar;
        }

        for (int ndx = 1; ndx < ee.Count; ndx++)
        {
            if (ee[ndx] == ".") continue;
            if (ee[ndx] == "..")
            {
                d3 = Path.Combine(d3, ee[ndx]);
                continue;
            }
            var ff = Directory.GetDirectories(d3, searchPattern: ee[ndx]);
            switch (ff.Length)
            {
                case 0:
                    if ((ndx+1) == ee.Count)
                    {
                        return (path: d3, wild: ee[ndx]);
                    }
                    else
                    {
                        throw new ConfigException(
                            $"No dir matching '{ee[ndx]}' on '{d3}'");
                    }
                case 1:
                    d3 = ff[0];
                    break;
                default:
                    throw new ConfigException(
                        $"Too many dir matching '{ee[ndx]}' on '{d3}'");
            }
        }
        return (path: d3, wild: e2);
    }
}
