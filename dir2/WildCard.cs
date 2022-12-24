using System.Text.RegularExpressions;
using static dir2.MyOptions;

namespace dir2;

static public class Wild
{
    static public Func<string, Regex> MakeRegex { get; private set; }
        = (it) => new Regex(it, RegexOptions.IgnoreCase);

    static public Func<string, string> ToRegexText { get; private set; } = (it) =>
    {
        var regText = new System.Text.StringBuilder("^");
        regText.Append(it
            .Replace(@"\", @"\\")
            .Replace("^", @"\^")
            .Replace("$", @"\$")
            .Replace(".", @"\.")
            .Replace("?", ".")
            .Replace("*", ".*")
            .Replace("(", @"\(")
            .Replace(")", @"\)")
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("{", @"\{")
            .Replace("}", @"\}")
            ).Append('$');
        return regText.ToString();
    };

    static public Func<string, bool> ToWildMatch(string arg)
    {
        var regThe = MakeRegex(ToRegexText(arg));
        return (it) => regThe.Match(it).Success;
    }

    static public Func<string, bool> CheckIfFileNameMatched
    { get; private set; } = Always<string>.True;
    static public Func<string, bool> CheckIfDirNameMatched
    { get; private set; } = Always<string>.True;

    static public void InitMatchingNames(IEnumerable<string> names,
        bool checkingFilename = true)
    {
        var matchFuncs = names
            .Where((it) => it.Length>0)
            .Distinct()
            .Select((it) => ToWildMatch(it))
            .ToArray();
        if (matchFuncs.Length > 0)
        {
            if (checkingFilename)
            {
                CheckIfFileNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
            }
            else
            {
                CheckIfDirNameMatched = (it) => matchFuncs.Any((chk) => chk(it));
            }
        }
    }

    static public readonly IInovke<string, bool> ExcludeFileOption =
        new ParseInvoker<string, bool>("--excl", help: "FILE[;FILE ..]",
            init: Helper.Never,
            resolve: (parser, args) =>
            {
                var checkFuncs = args
                .Select((it) => it.Split(':',';'))
                .SelectMany((it) => it)
                .Where((it) => it.Length>0)
                .Distinct()
                .Select((it) => ToWildMatch(it))
                .ToArray();
                parser.SetImplementation((arg) => checkFuncs.Any((chk) => chk(arg)));
            });
}
