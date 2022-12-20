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

        var aa = mainArgs
            .Select((it) => (false, it));
        aa = ParseDirectoryOption(aa);
        aa = ParseSubDirOption(aa);

        var args = aa.Select((it) => it.Item2).ToArray();

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

        if (IsScanSubDir)
        {
            pathThe = Path.GetFullPath(pathThe);
            var cntFile = Helper.GetAllFiles(pathThe)
                .Select((it) => new FileInfo(it))
                .Select((it) =>
                {
                    Console.Write($"{it.Length,8} ");
                    Console.Write(it.LastWriteTime.ToString("u"));
                    Console.Write(" ");
                    var a2 = Path.Join(it.DirectoryName, it.Name);
                    Console.WriteLine(a2.Substring(pathThe.Length));
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
            if (ScanOptionThe != ScanOption.ExcludeDirectory)
            {
                var cntDir = Helper.GetDirs(pathThe)
                    .Select((it) => new DirectoryInfo(it))
                    .Select((it) =>
                    {
                        Console.Write("DIR ");
                        Console.Write(it.LastWriteTime.ToString("u"));
                        Console.Write(" ");
                        Console.WriteLine(it.Name);
                        return it;
                    })
                    .Count();
                if (cntDir > 1)
                {
                    Console.WriteLine($"{cntDir} directories are found.");
                    Console.WriteLine();
                }
            }

            if (ScanOptionThe != ScanOption.ExcludeFile)
            {
                var cntFile = Helper.GetFiles(pathThe)
                    .Select((it) => new FileInfo(it))
                    .Select((it) =>
                    {
                        Console.Write($"{it.Length,8} ");
                        Console.Write(it.LastWriteTime.ToString("u"));
                        Console.Write(" ");
                        Console.WriteLine(it.Name);
                        return it;
                    })
                    .Count();
                if (cntFile > 1)
                {
                    Console.WriteLine($"{cntFile} files are found.");
                }
            }
        }

        return true;
	}

	enum ScanOption
	{
		Both,
		ExcludeDirectory,
		ExcludeFile,
	}

	static ScanOption ScanOptionThe = ScanOption.Both;

	static IEnumerable<(bool,string)> ParseDirectoryOption(
		IEnumerable<(bool,string)> args)
	{
        var it = args.GetEnumerator();
        while (it.MoveNext())
        {
            var current = it.Current;
            if (current.Item2 != "--dir")
            {
                yield return current;
            }
            else
            {
                if (!it.MoveNext())
                    throw new ArgumentException("Missing value to --dir");
                current = it.Current;
                switch (current.Item2)
                {
                    case "both":
                        ScanOptionThe = ScanOption.Both;
                        break;
                    case "only":
                        ScanOptionThe = ScanOption.ExcludeFile;
                        break;
                    case "off":
                        ScanOptionThe = ScanOption.ExcludeDirectory;
                        break;
                    default:
                        throw new ArgumentException(
                            $"Bad value '{current.Item2}' to --dir");
                }
            }
        }
	}

	static bool IsScanSubDir = false;

	static IEnumerable<(bool,string)> ParseSubDirOption(
		IEnumerable<(bool,string)> args)
	{
        var it = args.GetEnumerator();
        while (it.MoveNext())
        {
            var current = it.Current;
            if (current.Item2 == "--sub")
            {
                IsScanSubDir = true;
            }
            else
            {
                yield return current;
            }
        }
    }
}
