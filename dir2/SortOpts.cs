using static dir2.MyOptions;

namespace dir2;

static public class Sort
{
    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> Files
    { get; private set; } = (seq) => seq;

    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> Dirs
    { get; private set; } = (seq) => seq;

    static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>> Sums
    { get; private set; } = (seq) => seq;

    static (Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>>,
        Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>>,
        Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>>)
        unknownValues(string name, string bad1, string bad2)
    {
        throw new ArgumentException($"Value pair ({bad1},{bad2}) is UNKNOWN to {name}.");
    }

    static public readonly IParse Opt = new SimpleParser(name: "--sort",
        help: "off | name | size | date | ext | count | last      (up to 2 columns)",
        resolve: (parser, args) =>
        {
            var aa = Helper.CommonSplit(args).Take(3).ToArray();

            if ((aa.Length > 0) && aa[0] == "name")
            {
                Dirs = (seq) => seq.OrderBy((it) => it.FullName, Wild.StringComparer);
                Files = (seq) => seq.OrderBy((it) => it.FullName, Wild.StringComparer);
                Sums = (seq) => seq.OrderBy((it) => it.Name, Wild.StringComparer);
                return;
            }

            if (aa.Length == 1)
            {
                switch (aa[0])
                {
                    case "off":
                        Dirs = Helper.itself;
                        Files = Helper.itself;
                        Sums = Helper.itself;
                        break;

                    case "size":
                        Files = (seq) => seq.OrderBy((it) => it.Length);
                        Sums = (seq) => seq.OrderBy(it => it.Length);
                        break;
                    case "date":
                        Dirs = (seq) => seq.OrderBy((it) => Show.GetDate(it));
                        Files = (seq) => seq.OrderBy((it) => Show.GetDate(it));
                        Sums = (seq) => seq.OrderBy(it => it.StartTime);
                        break;
                    case "ext":
                        Dirs = (seq) => seq.OrderBy((it) =>
                        it.Extension, Wild.StringComparer);
                        Files = (seq) => seq.OrderBy((it) =>
                        it.Extension, Wild.StringComparer);
                        Sums = (seq) => seq.OrderBy((it) =>
                        Path.GetExtension(it.Name), Wild.StringComparer);
                        break;
                    case "count":
                        Sums = (seq) => seq.OrderBy((it) => it.Count);
                        break;
                    case "last":
                        Sums = (seq) => seq.OrderBy((it) => it.EndTime);
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
                }
            }
            else if (aa.Length == 2)
            {
                (Dirs, Files, Sums) = (aa[0], aa[1]) switch
                {
                    ("size", "name") => (
                    (seq) => seq.OrderBy((it) => it.FullName, Wild.StringComparer),
                    (seq2) => seq2.OrderBy((it) => it.Length)
                    .ThenBy((it) => it.FullName, Wild.StringComparer),
                    (seq3) => seq3.OrderBy((it) => it.Length)
                    .ThenBy((it) => it.Name, Wild.StringComparer)),

                    ("size", "date") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => it.Length).ThenBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.Length).ThenBy((it) => it.StartTime)),

                    ("size", "ext") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Length).ThenBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.Length).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("size", "count") => (
                    (seq) => seq,
                    (seq2) => seq2.OrderBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.Length).ThenBy((it) => it.Count)),

                    ("size", "last") => (
                    (seq) => seq,
                    (seq2) => seq2.OrderBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.Length).ThenBy((it) => it.EndTime)),

                    ("date", "name") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.FullName),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.FullName),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Name)),

                    ("date", "size") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Length)),

                    ("date", "ext") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("date", "count") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Count)),

                    ("date", "last") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.EndTime)),

                    ("ext", "name") => (
                    (seq) => seq.OrderBy((it) => it.Extension).ThenBy((it) => it.FullName),
                    (seq2) => seq2.OrderBy((it) => it.Extension).ThenBy((it) => it.FullName),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.Name)),

                    ("ext", "size") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Extension).ThenBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.Length)),

                    ("ext", "date") => (
                    (seq) => seq.OrderBy((it) => it.Extension).ThenBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => it.Extension).ThenBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.StartTime)),

                    ("ext", "count") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.Count)),

                    ("ext", "last") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.EndTime)),

                    ("count", "name") => (
                    (seq) => seq.OrderBy((it) => it.FullName),
                    (seq2) => seq2.OrderBy((it) => it.FullName),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.Name)),

                    ("count", "size") => (
                    (seq) => seq,
                    (seq2) => seq2.OrderBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.Length)),

                    ("count", "date") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.StartTime)),

                    ("count", "ext") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("count", "last") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.EndTime)),

                    ("last", "name") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.FullName),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.FullName),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Name)),

                    ("last", "size") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Length)),

                    ("last", "ext") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)).ThenBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("last", "count") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Count)),

                    ("last", "date") => (
                    (seq) => seq.OrderBy((it) => Show.GetDate(it)),
                    (seq2) => seq2.OrderBy((it) => Show.GetDate(it)),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.StartTime)),

                    _ => unknownValues(name: parser.Name, aa[0], aa[1])
                };
            }
            else
            {
                throw new ArgumentException($"Too many values to {parser.Name}");
            }
        });

    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> ReverseInfo
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> ReverseDir
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>> ReverseSum
    { get; private set; } = Helper.itself;

    static internal readonly IParse ReverseOpt = new SimpleParser(
        "--reverse", help: "off | on", resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "off":
                    ReverseInfo = Helper.itself;
                    ReverseDir = Helper.itself;
                    ReverseSum = Helper.itself;
                    break;
                case "on":
                    ReverseInfo = (seq) => seq.Reverse();
                    ReverseDir = (seq) => seq.Reverse();
                    ReverseSum = (seq) => seq.Reverse();
                    break;
                default:
                    throw new ArgumentException($"'{aa[0]}' is bad value to {parser.Name}");
            }
        });

    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> TakeInfo
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> TakeDir
    { get; private set; } = Helper.itself;
    static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>> TakeSum
    { get; private set; } = Helper.itself;
    static internal readonly IParse TakeOpt = new SimpleParser("--take",
        help: "NUMBER | SIZE   where SIZE ends with k, m or g", resolve: (parser, args) =>
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
                    if (PrintDir == EnumPrintDir.Only)
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
}
