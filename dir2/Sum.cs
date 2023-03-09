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
            Helper.ItemWrite(Show.Date($"{Helper.DateFormatOpt.Invoke(Show.GetDate(it))} "));
            Helper.ItemWrite(Helper.io.GetRelativeName(it.FullName));
            Helper.ItemWrite(Show.Link.Invoke(it));
            Helper.ItemWriteLine(string.Empty);
            return it;
        })
        .Aggregate(
            seed: new InfoSum(Helper.io.RealInitPath),
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
        help: "ext | dir | +dir | year | month | week | day",
        resolve: (parser, args) =>
        {
            var aa = args
            .Where((it) => it.Length > 0)
            .Distinct()
            .Take(2)
            .ToArray();

            if (aa.Length>1)
                throw new ArgumentException(
                    $"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "dir":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => Helper.GetFirstDir(Path.GetDirectoryName(
                            Helper.io.GetRelativeName(it.FullName))))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "." : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "ext":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => Wild.GetRawText(it.Extension))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "*NO-EXT*" : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "+dir":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) =>
                    {
                        var qry2 = seq
                        .GroupBy((it) => Helper.GetFirstDir(
                            Path.GetDirectoryName(Helper.io.GetRelativeName(it.FullName))))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "." : grp.Key),
                            func: (acc, it) => acc.AddWith(it)));

                        var qry3 =
                        from dirName in new string[] { "." }.AsEnumerable()
                        .Union(Directory.EnumerateDirectories(Helper.io.InitPath))
                        .Select((it) => Path.GetFileName(it))
                        join dirThe in qry2
                        on dirName equals dirThe.Name into joinQuery
                        from joinThe in joinQuery.DefaultIfEmpty()
                        select (joinThe == null) ? new InfoSum(dirName,false) : joinThe;

                        return qry3
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    };
                    break;
                case "year":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => "Y"+ Show.GetDate(it).ToString("yyyy"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "month":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => "M" + Show.GetDate(it).ToString("yyyy-MM"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "week":
                case "weekMonday":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => "W" + Show.GetDate(it).ToString("yyyy")
                        + "-" + CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                            Show.GetDate(it),
                            CalendarWeekRule.FirstDay, DayOfWeek.Monday
                            ).ToString("d2"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "weekSunday":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => "W" + Show.GetDate(it).ToString("yyyy")
                        + "-" + CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                            Show.GetDate(it),
                            CalendarWeekRule.FirstDay, DayOfWeek.Sunday
                            ).ToString("d2"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                case "day":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Reduce = (seq) => seq
                        .GroupBy((it) => "D" + Show.GetDate(it).ToString("yyyy-MM-dd"))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name: grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke(Sort.Sums)
                        .Invoke(Sort.ReverseSum)
                        .Invoke(Sort.TakeSum)
                        .Select((it) =>
                        {
                            it.Print(Helper.ItemWrite, Helper.ItemWriteLine);
                            return it;
                        })
                        .Aggregate(
                            seed: new InfoSum(Helper.io.RealInitPath),
                            func: (acc, it) => acc.AddWith(it));
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });
}
