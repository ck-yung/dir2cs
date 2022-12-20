namespace dir2;

static public partial class Helper
{
    static public partial class System
    {
        static public Func<string, IEnumerable<string>> EnumerateDirectories
        { get; private set; } = Directory.EnumerateDirectories;
        static public Func<string, IEnumerable<string>> EnumerateFiles
        { get; private set; } = Directory.EnumerateFiles;

        static public void Init(
            Func<string, IEnumerable<string>> EnumDirs,
            Func<string, IEnumerable<string>> EnumFiles)
        {
            EnumerateDirectories = EnumDirs;
            EnumerateFiles = EnumFiles;
        }
    }

    #region Call Enumerator Function Safely
    static IEnumerator<string> SafeGetFileEnumerator(string dirname)
    {
        try { return Helper.System.EnumerateFiles(dirname).GetEnumerator(); }
        catch { return Enumerable.Empty<string>().GetEnumerator(); }
    }

    static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
    {
        try { return Helper.System.EnumerateDirectories(dirname).GetEnumerator(); }
        catch { return Enumerable.Empty<string>().GetEnumerator(); }
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

    static public IEnumerable<string> GetDirs(string path)
    {
        var enumFile = SafeGetDirectoryEnumerator(path);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }
    }

    static public IEnumerable<string> GetFiles(string path)
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
            var dirnameThe = Path.GetFileName(currentDirname);
            if (string.IsNullOrEmpty(dirnameThe)) continue;
            if (dirnameThe[0] == '.') continue;
            // TODO <<<
            foreach (var pathThe in GetAllFiles(currentDirname))
            {
                yield return pathThe;
            }
        }
    }
}
