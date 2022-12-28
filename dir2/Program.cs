using System.Collections.Immutable;
using static dir2.MyOptions;
using static dir2.Helper;

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
            if (GetExeEnvr().Contains("--dump-exception-stack"))
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

	static bool RunMain(string[] mainArgs)
	{
		if (mainArgs.Contains("--version"))
		{
			WriteLine(GetVersion());
			return false;
		}

        if (mainArgs.Contains("--help") ||
            mainArgs.Contains("-?"))
        {
            if (mainArgs.Contains("cfg") || mainArgs.Contains("config"))
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

        var tmp2 = cfgRest.Concat(ExpandFromShortCut(mainArgs)
            .Select((it) => (ArgType.CommandLine, it)));
        var tmp3 = Parsers.Resolve(tmp2,
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
                .Where((it) => it != Program.CfgOffOpt)
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
                    $"Unknown '{tmp5.Key}' options: {tmp6}");
            }
        }

        Show.EncodeConsoleOutput();

        var pathThe = "." + Path.DirectorySeparatorChar;

        if (args.Length == 1)
        {
            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
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
                Wild.InitMatchingNames(
                    new string[] { Path.GetFileName(args[0])});
            }
        }
        else if (args.Length> 0)
		{
            PrintDirOptBothOff();
            if (args.Where((it) => it.Contains(Path.DirectorySeparatorChar)).Any())
            {
                var bb = args
                    .GroupBy((it) => Path.GetDirectoryName(it))
                    .ToImmutableDictionary((grp)=>grp.Key, (grp)=>grp.ToArray());
                if (bb.Count>1)
                {
                    WriteLine($"""
                    Syntax: {ExeName} DIR{Path.DirectorySeparatorChar}WILD [OPTION ..]
                    
                    Syntax: {ExeName} WILD [WILD ..] [OPTION ..]
                    where all WILD have same directory.
                    """);
                    return false;
                }

                var cc = new List<string>() { bb.Keys.First() };
                cc.AddRange(args.Select((it) => Path.GetFileName(it)));
                args = cc.ToArray();
            }

            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
                Wild.InitMatchingNames(args.Skip(1));
            }
            else
            {
                PrintDirOptBothOff();
                Wild.InitMatchingNames(args);
            }
        }

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

        pathThe = io.GetFullPath(pathThe);
        InfoSum sumThe = SubDirOpt.Invoke(pathThe);
        PrintInfoTotal(sumThe);

        return true;
	}
}
