using System.Globalization;

namespace dir2;

static public class Sum
{
    static public bool IsFuncChanged { get; private set; } = false;
    static private Func<IEnumerable<InfoFile>, InfoSum> _func =
        (seq) => seq
        .Invoke(Sort.Files)
        .Invoke(Sort.ReverseInfo)
        .Invoke(Sort.TakeInfo)
        .Select((it) =>
        {
            Helper.ItemWrite(Show.Attributes(it));
            Helper.ItemWrite(Show.Owner(it));
            Helper.ItemWrite(Show.Size(Show.LengthFormatOpt.Invoke(it.Length)));
            Helper.ItemWrite(Show.Date(Helper.DateFormatOpt.Invoke(Show.GetDate(it))));
            Helper.ItemWrite(Helper.Io.GetRelativeName(it.FullName));
            Helper.ItemWrite(Show.Link.Invoke(it));
            Helper.ItemWriteLine(string.Empty);
            return it;
        })
        .Aggregate(
            seed: new InfoSum(isBase:true),
            func: (acc, it) => acc.AddWith(it));

    static public Func<IEnumerable<InfoFile>, InfoSum> Reduce
    {
        get => _func;
        private set
        {
            _func = value;
            IsFuncChanged = true;
        }
    }

    static public readonly IParse Opt = new MyOptions.SimpleParser(name: "--sum",
        help: "ext | dir | +dir | year",
        resolve: (parser, args) =>
        {
            var argThe = Helper.GetUnique(args, parser);

            Func<IEnumerable<InfoSum>, InfoSum> reduceTotal =
            (seq) => seq
            .Invoke(Sort.Sums)
            .Invoke(Sort.ReverseSum)
            .Invoke(Sort.TakeSum)
            .Select((it) =>
            {
                it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                return it;
            })
            .Aggregate(
                seed: new InfoSum(isBase:true),
                func: (acc, it) => acc.AddWith(it));

            switch (argThe)
            {
                case "dir":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => Helper.GetFirstDir(Path.GetDirectoryName(
                            Helper.Io.GetRelativeName(it.FullName))))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "." : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(reduceTotal);
                    break;
                case "ext":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => Wild.GetRawText(it.Extension))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "*NO-EXT*" : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(reduceTotal);
                    break;
                case "+dir":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) =>
                    {
                        var qry2 = seq
                        .GroupBy((it) => Helper.GetFirstDir(
                            Path.GetDirectoryName(Helper.Io.GetRelativeName(it.FullName))))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "." : grp.Key),
                            func: (acc, it) => acc.AddWith(it)));

                        var qry3 =
                        from dirName in new string[] { "." }.AsEnumerable()
                        .Union(Directory.EnumerateDirectories(Helper.Io.InitPath))
                        .Select((it) => Path.GetFileName(it))
                        join dirThe in qry2
                        on dirName equals dirThe.Name into joinQuery
                        from joinThe in joinQuery.DefaultIfEmpty()
                        select (joinThe == null) ? new InfoSum(dirName,false) : joinThe;

                        return qry3.Invoke(reduceTotal);
                    };
                    break;
                case "year":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => Show.GetDate(it).ToString("Yyyyy"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(reduceTotal);
                    break;
                default:
                    throw new ArgumentException($"Bad value '{argThe}' to {parser.Name}");
            }
        });
}
