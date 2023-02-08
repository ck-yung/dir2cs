using System.Collections.Immutable;
using static dir2.MyOptions;
using static dir2.Helper;
using System.Linq;
using dir2;

namespace dir2;
public class Program
{
    static public void Main(string[] args)
    {
		try
		{
			RunMain(args);
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

    static bool RunMain(string[] mainArgs)
	{
		if (mainArgs.Contains("--version") ||
            mainArgs.Contains("-v"))
		{
			WriteLine(GetVersion());
            WriteLine($"Native: {NativeLib.LibraryName()}");
			return false;
		}

        if (mainArgs.Contains("--HELP"))
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

        (bool envrCfgOff, IEnumerable<(ArgType, string)> cfgRest) =
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
            .Select((it) => (ArgType.CommandLine, it)));
        var tmp3 = Parsers.Resolve(
            tmp2.Where((it) => false == EnvrSkipOpts.Contains(it.Item2)),
            isIncludeExclNameOptions: true)
            .GroupBy((it) => it.Item1 == ArgType.CommandLine)
            .ToImmutableDictionary(
            (grp) => grp.Key,
            (grp) => grp.AsEnumerable());

        string[] args = Array.Empty<string>();
        if (tmp3.ContainsKey(true))
        {
            args = tmp3[true]
                .Select((it) => it.Item2)
                .ToArray();
        }

        if (tmp3.ContainsKey(false))
        {
            foreach (var tmp4 in tmp3[false]
                .GroupBy((it) => it.Item1))
            {
                var tmp5 = string.Join(" ",
                    tmp4.Select((it) => it.Item2).ToArray());
                Console.Error.WriteLine(
                    $"Unknown {tmp4.Key} options: {tmp5}");
            }
        }

        Show.EncodeConsoleOutput();

        var pathThe = "." + Path.DirectorySeparatorChar;

        if (args.Length > 0 )
        {
            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
                args = args.Skip(1).ToArray();
            }
        }

        if (GetExeEnvr().Contains(DebugOpt))
        {
            Console.WriteLine($"path='{pathThe}'");
            Console.Write("args:");
            foreach (var arg in args) Console.Write($" '{arg}'");
            Console.WriteLine();
        }
        Wild.InitMatchingNames(args);

        if (pathThe.EndsWith(':'))
        {
            pathThe += ".";
        }

        if (false == pathThe.EndsWith(Path.DirectorySeparatorChar))
        {
            pathThe += Path.DirectorySeparatorChar;
        }

        if (!Directory.Exists(pathThe))
		{
			WriteLine($"Dir '{args[0]}' is NOT found.");
			return false;
		}

        if (GetExeEnvr().Contains(DumpArgsIfNothingOpt))
        {
            DumpArgsAction = () =>
            {
                Console.Write($" on {pathThe}");
                if (args.Length > 0)
                {
                    Console.Write(" for ");
                    Console.Write(string.Join(' ', args));
                }
            };
        }

        pathThe = io.GetFullPath(pathThe);
        InfoSum sumThe = SubDirOpt.Invoke(pathThe);
        PrintInfoTotal(sumThe);
        return true;
	}

    static bool DirNamesAreFound(string[] args)
    {
        var aa = args
            .Select((it) => (it, File.Exists(it), Directory.Exists(it)))
            .GroupBy((pair) => pair.Item2 || pair.Item3)
            .ToDictionary((grp) => grp.Key, (grp) => grp.ToList());
        if (aa.ContainsKey(true))
        {
            var cntDir = aa[true]
                .Where((pair) => pair.Item3)
                .Select((pair) => pair.Item1)
                .Select((it) => ToInfoDir(it))
                .Where((it) => it.IsNotFake())
                .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
                .Where((it) => (false == Wild.ExclDirNameOpt.Invoke(it.Name)))
                .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
                .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
                .Invoke(Sort.Dirs)
                .Invoke(Sort.ReverseDir)
                .Invoke(Sort.TakeDir)
                .Select((it) =>
                {
                    ItemWrite(Show.Attributes(it));
                    ItemWrite(Show.Owner(it));
                    ItemWrite(DirPrefixText("DIR "));
                    ItemWrite(Show.Date($"{DateFormatOpt.Invoke(Show.GetDate(it))} "));
                    ItemWrite(Show.GetDirName(io.GetRelativeName(it.FullName)));
                    ItemWrite(Show.Link.Invoke(it));
                    ItemWriteLine(string.Empty);
                    return it;
                })
                .Count();
                PrintDirCount(cntDir);

            var sumFile = aa[true]
                .Where((pair) => pair.Item2)
                .Select((pair) => pair.Item1)
                .Select((it) => ToInfoFile(it))
                .Where((it) => it.IsNotFake())
                .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
                .Where((it) => (false == Wild.ExclFileNameOpt.Invoke(it.Name)))
                .Where((it) => Wild.IsMatchWithinSize(it.Length))
                .Where((it) => Wild.IsMatchWithinDate(Show.GetDate(it)))
                .Where((it) => Wild.IsMatchNotWithinSize(it.Length))
                .Where((it) => Wild.IsMatchNotWithinDate(Show.GetDate(it)))
                .Where((it) => Wild.ExtensionOpt.Invoke(it))
                .Where((it) => IsHiddenFileOpt.Invoke(it))
                .Where((it) => IsLinkFileOpt.Invoke(it))
                .Invoke(Sum.Reduce);
            PrintInfoTotal(sumFile);
        }
        if (aa.ContainsKey(false))
        {
            Console.WriteLine("Unknown arg: " + // *** TODO
                string.Join(",",
                aa[false].Select((pair)=>pair.Item1)));
        }
        return false;
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
                yield return homeDir;
            }
            else if (1 < current.Length && '~' == current[0] &&
                Path.DirectorySeparatorChar == current[1])
            {
                yield return Path.Combine(homeDir, current.Substring(2));
            }
            else
            {
                yield return current;
            }
        }
    }
}
