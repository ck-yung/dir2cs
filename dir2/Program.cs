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
                Write(GetSyntax());
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
            .Concat(ExpandFromShortCut(mainArgs)
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
            var tmp4 = tmp3[false].GroupBy((it) => it.Item1);
            foreach (var tmp5 in tmp4)
            {
                var tmp6 = string.Join(" ",
                    tmp5.Select((it) => it.Item2).ToArray());
                Console.Error.WriteLine(
                    $"Unknown {tmp5.Key} options: {tmp6}");
            }
        }

        Show.EncodeConsoleOutput();

        var pathThe = "." + Path.DirectorySeparatorChar;

        if (args.Length == 1)
        {
            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
                args = Array.Empty<string>();
            }
            else
            {
                if (args[0].EndsWith(Path.DirectorySeparatorChar))
                {
                    throw new ArgumentException($"Dir '{args[0]}' is NOT found.");
                }

                PrintDirOptBothOff();
                var path2 = Path.GetDirectoryName(args[0]);
                pathThe = string.IsNullOrEmpty(path2) ? "." : path2;
                args = new string[] { Path.GetFileName(args[0]) };
            }
        }
        else if (args.Length> 0)
		{
            PrintDirOptBothOff();
            if (Directory.Exists(args[0]))
            {
                if (args.Skip(1).Where((it) => it.Contains(Path.DirectorySeparatorChar)).Any())
                {
                    return DirNamesAreFound(args, caseNumber: 11);
                }
                pathThe = args[0];
                args = args.Skip(1).ToArray();
            }
            else
            {
                if (args.Where((it) => it.Contains(Path.DirectorySeparatorChar)).Any())
                {
                    var bb = args
                        .GroupBy((it) => Wild.GetRawText(Path.GetDirectoryName(it)))
                        .ToImmutableDictionary((grp) => grp.Key,
                        (grp) => grp.AsEnumerable());
                    if (bb.Count() > 1)
                    {
                        return DirNamesAreFound(args, caseNumber: 21);
                    }
                    var pathThe2 = bb.First().Key;
                    var pathThe2Length = pathThe2.Length;
                    var pathThe3 = pathThe2 + Path.DirectorySeparatorChar;
                    var pathThe3Length = pathThe2.Length + 1;
                    args = args
                        .Select((it) =>
                        {
                            if (it.StartsWith(pathThe3))
                                return it.Substring(pathThe3Length);
                            return it.Substring(pathThe2Length);
                        })
                        .ToArray();
                    if (pathThe2.EndsWith(":"))
                    {
                        pathThe = pathThe2 + "." + Path.DirectorySeparatorChar;
                    }
                    else
                    {
                        pathThe = pathThe3;
                    }
                }
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

    static bool DirNamesAreFound(string[] args, int caseNumber)
    {
        var bb = args
            .GroupBy((it) => Wild.GetRawText(Path.GetDirectoryName(it)))
            .Take(2)
            .Select((grp) => grp.First())
            .ToArray();
        if (bb.Length == 2)
        {
            if (GetExeEnvr().Contains(DebugOpt))
            {
                Console.WriteLine($"debug: case# {caseNumber}");
                Console.Write("args:");
                foreach (var arg in args) Console.Write($" '{arg}'");
                Console.WriteLine();
            }
            WriteLine($"""
                    Syntax: {ExeName} DIR{Path.DirectorySeparatorChar}WILD [WILD ..] [OPTION ..]

                    Syntax: {ExeName} WILD [WILD ..] [OPTION ..]
                    where all WILD have same directory.
                    """);
            WriteLine($"But dir of '{bb[0]}' is different to '{bb[1]}'.");
        }
        else
        {
            Console.Error.WriteLine("Unhandling error!");
            Console.Error.WriteLine($"debug: case# {caseNumber}");
            Console.Error.Write("args:");
            foreach (var arg in args) Console.Error.Write($" '{arg}'");
            Console.Error.WriteLine();
        }
        return false;
    }
}
