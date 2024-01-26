using static dir2.MyOptions;

namespace dir2;

static public partial class Helper
{
    static public string Write(string msg)
    {
        Console.Write(msg);
        return msg;
    }

    static public string WriteLine(string msg)
    {
        Console.WriteLine(msg);
        Show.PauseOpt.Invoke(false);
        return msg;
    }

    static public void WriteTotalLine(string msg, bool isExtraNewLine = false)
    {
        if (false == string.IsNullOrEmpty(msg))
        {
            Show.Color.TotalLine();
            Show.PauseOpt.Invoke(false);
            Console.WriteLine(msg);
        }

        if (isExtraNewLine)
        {
            Console.WriteLine();
            Show.PauseOpt.Invoke(false);
        }

        Show.Color.Reset();
    }

    static public partial class Io
    {
        static public string InitPath { get; private set; } = "?";
        static public string RealInitPath { get; private set; } = string.Empty;
        static public Func<string, string> GetRelativeName
        { get; private set; } = (arg) => arg;

        static public string GetFullPath(string path)
        {
            InitPath = _GetFullPath(path);
            RealInitPath = path;
            var lenThe = InitPath.Length;
            if (KeepDirOpt && (lenThe > path.Length) &&
                (path != ("." + Path.DirectorySeparatorChar)) &&
                (false == path.Contains("..")))
            {
                lenThe -= path.Length;
            }

            if (Show.IsOututCsv)
            {
                GetRelativeName = (arg) => "\""+ arg.Substring(lenThe) + "\"";
            }
            else
            {
                GetRelativeName = (arg) => arg.Substring(lenThe);
            }

            return InitPath;
        }

        static public readonly ImplicitBool KeepDirOpt =
            new SwitchParser(name: "--keep-dir");

        static public Func<string, string> _GetFullPath
        { get; private set; } = Path.GetFullPath;

        static public Func<string, string> GetFileName
        { get; private set; } = Path.GetFileName;

        static public void InitGetNames(
            Func<string,string> GetFullPath,
            Func<string,string> GetFileName)
        {
            _GetFullPath= GetFullPath;
            Io.GetFileName= GetFileName;
        }
    }

    static public IEnumerable<string> GetAllFiles(InfoDir dir)
    {
        foreach (var filename in dir.GetFiles()
            .Where((it) => false == String.IsNullOrEmpty(it)))
        {
            yield return filename;
        }

        foreach (var dirNext in dir.GetDirectories())
        {
            foreach (var filename in GetAllFiles(dirNext))
            {
                yield return filename;
            }
        }
    }

    static public IEnumerable<InfoDir> GetAllDirs(InfoDir dir)
    {
        foreach (var dirNext2 in dir.GetDirectories())
        {
            yield return dirNext2;
            foreach (var dirNext3 in GetAllDirs(dirNext2))
            {
                yield return dirNext3;
            }
        };
    }

    static public InfoSum PrintDirTree(string path)
    {
        var colorThe = Show.ColorOpt.Invoke(6).GetEnumerator();
        int GetNextColor()
        {
            colorThe.MoveNext();
            return colorThe.Current();
        }

        void PrintSubTree(string prefix, InfoDir dir)
        {
            var enumDir = dir.GetDirectories()
                .GetEnumerator();

            if (false == enumDir.MoveNext()) return;
            var prevDir = enumDir.Current;

            while (enumDir.MoveNext())
            {
                var currDir = enumDir.Current;
                if (currDir.IsFake) break;
                var a2 = GetNextColor();
                Helper.WriteLine(Show.Color.SwitchFore($"{prefix}+- {prevDir.Name}"));
                PrintSubTree($"{prefix}|  ", prevDir);
                prevDir = currDir;
            }

            if (prevDir.IsNotFake)
            {
                var a3 = GetNextColor();
                Helper.WriteLine(Show.Color.SwitchFore($"{prefix}\\- {prevDir.Name}"));
                PrintSubTree($"{prefix}   ", prevDir);
            }
        }

        var infoThe = ToInfoDir(path);
        if (infoThe.IsFake)
        {
            Console.WriteLine($"Dir '{path}' is NOT found!");
        }
        else
        {
            Helper.WriteLine(path);
            PrintSubTree("", infoThe);
        }
        return InfoSum.Fake;
    }
}
