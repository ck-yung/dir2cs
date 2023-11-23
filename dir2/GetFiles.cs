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

    static public partial class io
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

            GetRelativeName = (arg) => arg.Substring(lenThe);
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

    #region Call Enumerator Function Safely
    static internal readonly IEnumerator<string> EmptyEnumStrings
        = Enumerable.Empty<string>().GetEnumerator();
    static IEnumerator<string> SafeGetFileEnumerator(string dirname)
    {
        try { return Directory.EnumerateFiles(dirname).GetEnumerator(); }
        catch { return EmptyEnumStrings; }
    }

    static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
    {
        try { return Directory.EnumerateDirectories(dirname).GetEnumerator(); }
        catch { return EmptyEnumStrings; }
    }

    static bool SafeMoveNext(IEnumerator<string> it)
    {
        try { return it.MoveNext(); }
        catch { return false; }
    }

    static string SafeGetCurrent(IEnumerator<string> it)
    {
        try { return it.Current; }
        catch { return string.Empty; }
    }
    #endregion

    static public IEnumerable<string> ImpGetDirs(string path)
    {
        var enumFile = SafeGetDirectoryEnumerator(path);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }
    }

    static public IEnumerable<string> ImpGetFiles(string path)
    {
        var enumFile = SafeGetFileEnumerator(path);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }
    }

    static public IEnumerable<string> GetAllFiles(string path)
    {
        var enumFile = SafeGetFileEnumerator(path);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }

        var enumDir = SafeGetDirectoryEnumerator(path);
        while (enumDir.MoveNext())
        {
            var currentDirname = SafeGetCurrent(enumDir);
            if (string.IsNullOrEmpty(currentDirname)) continue;
            var dirnameThe = io.GetFileName(currentDirname);
            if (string.IsNullOrEmpty(dirnameThe)) continue;
            if (Wild.ExclDirNameOpt.Invoke(dirnameThe)) continue;
            if (IsFakeDirOrLinked(currentDirname)) continue;
            foreach (var pathThe in GetAllFiles(currentDirname))
            {
                yield return pathThe;
            }
        }
    }

    static public IEnumerable<string> GetAllDirs(string path)
    {
        var dirThe = io.GetFileName(path);
        if (Wild.ExclDirNameOpt.Invoke(dirThe))
        {
            yield break;
        }

        var enumDir = SafeGetDirectoryEnumerator(path);

        if (enumDir != EmptyEnumStrings)
        {
            yield return path;
        }

        while (enumDir.MoveNext())
        {
            var currentDirname = SafeGetCurrent(enumDir);
            if (IsFakeDirOrLinked(currentDirname)) continue;
            if (string.IsNullOrEmpty(currentDirname)) continue;
            var dirnameThe = io.GetFileName(currentDirname);
            if (string.IsNullOrEmpty(dirnameThe)) continue;
            foreach (var pathThe in GetAllDirs(currentDirname))
            {
                yield return pathThe;
            }
        }
    }

    static public InfoSum PrintDirTree(string path)
    {
        void PrintSubTree(string prefix, string dirname)
        {
            var enumDir = SafeGetDirectoryEnumerator(dirname);

            string GetNext()
            {
                while (SafeMoveNext(enumDir))
                {
                    var currDir = SafeGetCurrent(enumDir);
                    var dirThe = Path.GetFileName(currDir);
                    if (false == Wild.ExclDirNameOpt.Invoke(dirThe))
                    {
                        return currDir;
                    }
                }
                return string.Empty;
            }

            var prevDir = GetNext();

            while (true)
            {
                var currDir = GetNext();
                if (string.IsNullOrEmpty(currDir)) break;
                if (false == IsFakeDirOrLinked(prevDir))
                {
                    var dirThe = Path.GetFileName(prevDir);
                    Console.WriteLine($"{prefix}+- {dirThe}");
                    PrintSubTree($"{prefix}|  ", prevDir);
                }
                prevDir = currDir;
            }

            if (!string.IsNullOrEmpty(prevDir) && !IsFakeDirOrLinked(prevDir))
            {
                var dirThe = Path.GetFileName(prevDir);
                Console.WriteLine($"{prefix}\\- {dirThe}");
                PrintSubTree($"{prefix}   ", prevDir);
            }
        }
        Console.WriteLine(path);
        PrintSubTree("", path);
        return InfoSum.Fake;
    }
}
