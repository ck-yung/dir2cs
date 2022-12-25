using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static internal class Show
{
    static readonly Func<string, string> blank = (arg) => string.Empty;
    static public Func<string, string> GetDirName
    { get; private set; } = Helper.itself;

    static public Func<string, string> Date { get; private set; } = Helper.itself;
    static public Func<string, string> Size { get; private set; } = Helper.itself;
    static public Func<string, string> Count { get; private set; } = Helper.itself;

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--hide",
        help: "date,size,count",
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
                        GetDirName = (arg) => arg + Path.DirectorySeparatorChar;
                        break;
                    case "count":
                        Count = blank;
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{arg}' to {parser.Name}");
                }
            }
        });

    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> ReverseInfo
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> ReverseDir
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>> ReverseSum
    { get; private set; } = Helper.itself;

    static internal readonly IParse ReverseOpt = new SwitchParser(
        "--reverse", action: () =>
        {
            ReverseInfo = (seq) => seq.Reverse();
            ReverseDir = (seq) => seq.Reverse();
            ReverseSum = (seq) => seq.Reverse();
        });

    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> TakeInfo
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> TakeDir
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>> TakeSum
    { get; private set; } = Helper.itself;
    static internal readonly IParse TakeOpt = new SimpleParser("--take",
        help: "NUMBER | SIZE", resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            if (int.TryParse(aa[0], out int takeCount))
            {
                if (Sum.IsFuncChanged)
                {
                    int sumCount = 0;
                    TakeSum = (seq) => seq
                    .TakeWhile((it) =>
                    {
                        sumCount += it.Count;
                        return sumCount < takeCount;
                    });
                }
                else
                {
                    if (PrintDirOpt == PrintDir.Only)
                    {
                        TakeDir = (seq) => seq.Take(takeCount);
                    }
                    else
                    {
                        TakeInfo = (seq) => seq.Take(takeCount);
                    }
                }
            }
            else if (Helper.TryParseKiloNumber(aa[0], out long maxSize))
            {
                long sumSize = 0L;
                if (Sum.IsFuncChanged)
                {
                    TakeSum = (seq) => seq
                    .TakeWhile((it) =>
                    {
                        sumSize += it.Length;
                        return sumSize < maxSize;
                    });
                }
                else
                {
                    TakeInfo = (seq) => seq
                    .TakeWhile((it) =>
                    {
                        sumSize += it.Length;
                        return sumSize < maxSize;
                    });
                }
            }
            else
            {
                throw new ArgumentException($"'{aa[0]}' is bad value to {parser.Name}");
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
                    parser.SetImplementation((val) => ToKiloUnit(val));
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
}
