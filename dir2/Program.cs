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
                Console.WriteLine($"Exception: {ee.Message}");
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
			pathThe = args[0];
			if (!pathThe.EndsWith(Path.DirectorySeparatorChar))
			{
                pathThe += Path.DirectorySeparatorChar;
			}
		}

		if (!Directory.Exists(pathThe))
		{
			Helper.WriteLine($"'{pathThe}' is NOT a directory.");
			return false;
		}

        if (ScanSubDir)
        {
            pathThe = Helper.System.GetFullPath(pathThe);
            var cntFile = Helper.GetAllFiles(pathThe)
                .Select((it) => Helper.System.ToInfoFile(it))
                .Invoke(SortFiles)
                .Select((it) =>
                {
                    Helper.Write($"{it.Length,8} ");
                    Helper.Write(it.LastWriteTime.ToString("u"));
                    Helper.Write(" ");
                    Helper.WriteLine(it.FullName.Substring(pathThe.Length));
                    return it;
                })
                .Count();
            if (cntFile > 1)
            {
                Helper.WriteLine($"{cntFile} files are found.");
            }
        }
        else
        {
            PrintDirOption.Invoke(pathThe);
        }

        return true;
	}
}
