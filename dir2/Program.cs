using static dir2.MyOptions;

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
			Helper.WriteLine(Helper.GetVersion());
			return false;
		}

        if (mainArgs.Contains("--help") ||
            mainArgs.Contains("-?"))
        {
            Helper.WriteLine(Helper.GetSyntax());
            return false;
        }

        var args = Parse(mainArgs);

        var pathThe = "." + Path.DirectorySeparatorChar;
		if (args.Length> 0)
		{
            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
                if (!pathThe.EndsWith(Path.DirectorySeparatorChar))
                {
                    pathThe += Path.DirectorySeparatorChar;
                }
                Wild.InitMatchingNames(args.Skip(1), !IsPrintDirOnly);
            }
            else
            {
                Wild.InitMatchingNames(args, !IsPrintDirOnly);
            }
        }

		if (!Directory.Exists(pathThe))
		{
			Helper.WriteLine($"'{pathThe}' is NOT a directory.");
			return false;
		}

        pathThe = Helper.System.GetFullPath(pathThe);
        InfoSum sumThe;
        if (ScanSubDir)
        {
            sumThe = Helper.GetAllFiles(pathThe)
                .Select((it) => Helper.System.ToInfoFile(it))
                .Where((it) => it.IsNotFake())
                .Where((it) => Wild.CheckIfFileNameMatched(it.Name))
                .Invoke(Sort.Files)
                .Invoke((seq) => Sum.Func(seq, pathThe));
        }
        else
        {
            sumThe = PrintDirOption.Invoke(pathThe);
        }
        Helper.PrintInfoTotal(sumThe);

        return true;
	}
}
