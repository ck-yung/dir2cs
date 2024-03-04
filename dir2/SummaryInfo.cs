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
    const string PrefixNoDirFound = "Summary.NoDirFound=";

    const string PrefixFilesFoundFormat = "Summary.Format.FilesFound=";
    const string PrefixnoFilesFoundWithWildOnDirFormat =
        "Summary.Format.NoFilesFound.Wild.Dir=";
    const string PrefixnoFilesFoundWithWildFormat =
        "Summary.Format.NoFilesFound.Wild=";

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
            else if (current.StartsWith(PrefixNoDirFound))
            {
                textFound = current.Substring(PrefixNoDirFound.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    TextMap[FixedText.ZeroDir] = textFound;
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
            else if (current.StartsWith(PrefixnoFilesFoundWithWildOnDirFormat))
            {
                textFound = current.Substring(
                    PrefixnoFilesFoundWithWildOnDirFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatFileZeroWithWildOnDir = textFound;
                }
            }
            else if (current.StartsWith(PrefixnoFilesFoundWithWildFormat))
            {
                textFound = current.Substring(
                    PrefixnoFilesFoundWithWildFormat.Length);
                if (false == string.IsNullOrEmpty(textFound))
                {
                    FormatMap[StringFormat.FileZeroWithWild] = textFound;
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
}
