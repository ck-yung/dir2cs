using System.Collections.Generic;
using static dir2.MyOptions;

namespace dir2;

static public partial class MyOptions
{
    static public string[] Parse(IEnumerable<string> args)
    {
        return Parsers.Aggregate(
            seed: args.Select((it) => (false, it)),
            func: (acc, it) => it.Parse(acc))
            .Select((it) => it.Item2).ToArray()!;
    }

    static public ImplicitBool ScanSubDir = new SwitchParser(name:"--sub");

    static public IInovke<string, int> PrintDirOption =
        new ParseInvoker<string, int>(
        name: "--dir", help: "both | only | off",
        init: (path) =>
        {
            Helper.PrintDir(path);
            return Helper.PrintFile(path);
        }, resolve: (parser, args) =>
        {
            var aa = args.Where((it)=>it.Length>0).ToHashSet().ToArray();
            if (aa.Length > 1)
                throw new ArgumentException($"Too many values to {parser.Name}");
            switch (aa[0])
            {
                case "both": // default value
                    break;
                case "only":
                    parser.SetImplementation(Helper.PrintDir);
                    break;
                case "off":
                    parser.SetImplementation(Helper.PrintFile);
                    break;
                default:
                    throw new ArgumentException($"Bad value '{aa[0]}' to {parser.Name}");
            }
        });

    #region Sort
    static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>> SortFile
    { get; private set; } = (seq) => seq;

    static public Func<IEnumerable<InfoDir>, IEnumerable<InfoDir>> SortDir
    { get; private set; } = (seq) => seq;

    static public IParse
        SortFileOpt = new SimpleParser(name: "--sort", help: "name | size | date | ext",
            resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).ToHashSet().ToArray();

                if ((aa.Length > 0) && aa[0] == "name")
                {
                    SortDir = (seq) => seq.OrderBy((it) => it.Name);
                    SortFile = (seq) => seq.OrderBy((it) => it.Name);
                    return;
                }

                if (aa.Length == 1)
                {
                    switch (aa[0])
                    {
                        case "size":
                            SortFile = (seq) => seq.OrderBy((it) => it.Length);
                            break;
                        case "date":
                            SortDir = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                            SortFile = (seq) => seq.OrderBy((it) => it.LastWriteTime);
                            break;
                        case "ext":
                            SortDir = (seq) => seq.OrderBy((it) => it.Extension);
                            SortFile = (seq) => seq.OrderBy((it) => it.Extension);
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
                    };

                    if (qq.TryGetValue((aa[0], aa[1]), out var found))
                    {
                        SortDir = found.Item1;
                        SortFile = found.Item2;
                        return;
                    }
                    throw new ArgumentException($"Bad values ('{aa[0]}', ..) to {parser.Name}");
                }
                else
                {
                    throw new ArgumentException($"Too many values to {parser.Name}");
                }
            });
    #endregion

    static public IParse[] Parsers = new IParse[] 
    {
        (IParse) ScanSubDir,
        (IParse) PrintDirOption,
        (IParse) SortFileOpt,
    };

    /// <summary>
    /// Implicit boolean, default false
    /// </summary>
    private class SwitchParser : ImplicitBool, IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }

        public SwitchParser(string name, string help ="")
        {
            Name = name;
            Help = help;
        }

        public IEnumerable<(bool, string)> Parse(IEnumerable<(bool, string)> args)
        {
            var it = args.GetEnumerator();
            while (it.MoveNext())
            {
                var current = it.Current;
                if (current.Item2 == Name)
                {
                    Flag = true;
                }
                else
                {
                    yield return current;
                }
            }
        }
    }

    internal abstract class Parser: IParse
    {
        public string Name { get; init; }

        public string Help { get; init; }
        public Action<Parser, IEnumerable<string>>
            Resolve { get; init; }

        public Parser(string name, string help,
            Action<Parser, IEnumerable<string>> resolve)
        {
            Name = name;
            Help = help;
            Resolve = resolve;
        }

        public IEnumerable<(bool, string)> Parse(
            IEnumerable<(bool, string)> args)
        {
            IEnumerable<(bool, string)> ToFlagEnum()
            {
                var it = args.GetEnumerator();
                while (it.MoveNext())
                {
                    var current = it.Current;
                    if (current.Item2 != Name)
                    {
                        yield return it.Current;
                    }
                    else
                    {
                        if (!it.MoveNext())
                        {
                            throw new ArgumentException(
                                $"Missing value to {Name}");
                        }
                        yield return (true, it.Current.Item2);
                    }
                }
            }

            var groupThe = ToFlagEnum()
                .GroupBy((it) => it.Item1)
                .ToDictionary((it) => it.Key, (it) => it.AsEnumerable());

            if (groupThe.ContainsKey(true))
            {
                var argsFound = groupThe[true].Select((it) => it.Item2);
                Resolve(this, argsFound);
            }

            if (groupThe.ContainsKey(false)) return groupThe[false];
            return Enumerable.Empty<(bool, string)>();
        }
    }

    private class SimpleParser : Parser
    {
        public SimpleParser(string name,
            Action<Parser, IEnumerable<string>> resolve,
            string help = "") : base(name, help, resolve)
        {
        }
    }

    private class ParseInvoker<T,R>: Parser, IInovke<T,R>
    {
        protected Func<T, R> imp { get; private set; }
        public ParseInvoker(string name, Func<T,R> @init,
            Action<ParseInvoker<T, R>, IEnumerable<string>> resolve,
            string help = ""): base(name, help,
                resolve: (obj, args) =>
                resolve((ParseInvoker<T, R>) obj, args))
        {
            imp = @init;
        }

        public R Invoke(T arg)
        {
            return imp(arg);
        }

        public bool SetImplementation(Func<T, R> impNew)
        {
            if (impNew != null)
            {
                imp = impNew;
                return true;
            }
            return false;
        }
    }
}
