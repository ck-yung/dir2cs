using System.Collections.Immutable;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static internal partial class Show
{
    static readonly Func<string, string> blank = (arg) => string.Empty;
    static string Blank<T>(T _) { return ""; }

    static public Func<string, string> GetDirName
    { get; private set; } = (dirname) => dirname.TrimEnd(Path.DirectorySeparatorChar);

    static string getDateText(string arg) => arg + " ";
    static public Func<string, string> Date { get; private set; } = getDateText;

    static string getLastDateTimeText(string arg) => $"- {arg}";
    static public Func<string, string> Last { get; private set; } = getLastDateTimeText;

    static public Func<string, string> Size { get; private set; } = Helper.itself;
    static public Func<string, string> Count { get; private set; } = Helper.itself;
    static public Func<InfoBase, string> Link { get; private set; } = (arg) =>
    {
        if (string.IsNullOrEmpty(arg.LinkTarget))
            return string.Empty;
        return $" -> {arg.LinkTarget}";
    };

    static public Func<InfoBase, string> Attributes { get; private set; } = Blank;
    static public Func<InfoBase, string> Owner { get; private set; } = Blank;


    static public readonly IParse HideOpt = new SimpleParser(name: "--hide",
        help: "date,size,count,mode,owner,last,link",
        resolve: (parser, args) =>
        {
            foreach (var arg in Helper.CommonSplit(args))
            {
                switch (arg)
                {
                    case Helper.ExtraHelp:
                        throw new ArgumentException($"Syntax: {parser.Name} {parser.Help}");
                    case "date":
                        Date = blank;
                        Last = blank;
                        break;
                    case "size":
                        Size = blank;
                        Helper.DirPrefixText = blank;
                        GetDirName = (dirname) =>
                        {
                            if (dirname.EndsWith(Path.DirectorySeparatorChar))
                                return dirname;
                            return dirname + Path.DirectorySeparatorChar;
                        };
                        break;
                    case "count":
                        Count = blank;
                        break;
                    case "last":
                        Last = blank;
                        break;
                    case "link":
                        Link = Blank;
                        break;
                    case "mode":
                        Attributes = Blank;
                        break;
                    case "owner":
                        Owner = Blank;
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{arg}' to {parser.Name}");
                }
            }
        });

    static public readonly IParse Opt = new SimpleParser(name: "--show",
        help: "date,size,count,mode,owner,last,link,link-size,link-date",
        resolve: (parser, args) =>
        {
            foreach (var arg in Helper.CommonSplit(args))
            {
                switch (arg)
                {
                    case Helper.ExtraHelp:
                        throw new ArgumentException($"Syntax: {parser.Name} {parser.Help}");
                    case "date":
                        Date = getDateText;
                        break;
                    case "size":
                        Size = Helper.itself;
                        Helper.DirPrefixText = Helper.itself;
                        GetDirName = (dirname) => dirname.TrimEnd(
                            Path.DirectorySeparatorChar);
                        break;
                    case "count":
                        Count = Helper.itself;
                        break;
                    case "last":
                        Date = getDateText;
                        Last = getLastDateTimeText;
                        break;
                    case "link":
                        Link = (arg) =>
                        {
                            if (string.IsNullOrEmpty(arg.LinkTarget))
                                return string.Empty;
                            return $" -> {arg.LinkTarget}";
                        };
                        break;
                    case "mode":
                        Helper.DirPrefixText = blank;
                        Attributes = (arg) => arg.AttributeText();
                        break;
                    case "owner":
                        Owner = (arg) => arg.OwnerText().PadRight(20);
                        break;
                    case "link-size":
                        GetViewSize = (info) =>
                        {
                            if (String.IsNullOrEmpty(info.LinkTarget))
                            {
                                return info.Length;
                            }
                            try
                            {
                                var infoTo = new FileInfo(info.LinkTarget);
                                return infoTo.Length;
                            }
                            catch
                            {
                                return 0;
                            }
                        };
                        break;
                    case "link-date":
                        GetInfoLink = (info) =>
                        {
                            var linkThe = info.LinkTarget;
                            if (string.IsNullOrEmpty(linkThe))
                            {
                                return info;
                            }
                            try
                            {
                                if (File.Exists(linkThe))
                                {
                                    return new FileInfo(linkThe);
                                }
                                else if (Directory.Exists(linkThe))
                                {
                                    return new DirectoryInfo(linkThe);
                                }
                                else return info;
                            }
                            catch
                            {
                                return info;
                            }
                        };
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{arg}' to {parser.Name}");
                }
            }
        });

    static public Func<InfoBase, DateTimeOffset> GetDate { get; private set; }
    = (it) => it.LastWriteTime;

    static public Func<FileInfo, long> GetViewSize { get; private set; }
    = (it) => it.Length;

    static public Func<FileSystemInfo, FileSystemInfo> GetInfoLink { get; private set; }
    = (it) => it;

    static public readonly IParse CreationDateOpt = new SwitchParser("--creation-date",
        action: () =>
        {
            GetDate = (it) => it.CreationTime;
        });

    static internal readonly IInovke<int, string> CountFormat =
        new ParseInvoker<int, string>("--count-format", help: "short | comma;WIDTH",
            init: (it) => $"{it,5} ", resolve: (parser, args) =>
            {
                var aa = Helper.GetUniqueTexts(args, 2, parser).ToImmutableSortedSet();

                var chkRegex = new Regex(@"\d+|comma|short");
                foreach (var a2 in aa)
                {
                    if (false == chkRegex.IsMatch(a2))
                    {
                        throw new ArgumentException($"'{a2}' is bad to {parser.Name}");
                    }
                }

                switch (aa.Count, aa.Contains("comma"), aa.Contains("short"))
                {
                    case (1, false, true):
                        parser.SetImplementation((val) => Helper.ToKiloUnit(val));
                        break;

                    case (1, true, false):
                        parser.SetImplementation((it) => $"{it,5:N0} ");
                        break;

                    case (1, false, false):
                        if (int.TryParse(aa[0], out int width))
                        {
                            if (width > 30)
                                throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
                            var fmtThe = $"{{0,{width}}} ";
                            parser.SetImplementation((it) => string.Format(fmtThe, it));
                            return;
                        }
                        throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");

                    case (2, true, false):
                        if (int.TryParse(aa[0], out int width2))
                        {
                            if (width2 > 30)
                                throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
                            var fmtThe = $"{{0,{width2}:N0}} ";
                            parser.SetImplementation((it) => string.Format(fmtThe, it));
                            return;
                        }
                        throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");

                    default:
                        throw new ArgumentException($"Bad values is found to {parser.Name}");
                }
            });

    static public Action EncodeConsoleOutput { get; private set; } = () => { };

    static public readonly IParse EncodeConsoleOpt = new SwitchParser("--utf8",
        action: () =>
        {
            EncodeConsoleOutput = () => Console.OutputEncoding = Encoding.UTF8;
        });

    static public readonly IInovke<long, string> LengthFormatOpt =
        new ParseInvoker<long, string>(name: "--size-format",
            help: "short | +short | comma;WIDTH",
            init: (it) => $"{it,8} ", resolve: (parser, args) =>
            {
                var pattern = @"\d+|comma|short";
                var aa = Helper.CommonSplit(args).OrderBy((it) => it).Take(4).ToArray();

                if (aa.Any((it) => it == Helper.ExtraHelp))
                {
                    throw new ArgumentException($"Syntax: {parser.Name} {parser.Help}");
                }

                foreach (var a2 in aa)
                {
                    if (false == Regex.Match(a2, pattern, RegexOptions.None).Success)
                    {
                        if (a2.ToUpper().Equals("WIDTH"))
                        {
                            throw new ArgumentException($"""
                                Command line option could be
                                  --size-format 12
                                  --size-format comma,12
                                  --size-format short
                                  --size-format +short
                                """);
                        }
                        throw new ArgumentException($"'{a2}' is bad to {parser.Name}");
                    }
                }

                if (aa.Contains("short"))
                {
                    if (aa.Length > 1)
                        throw new ArgumentException($"Too many values to {parser.Name}");
                    parser.SetImplementation((val) => Helper.ToKiloUnit(val));
                    return;
                }

                if (aa.Contains("+short"))
                {
                    if (aa.Length > 1)
                        throw new ArgumentException($"Too many values to {parser.Name}");
                    parser.SetImplementation((val) => Helper.ToExtraShortKiloUnit(val));
                    return;
                }

                if (aa.Length == 1)
                {
                    if (aa[0] == "comma")
                    {
                        parser.SetImplementation((it) => $"{it,8:N0} ");
                        return;
                    }
                    else
                    {
                        if (int.TryParse(aa[0], out int width))
                        {
                            if (width > 30)
                                throw new ArgumentException(
                                    $"'{aa[0]}' is too largth width to {parser.Name}");
                            var fmtThe = $"{{0,{width}}} ";
                            parser.SetImplementation((it) => string.Format(fmtThe, it));
                            return;
                        }
                        throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");
                    }
                }
                else if (2 == aa.Length && aa[1] == "comma")
                {
                    if (int.TryParse(aa[0], out int width))
                    {
                        if (width > 30)
                            throw new ArgumentException(
                                $"'{aa[0]}' is too largth width to {parser.Name}");
                        var fmtThe = $"{{0,{width}:N0}} ";
                        parser.SetImplementation((it) => string.Format(fmtThe, it));
                        return;
                    }
                    throw new ArgumentException(
                        $"'{aa[0]}' is NOT width to {parser.Name}");
                }
                else
                {
                    throw new ArgumentException($"Bad values is found to {parser.Name}");
                }
            });

    /// <summary>
    /// Invoke(bool isLeadingSpace)
    /// </summary>
    static public readonly IInovke<bool, string> EndTime =
        new ParseInvoker<bool, string>("--end-time", help: "FORMAT",
            init: (_) => "", resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                if (string.IsNullOrEmpty(argThe)) return;
                argThe = System.Web.HttpUtility.UrlDecode(argThe);

                Helper.impPrintInfoTotal = (path, wilds, arg)
                => Helper.PrintIntoTotalWithFlag( // --total always
                    path, wilds, arg, printEvenCountOne: false);

                parser.SetImplementation((isLeadingSpace) =>
                {
                    var leading = (isLeadingSpace) ? " " : "";
                    try
                    {
                        var rtn = Helper.FromUtcToReportTimeZone(
                            DateTimeOffset.UtcNow).ToString(argThe);
                        if (string.IsNullOrEmpty(rtn)) return string.Empty;
                        return leading + rtn;
                    }
                    catch
                    {
                        return "Error!";
                    }
                });
            });
}
