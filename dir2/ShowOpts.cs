using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static internal class Show
{
    static readonly Func<string, string> blank = (arg) => string.Empty;
    static string Blank<T>(T _) { return ""; }

    static public Func<string, string> GetDirName
    { get; private set; } = (dirname) => dirname.TrimEnd(Path.DirectorySeparatorChar);

    static public Func<string, string> Date { get; private set; } = Helper.itself;
    static public Func<string, string> Size { get; private set; } = Helper.itself;
    static public Func<string, string> Count { get; private set; } = Helper.itself;
    static public Func<InfoBase, string> Link { get; private set; } = Blank;
    static public Func<InfoBase, string> Attributes { get; private set; } = Blank;
    static public Func<InfoBase, string> Owner { get; private set; } = Blank;

    static public readonly IParse HideOpt = new SimpleParser(name: "--hide",
        help: "date,size,count,link,mode,owner",
        resolve: (parser, args) =>
        {
            foreach (var arg in Helper.CommonSplit(args))
            {
                switch (arg)
                {
                    case "date":
                        Date = blank;
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
        help: "date,size,count,link,mode,owner",
        resolve: (parser, args) =>
        {
            foreach (var arg in Helper.CommonSplit(args))
            {
                switch (arg)
                {
                    case "date":
                        Date = Helper.itself;
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
                    default:
                        throw new ArgumentException($"Bad value '{arg}' to {parser.Name}");
                }
            }
        });

    static public Func<InfoBase, DateTime> GetDate { get; private set; }
    = (it) => it.LastWriteTime;

    static public readonly IParse CreationDateOpt = new SwitchParser("--creation-date",
        action: () =>
        {
            GetDate = (it) => it.CreationTime;
        });

    static internal readonly IInovke<int, string> CountFormat =
        new ParseInvoker<int, string>("--count-format", help: "short | comma;WIDTH",
            init: (it) => $"{it,5} ", resolve: (parser, args) =>
            {
                var pattern = @"\d+|comma|short";
                var aa = Helper.CommonSplit(args).OrderBy((it) => it).Take(4).ToArray();
                foreach (var a2 in aa)
                {
                    if (false == Regex.Match(a2, pattern, RegexOptions.None).Success)
                    {
                        throw new ArgumentException($"'{a2}' is bad to {parser.Name}");
                    }
                }

                if (aa.Contains("short"))
                {
                    if (aa.Length > 1) throw new ArgumentException($"Too many values to {parser.Name}");
                    parser.SetImplementation((val) => Helper.ToKiloUnit(val));
                    return;
                }

                if (aa.Length == 1)
                {
                    if (aa[0] == "comma")
                    {
                        parser.SetImplementation((it) => $"{it,5:N0} ");
                        return;
                    }
                    else
                    {
                        if (int.TryParse(aa[0], out int width))
                        {
                            if (width > 30)
                                throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
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
                            throw new ArgumentException($"'{aa[0]}' is too largth width to {parser.Name}");
                        var fmtThe = $"{{0,{width}:N0}} ";
                        parser.SetImplementation((it) => string.Format(fmtThe, it));
                        return;
                    }
                    throw new ArgumentException($"'{aa[0]}' is NOT width to {parser.Name}");
                }
                else
                {
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
            help: "short | comma;WIDTH",
            init: (it) => $"{it,8} ", resolve: (parser, args) =>
            {
                var pattern = @"\d+|comma|short";
                var aa = Helper.CommonSplit(args).OrderBy((it) => it).Take(4).ToArray();
                foreach (var a2 in aa)
                {
                    if (false == Regex.Match(a2, pattern, RegexOptions.None).Success)
                    {
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
}
