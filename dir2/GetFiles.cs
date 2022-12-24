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
        static public Func<string, IEnumerable<string>> EnumerateDirectories
        { get; private set; } = Directory.EnumerateDirectories;
        static public Func<string, IEnumerable<string>> EnumerateFiles
        { get; private set; } = Directory.EnumerateFiles;

        static public void InitEnum(
            Func<string, IEnumerable<string>> EnumDirs,
            Func<string, IEnumerable<string>> EnumFiles)
        {
            EnumerateDirectories = EnumDirs;
            EnumerateFiles = EnumFiles;
        }

        static public string InitPath { get; private set; } = "?";
        static public Func<string, string> GetRelativeName
        { get; private set; } = (arg) => arg;

        static public string GetFullPath(string path)
        {
            InitPath = _GetFullPath(path);
            var lenThe = InitPath.Length;
            GetRelativeName = (arg) => arg.Substring(lenThe);
            return InitPath;
        }
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
        try { return Helper.io.EnumerateFiles(dirname).GetEnumerator(); }
        catch { return EmptyEnumStrings; }
    }

    static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
    {
        try { return Helper.io.EnumerateDirectories(dirname).GetEnumerator(); }
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
            // TODO >>>
            //if (Opts.ExclDirnameFilter.Func(
            //    Path.GetFileName(currentDirname))) continue;
            var dirnameThe = Helper.io.GetFileName(currentDirname);
            if (string.IsNullOrEmpty(dirnameThe)) continue;
            if (dirnameThe[0] == '.') continue;
            // TODO <<<
            foreach (var pathThe in GetAllFiles(currentDirname))
            {
                yield return pathThe;
            }
        }
    }

    static public IEnumerable<string> GetAllDirs(string path)
    {
        var enumDir = SafeGetDirectoryEnumerator(path);
        if (enumDir!=Helper.EmptyEnumStrings)
        {
            yield return path;
        }

        while (enumDir.MoveNext())
        {
            var currentDirname = SafeGetCurrent(enumDir);
            if (string.IsNullOrEmpty(currentDirname)) continue;
            // TODO >>>
            //if (Opts.ExclDirnameFilter.Func(
            //    Path.GetFileName(currentDirname))) continue;
            var dirnameThe = Helper.io.GetFileName(currentDirname);
            if (string.IsNullOrEmpty(dirnameThe)) continue;
            if (dirnameThe[0] == '.') continue;
            // TODO <<<
            foreach (var pathThe in GetAllDirs(currentDirname))
            {
                yield return pathThe;
            }
        }
    }
}
