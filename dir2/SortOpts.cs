using System.Collections.Immutable;
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
        makeException(string badValue, string name)
    {
        throw new ArgumentException($"Bad values ('{badValue}', ..) to {name}");
    }

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--sort",
        help: "name | size | date | ext | count | last",
        resolve: (parser, args) =>
        {
            var aa = args.Where((it) => it.Length > 0).ToHashSet().ToArray();


            if ((aa.Length > 0) && aa[0] == "name")
            {
                Dirs = (seq) => seq.OrderBy((it) => it.Name);
                Files = (seq) => seq.OrderBy((it) => it.Name);
                Sums = (seq) => seq.OrderBy((it) => it.Name);
                return;
            }

            if (aa.Length == 1)
            {
                switch (aa[0])
                {
                    case "size":
                        Files = (seq) => seq.OrderBy((it) => it.Length);
                        Sums = (seq) => seq.OrderBy(it => it.Length);
                        break;
                    case "date":
                        Dirs = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                        Files = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                        Sums = (seq) => seq.OrderBy(it => it.StartTime);
                        break;
                    case "ext":
                        Dirs = (seq) => seq.OrderBy((it) => it.Extension);
                        Files = (seq) => seq.OrderBy((it) => it.Extension);
                        Sums = (seq) => seq.OrderBy((it) => Path.GetExtension(it.Name));
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
                    (seq) => seq.OrderBy((it) => it.Name),
                    (seq2) => seq2.OrderBy((it) => it.Length).ThenBy((it) => it.Name),
                    (seq3) => seq3.OrderBy((it) => it.Length).ThenBy((it) => it.Name)),

                    ("size", "date") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.Length).ThenBy((it) => it.LastWriteTime),
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
                    (seq) => seq.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Name),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Name),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Name)),

                    ("date", "size") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Length)),

                    ("date", "ext") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("date", "count") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.Count)),

                    ("date", "last") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.StartTime).ThenBy((it) => it.EndTime)),

                    ("ext", "name") => (
                    (seq) => seq.OrderBy((it) => it.Extension).ThenBy((it) => it.Name),
                    (seq2) => seq2.OrderBy((it) => it.Extension).ThenBy((it) => it.Name),
                    (seq3) => seq3.OrderBy((it) => Path.GetExtension(it.Name)).ThenBy((it) => it.Name)),

                    ("ext", "date") => (
                    (seq) => seq.OrderBy((it) => it.Extension).ThenBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.Extension).ThenBy((it) => it.LastWriteTime),
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
                    (seq) => seq.OrderBy((it) => it.Name),
                    (seq2) => seq2.OrderBy((it) => it.Name),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.Name)),

                    ("count", "size") => (
                    (seq) => seq,
                    (seq2) => seq2.OrderBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.Length)),

                    ("count", "date") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.StartTime)),

                    ("count", "ext") => (
                    (seq) => seq.OrderBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("count", "last") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.Count).ThenBy((it) => it.EndTime)),

                    ("last", "name") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Name),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Name),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Name)),

                    ("last", "size") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Length),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Length)),

                    ("last", "ext") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Extension),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime).ThenBy((it) => it.Extension),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => Path.GetExtension(it.Name))),

                    ("last", "count") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.Count)),

                    ("last", "date") => (
                    (seq) => seq.OrderBy((it) => it.LastWriteTime),
                    (seq2) => seq2.OrderBy((it) => it.LastWriteTime),
                    (seq3) => seq3.OrderBy((it) => it.EndTime).ThenBy((it) => it.StartTime)),

                    _ => makeException(aa[0], name: parser.Name)
                };
            }
            else
            {
                throw new ArgumentException($"Too many values to {parser.Name}");
            }
        });
}
