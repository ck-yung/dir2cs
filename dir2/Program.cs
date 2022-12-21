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
			Console.Write(ee.GetType());
            Console.Write(": ");
            Console.WriteLine(ee);
		}
    }

	static bool RunMain(string[] mainArgs)
	{
		if (mainArgs.Contains("--version"))
		{
			Console.WriteLine(Helper.GetVersion());
			return false;
		}

        if (mainArgs.Contains("--help") ||
            mainArgs.Contains("-?"))
        {
            Console.WriteLine(Helper.GetSyntax());
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
			Console.WriteLine($"'{pathThe}' is NOT a directory.");
			return false;
		}

        if (ScanSubDir.Flag)
        {
            pathThe = Helper.System.GetFullPath(pathThe);
            var cntFile = Helper.GetAllFiles(pathThe)
                .Select((it) => Helper.System.ToInfoFile(it))
                .Select((it) =>
                {
                    Console.Write($"{it.Length,8} ");
                    Console.Write(it.LastWriteTime.ToString("u"));
                    Console.Write(" ");
                    Console.WriteLine(it.FullName.Substring(pathThe.Length));
                    return it;
                })
                .Count();
            if (cntFile > 1)
            {
                Console.WriteLine($"{cntFile} files are found.");
            }
        }
        else
        {
            PrintDirOption.Invoke(pathThe);
        }

        return true;
	}
}
