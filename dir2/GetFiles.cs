namespace dir2;

public static partial class Helper
{
    #region Call Enumerator Function Safely
    static IEnumerator<string> SafeGetFileEnumerator(string dirname)
    {
        try { return Directory.EnumerateFiles(dirname).GetEnumerator(); }
        catch { return Enumerable.Empty<string>().GetEnumerator(); }
    }

    static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
    {
        try { return Directory.EnumerateDirectories(dirname).GetEnumerator(); }
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

    static public IEnumerable<string> GetFiles(string dirname)
    {
        var enumFile = SafeGetFileEnumerator(dirname);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }
    }

    static public IEnumerable<string> GetAllFiles(string dirname)
    {
        var enumFile = SafeGetFileEnumerator(dirname);
        while (SafeMoveNext(enumFile))
        {
            var currentFilename = SafeGetCurrent(enumFile);
            if (string.IsNullOrEmpty(currentFilename)) continue;
            yield return currentFilename;
        }

        var enumDir = SafeGetDirectoryEnumerator(dirname);
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
            foreach (var filename in GetAllFiles(currentDirname))
            {
                yield return filename;
            }
        }
    }
}
