namespace dir2;

static public class Sum
{
    static public bool IsFuncChanged { get; private set; } = false;
    static private Func<IEnumerable<InfoFile>, InfoSum>
        _func = (seq) => seq
        .Invoke(Sort.Files)
        .Invoke(Show.ReverseInfo)
        .Invoke(Show.TakeInfo)
        .Select((it) =>
        {
            Helper.ItemWrite(Show.Size(MyOptions.LengthFormat.Invoke(it.Length)));
            Helper.ItemWrite(Show.Date($"{Show.DateFormatOpt.Invoke(Show.GetDate(it))} "));
            Helper.ItemWriteLine(Helper.io.GetRelativeName(it.FullName));
            return it;
        })
        .Aggregate(
            seed: new InfoSum(Helper.io.RealInitPath),
            func: (acc, it) => acc.AddWith(it));

    static public Func<IEnumerable<InfoFile>, InfoSum> Func
    {
        get => _func;
        private set
        {
            _func = value;
            IsFuncChanged = true;
        }
    }

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--sum",
        help: "ext | dir | +dir",
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
                    Func = (seq) => seq
                        .GroupBy((it) => Helper.GetFirstDir(Path.GetDirectoryName(
                            Helper.io.GetRelativeName(it.FullName))))
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "." : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke((seq) => Sort.Sums(seq))
                        .Invoke(Show.ReverseSum)
                        .Invoke(Show.TakeSum)
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
                    Func = (seq) => seq
                        .GroupBy((it) => it.Extension.ToLower())
                        .Select((grp) => grp.Aggregate(
                            seed: new InfoSum(Name:
                            string.IsNullOrEmpty(grp.Key) ? "*NO-EXT*" : grp.Key),
                            func: (acc, it) => acc.AddWith(it)))
                        .Invoke((seq) => Sort.Sums(seq))
                        .Invoke(Show.ReverseSum)
                        .Invoke(Show.TakeSum)
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
                    Func = (seq) =>
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
                        .Invoke((seq) => Sort.Sums(seq))
                        .Invoke(Show.ReverseSum)
                        .Invoke(Show.TakeSum)
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
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });
}
