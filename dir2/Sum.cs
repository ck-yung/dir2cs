namespace dir2;

static public class Sum
{
    static public Func<IEnumerable<InfoFile>, string, InfoSum> Func
    { get; private set; } = (seq, path) => seq.Aggregate(
        seed: new InfoSum(Helper.GetLastDir(path)),
        func: (acc, it) => acc.AddWith(it));

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--sum",
        help: "dir | ext",
        resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).ToHashSet().Take(2).ToArray();
            if (aa.Length>1) throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "dir":
                    break;
                case "ext":
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });
}
