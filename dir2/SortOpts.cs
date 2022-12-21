using System.Collections.Immutable;

namespace dir2;

static public partial class MyOptions
{
    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> SortFiles
    { get; private set; } = (seq) => seq;

    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> SortDirs
    { get; private set; } = (seq) => seq;

    static public readonly IParse SortOptions = new SimpleParser(name: "--sort",
        help: "name | size | date | ext",
        resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).ToHashSet().ToArray();

            if ((aa.Length > 0) && aa[0] == "name")
            {
                SortDirs = (seq) => seq.OrderBy((it) => it.Name);
                SortFiles = (seq) => seq.OrderBy((it) => it.Name);
                return;
            }

            if (aa.Length == 1)
            {
                switch (aa[0])
                {
                    case "size":
                        SortFiles = (seq) => seq.OrderBy((it) => it.Length);
                        break;
                    case "date":
                        SortDirs = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                        SortFiles = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                        break;
                    case "ext":
                        SortDirs = (seq) => seq.OrderBy((it) => it.Extension);
                        SortFiles = (seq) => seq.OrderBy((it) => it.Extension);
                        break;
                    default:
                        throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
                }
            }
            else if (aa.Length == 2)
            {
                var qq = new Dictionary<(string, string), (
                    Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>>,
                    Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>>)>
                {
                    [("size", "name")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.Name),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Length)
                        .ThenBy((it) => it.Name)),
                    [("size", "date")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.LastWriteTime),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Length)
                        .ThenBy((it) => it.LastWriteTime)),
                    [("size", "ext")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.Extension),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Length)
                        .ThenBy((it) => it.Extension)),

                    [("date", "name")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.LastWriteTime)
                        .ThenBy((it) => it.Name),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.LastWriteTime)
                        .ThenBy((it) => it.Name)),
                    [("date", "size")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.LastWriteTime),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.LastWriteTime)
                        .ThenBy((it) => it.Length)),
                    [("date", "ext")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.LastWriteTime)
                        .ThenBy((it) => it.Extension),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.LastWriteTime)
                        .ThenBy((it) => it.Extension)),

                    [("ext", "name")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.Extension)
                        .ThenBy((it) => it.Name),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Extension)
                        .ThenBy((it) => it.Name)),
                    [("ext", "size")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.Extension),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Extension)
                        .ThenBy((it) => it.Length)),
                    [("ext", "date")] = (
                    (IEnumerable<InfoDir> seq) => seq
                        .OrderBy((it) => it.Extension)
                        .ThenBy((it) => it.LastWriteTime),
                    (IEnumerable<InfoFile> seq) => seq
                        .OrderBy((it) => it.Extension)
                        .ThenBy((it) => it.LastWriteTime)),
                }.ToImmutableDictionary();

                if (qq.TryGetValue((aa[0], aa[1]), out var found))
                {
                    SortDirs = found.Item1;
                    SortFiles = found.Item2;
                    return;
                }
                throw new ArgumentException($"Bad values ('{aa[0]}', ..) to {parser.Name}");
            }
            else
            {
                throw new ArgumentException($"Too many values to {parser.Name}");
            }
        });
}