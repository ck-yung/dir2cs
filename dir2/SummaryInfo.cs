namespace dir2;

internal static class SummaryInfo
{
    public enum FixedText
    {
        ZeroDir,
        ZeroFile,
        OneDir,
        OneFile,
    }

    public enum StringFormat
    {
        DirNotFound,
        DirZeroWithWild,
        DirZeroOnDir,
        DirOther,
        FileNotFound,
        FileZeroWithWild,
        FileZeroOnDir,
        FileOther,
    }

    public enum StringFormat2
    {
        DirOtherOnDir,
        FileOtherOnDir,
    }

    const string PrefixOneFileFound = "Summary.OneFileFound=";
    const string PrefixOneDirFound = "Summary.OneDirFound=";
    const string PrefixNoFileFound = "Summary.NoFileFound=";
    const string PrefixNoDirFoundFormat = "Summary.Format.NoDirFound=";

    const string PrefixFilesFoundFormat = "Summary.Format.FilesFound=";
    const string PrefixDirsFoundFormat = "Summary.Format.DirsFound=";

    const string PrefixNoFileFoundWithWildOnDirFormat =
        "Summary.Format.NoFileFound.Wild.Dir=";
    const string PrefixNoFileFoundWithWildFormat =
        "Summary.Format.NoFileFound.Wild=";

    const string PrefixNoDirFoundWithWildFormat =
        "Summary.Format.NoDirFound.Wild=";
    const string PrefixNoDirFoundWithWildOnDirFormat =
        "Summary.Format.NoDirFound.Wild.Dir=";
    const string PrefixTooManyDirsFoundWithWildOnDirFormat =
        "Summary.Format.TooManyDirsFound.Wild.Dir=";
    const string PrefixTooManyDirsFoundWithWildFormat =
        "Summary.Format.TooManyDirsFound.Wild=";

    public static IEnumerable<string> Init(IEnumerable<string> lines)
    {
        System.Diagnostics.Debug.WriteLine(
            $"{nameof(SummaryInfo)}.{nameof(Init)} is called");

        var textFound = string.Empty;
        var itrThe = lines.GetEnumerator();
        while (itrThe.MoveNext())
        {
            var current = itrThe.Current.Trim();
            if (current.StartsWith(PrefixOneFileFound))
            {
                textFound = current.Substring(PrefixOneFileFound.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    TextMap[FixedText.OneFile] = textFound;
                }
            }
            else if (current.StartsWith(PrefixOneDirFound))
            {
                textFound = current.Substring(PrefixOneDirFound.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    TextMap[FixedText.OneDir] = textFound;
                }
            }
            else if (current.StartsWith(PrefixNoFileFound))
            {
                textFound = current.Substring(PrefixNoFileFound.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    TextMap[FixedText.ZeroFile] = textFound;
                }
            }
            else if (current.StartsWith(PrefixNoDirFoundFormat))
            {
                textFound = current.Substring(PrefixNoDirFoundFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.DirNotFound] = textFound;
                }
            }
            else if (current.StartsWith(PrefixFilesFoundFormat))
            {
                textFound = current.Substring(PrefixFilesFoundFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.FileOther] = textFound;
                }
            }
            else if (current.StartsWith(PrefixDirsFoundFormat))
            {
                textFound = current.Substring(PrefixDirsFoundFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.DirOther] = textFound;
                }
            }
            else if(current.StartsWith(PrefixNoFileFoundWithWildOnDirFormat))
            {
                textFound = current.Substring(
                    PrefixNoFileFoundWithWildOnDirFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatFileZeroWithWildOnDir = textFound;
                }
            }
            else if (current.StartsWith(PrefixNoFileFoundWithWildFormat))
            {
                textFound = current.Substring(
                    PrefixNoFileFoundWithWildFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.FileZeroWithWild] = textFound;
                }
            }
            else if (current.StartsWith(PrefixNoDirFoundWithWildOnDirFormat))
            {
                textFound = current.Substring(
                    PrefixNoDirFoundWithWildOnDirFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatZeroDirOnDir = textFound;
                }
            }
            else if (current.StartsWith(PrefixNoDirFoundWithWildFormat))
            {
                textFound = current.Substring(
                    PrefixNoDirFoundWithWildFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.DirZeroWithWild] = textFound;
                }
            }
            else if (current.StartsWith(PrefixTooManyDirsFoundWithWildOnDirFormat))
            {
                textFound = current.Substring(
                    PrefixTooManyDirsFoundWithWildOnDirFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatTooManyDirOnDir = textFound;
                }
            }
            else if (current.StartsWith(PrefixTooManyDirsFoundWithWildFormat))
            {
                textFound = current.Substring(
                    PrefixTooManyDirsFoundWithWildFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatTooManyDir = textFound;
                }
            }
            else
            {
                yield return current;
            }
        }
    }

    static readonly Dictionary<FixedText, string> TextMap = new()
    {
        [FixedText.ZeroDir] = "No dir is found.",
        [FixedText.OneDir] = "One dir is found.",
        [FixedText.ZeroFile] = "No file is found.",
        [FixedText.OneFile] = "One file is found.",
    };

    static public string Text(FixedText text)
    {
        return TextMap[text];
    }

    static readonly Dictionary<StringFormat, string> FormatMap = new()
    {
        [StringFormat.DirNotFound] = "Dir '{0}' is NOT found.",
        [StringFormat.DirZeroWithWild] = "No dir is found for '{0}'.",
        [StringFormat.DirZeroOnDir] = "No dir is found on '{0}'.",
        [StringFormat.DirOther] = "{0} dirs are found.",
        [StringFormat.FileNotFound] = "File '{0}' is NOT found.",
        [StringFormat.FileZeroWithWild] = "No file is found for '{0}'.",
        [StringFormat.FileZeroOnDir] = "No file is found on '{0}'.",
        [StringFormat.FileOther] = "{0} files are found.",
    };

    public static string Format(StringFormat format, string dir)
    {
        return String.Format(FormatMap[format], dir);
    }

    public static string Format(StringFormat format, int count)
    {
        return String.Format(FormatMap[format], count);
    }

    static readonly Dictionary<StringFormat2, string> FormatMap2 = new()
    {
        [StringFormat2.DirOtherOnDir] = "{0} dir are found on {1}",
        [StringFormat2.FileOtherOnDir] = "{0} files are found on {1}",
    };

    public static string Format(StringFormat2 format, int count, string dir)
    {
        return String.Format(FormatMap2[format], count, dir);
    }

    static string FormatFileZeroWithWildOnDir { get; set; }
        = "No file is found for '{0}' on {1}.";

    public static string NoFileWithWildOnDir(string wild, string dir)
    {
        return String.Format(FormatFileZeroWithWildOnDir, wild, dir);
    }

    static string FormatZeroDirOnDir { get; set; }
        = "No dir matching '{0}' on '{1}'";

    public static string ZeroDirOnDir(string wild, string dir)
    {
        return String.Format(FormatZeroDirOnDir, wild, dir);
    }

    static string FormatTooManyDirOnDir { get; set; }
        = "Too many dir matching '{0}' on '{1}'";

    public static string TooManyDirOnDir(string wild, string dir)
    {
        return String.Format(FormatTooManyDirOnDir, wild, dir);
    }

    static string FormatTooManyDir { get; set; }
        = "Too many dir matching '{0}'";

    public static string TooManyDir(string wild)
    {
        return String.Format(FormatTooManyDir, wild);
    }
}
