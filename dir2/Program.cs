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
                // TODO: PrintDirOnly
                var path2 = Path.GetDirectoryName(args[0]);
                pathThe = string.IsNullOrEmpty(path2) ? "." : path2;
                Wild.InitMatchingNames(
                    new string[] { Path.GetFileName(args[0])},
                    !IsPrintDirOnly);
            }
        }
        else if (args.Length> 0)
		{
            if (Directory.Exists(args[0]))
            {
                pathThe = args[0];
                Wild.InitMatchingNames(args.Skip(1), !IsPrintDirOnly);
            }
            else
            {
                Wild.InitMatchingNames(args, !IsPrintDirOnly);
            }
        }

        if (!pathThe.EndsWith(Path.DirectorySeparatorChar))
        {
            pathThe += Path.DirectorySeparatorChar;
        }

        if (!Directory.Exists(pathThe))
		{
			Helper.WriteLine($"Dir '{args[0]}' is NOT found.");
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
