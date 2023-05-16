using System.Collections.Immutable;
using static dir2.MyOptions;
using static dir2.Helper;
using System.Linq;
using dir2;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace dir2;
public class Program
{
    static public void Main(string[] args)
    {
        try
        {
            RunMain(args);
        }
        catch (ArgumentException aee)
        {
            Console.WriteLine($"Invalid args: {aee.Message}");
        }
        catch (Exception ee)
        {
            if (GetExeEnvr().Contains(DumpExceptionOpt))
            {
                Console.WriteLine(ee);
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

    static Action<string> MyDebugWrite { get; set; }
        = (msg) => Debug.WriteLine(msg);

    static bool RunMain(string[] mainArgs)
	{
        if (GetExeEnvr().Contains("--debug:on"))
        {
            MyDebugWrite = (msg) => Console.WriteLine(msg);
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
            Write(GetSyntax());
            return false;
        }

        if (mainArgs.Contains("--help") ||
            mainArgs.Contains("-h") ||
            mainArgs.Contains("-?"))
        {
            if (mainArgs.Contains("cfg") ||
                mainArgs.Contains("config"))
            {
                Write(Config.GetHelp());
            }
            else
            {
                Write(ShortSyntax);
            }
            return false;
        }

        (bool envrCfgOff, IEnumerable<TypedArg> cfgRest) =
            Config.GetEnvirOpts();

        if (false == envrCfgOff &&
            false == mainArgs.Contains(CfgOffOpt))
        {
            cfgRest = cfgRest.Concat( Config.ParseConfigFile());
        }

        cfgRest = Parsers.Resolve(cfgRest,
            isIncludeExclNameOptions: false);

        var tmp2 = cfgRest
            .Concat(ExpandFromShortCut(ExpandForHomeDir(mainArgs))
            .Select((it) => new TypedArg(ArgType.CommandLine, it)));
        var tmp3 = Parsers.Resolve(
            tmp2.Where((it) => false == EnvrSkipOpts.Contains(it.arg)),
            isIncludeExclNameOptions: true)
            .GroupBy((it) => it.type == ArgType.CommandLine)
            .ToImmutableDictionary(
            (grp) => grp.Key,
            (grp) => grp.AsEnumerable());

        string[] args = Array.Empty<string>();
        if (tmp3.ContainsKey(true))
        {
            args = Wild.Parse_ExclNone(tmp3[true]
                .Select((it) => it.arg));
        }

        if (tmp3.ContainsKey(false))
        {
            foreach (var tmp4 in tmp3[false]
                .GroupBy((it) => it.type))
            {
                var tmp5 = string.Join(" ",
                    tmp4.Select((it) => it.arg).ToArray());
                Console.Error.WriteLine(
                    $"Unknown {tmp4.Key} options: {tmp5}");
            }
        }

        Show.EncodeConsoleOutput();

        string EnsureDirSep(string arg)
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

        var dd = args
            .Select((it) => new
            {
                hasColon = (it.Length > 1 && it[1] == ':'),
                path = EnsureDirSep(it),
            })
            .GroupBy((it) => it.hasColon)
            .ToImmutableDictionary((grp) => grp.Key, (grp) => grp.ToArray());
        if (dd.ContainsKey(true) && dd[true].Length == 1)
        {
            if (dd.ContainsKey(false))
            {
                return ScanSubDir(path: dd[true].First().path,
                    wilds: dd[false].Select((it) => it.path).ToArray(),
                    mark: "mark-10");
            }
            else
            {
                return ScanSubDir(path: dd[true].First().path,
                    wilds: Array.Empty<string>(),
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
            return (bb.Count(), aa.ContainsKey(false)) switch
            {
                (1, false) => ScanSubDir(path: bb.Keys.First(),
                wilds: bb.Values.First().ToArray(),
                mark: "mark-30"),

                (1, true) => ScanSubDir(path: bb.Keys.First(),
                wilds: bb.Values.First().Concat(aa[false]).ToArray(),
                mark: "mark-40"),

                _ => ListInfo(args, mark: "mark-50")
            }; ;
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
        MyDebugWrite($"*** {nameof(ListInfo)}>Tip='{mark}'>'{string.Join("|",args)}'");
        foreach (var arg in args)
        {
            if (File.Exists(arg))
            {
                var infoFile = new FileInfo(arg);
                Write(Show.Date($"{DateFormatOpt.Invoke(infoFile.LastWriteTime)} "));
                Write(Show.Size(Show.LengthFormatOpt.Invoke(infoFile.Length)));
                WriteLine(arg);
            }
            else if (Directory.Exists(arg))
            {
                var infoDir = new DirectoryInfo(arg);
                Write(Show.Date($"{DateFormatOpt.Invoke(infoDir.LastWriteTime)} "));
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
        MyDebugWrite($"*** {nameof(ScanSubDir)}>Tip='{mark}'>path={path}");

        Wild.InitMatchingNames(wilds);

        if (path.EndsWith(':'))
        {
            path += ".";
        }

        if (false == path.EndsWith(Path.DirectorySeparatorChar))
        {
            path += Path.DirectorySeparatorChar;
        }

        path = io.GetFullPath(path);
        InfoSum sumThe = SubDirOpt.Invoke(path);
        PrintInfoTotal(sumThe);

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
}
