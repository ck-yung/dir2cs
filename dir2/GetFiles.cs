using static dir2.MyOptions;

namespace dir2;

static public partial class Helper
{
    static public Action<string> Write
    { get; private set; } = (msg) => Console.Write(msg);
    static public Action<string> WriteLine
    { get; private set; } = (msg) => Console.WriteLine(msg);

    static public void Init(
        Action<string> Write,
        Action<string> WriteLine)
    {
        Helper.Write = Write;
        Helper.WriteLine = WriteLine;
    }

#pragma warning disable CS8981 // The type name only contains lower-cased ascii char.
    static public partial class io
#pragma warning restore CS8981 // The type name only contains lower-cased ascii char.
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
            io.GetFileName= GetFileName;
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
                Console.WriteLine($"{prefix}+- {prevDir.Name}");
                PrintSubTree($"{prefix}|  ", prevDir);
                prevDir = currDir;
            }

            if (prevDir.IsNotFake)
            {
                Console.WriteLine($"{prefix}\\- {prevDir.Name}");
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
            Console.WriteLine(path);
            PrintSubTree("", infoThe);
        }
        return InfoSum.Fake;
    }
}
