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
            if (Helper.GetExeEnvr().Contains(":exception.stack:"))
            {
                Console.WriteLine(ee);
            }
            else
            {
                Console.WriteLine($"{ee.GetType()}: {ee.Message}");
            }
        }
    }

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
            Write(GetSyntax());
            return false;
        }

        var args = Parse(ExpandFromShortCut(mainArgs));

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

        if (!pathThe.EndsWith(Path.DirectorySeparatorChar))
        {
            pathThe += Path.DirectorySeparatorChar;
        }

        if (!Directory.Exists(pathThe))
		{
			WriteLine($"Dir '{args[0]}' is NOT found.");
			return false;
		}

        pathThe = io.GetFullPath(pathThe);
        InfoSum sumThe = InfoSum.Fake;
        if (ScanSubDir)
        {
            if (PrintDirOpt == MyOptions.PrintDir.Only)
            {
                var cntDir = GetAllDirs(pathThe)
                    .Select((it) => io.ToInfoDir(it))
                    .Where((it) => it.IsNotFake())
                    .Where((it) => (false ==
                    string.IsNullOrEmpty(io.GetRelativeName(it.FullName))))
                    .Where((it) => Wild.CheckIfDirNameMatched(it.Name))
                    .Invoke(Sort.Dirs)
                    .Select((it) =>
                    {
                        ItemWriteLine(io.GetRelativeName(it.FullName));
                        return it;
                    })
                    .Count();
                PrintDirCount(cntDir);
            }
            else
            {
                sumThe = GetAllFiles(pathThe)
                    .Select((it) => io.ToInfoFile(it))
                    .Where((it) => it.IsNotFake())
                    .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
                    .Where((it) => (false == Wild.ExcludeFileName.Invoke(it.Name)))
                    .Invoke(Sort.Files)
                    .Invoke((seq) => Sum.Func(seq, pathThe));
            }
        }
        else
        {
            sumThe = PrintDirOption.Invoke(pathThe);
        }
        PrintInfoTotal(sumThe);

        return true;
	}
}
