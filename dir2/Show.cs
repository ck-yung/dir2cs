using System.Collections.Immutable;

namespace dir2;

static internal class Show
{
    static readonly Func<string, string> itself = (arg) => arg;
    static readonly Func<string, string> blank = (arg) => string.Empty;
    static public Func<string, string> GetDirName
    { get; private set; } = itself;

    static public Func<string, string> Date { get; private set; } = itself;
    static public Func<string, string> Size { get; private set; } = itself;
    static public Func<string, string> Count { get; private set; } = itself;

    static public readonly IParse Options = new MyOptions.SimpleParser(name: "--hide",
        help: "date  size  count",
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
}
