namespace dir2;

static public class Sum
{
    static public bool IsFuncChanged { get; private set; } = false;
    static private Func<IEnumerable<InfoFile>, string, InfoSum>
        _func = (seq, path) => seq
        .Invoke(Sort.Files)
        .Invoke(Show.ReverseInfo)
        .Invoke(Show.TakeInfo)
        .Select((it) =>
        {
            Helper.ItemWrite(Show.Size($"{MyOptions.LengthFormat.Invoke(it.Length)} "));
            Helper.ItemWrite(Show.Date($"{MyOptions.DateFormat.Invoke(it.LastWriteTime)} "));
            Helper.ItemWriteLine(Helper.io.GetRelativeName(it.FullName));
            return it;
        })
        .Aggregate(
            seed: new InfoSum(Helper.io.RealInitPath),
            func: (acc, it) => acc.AddWith(it));

    static public Func<IEnumerable<InfoFile>, string, InfoSum> Func
    {
        get => _func;
        private set
        {
            _func = value;
            IsFuncChanged = true;
        }
    }

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--sum",
        help: "dir | ext",
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
                    Func = (seq, path) => seq
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
                            func: (acc, it) => acc.AddWith(it))
                        ;
                    break;
                case "ext":
                    Helper.PrintDir = (_) => InfoSum.Fake;
                    Func = (seq, path) => seq
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
                            func: (acc, it) => acc.AddWith(it))
                        ;
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });
}
