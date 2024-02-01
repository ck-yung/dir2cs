using System.Text;

namespace dir2;
public interface IParse
{
    string Name { get; }
    string Help { get; }
    public IEnumerable<(bool, ArgType, string)> Parse(
        IEnumerable<(bool, ArgType, string)> args);
}

public interface IInovke<T, R>
{
    R Invoke(T arg);
}

public class ImplicitBool
{
    protected bool Flag { get; set; } = false;
    static public implicit operator bool(ImplicitBool the)
    {
        return the.Flag;
    }
}

public partial class Helper
{
    public const string ExtraHelp = "+?";

    public static string GetUnique(IEnumerable<string> args, IParse opt)
    {
        var rtn = args
            .Where((it) => it.Length > 0)
            .Distinct(comparer: StringComparer.InvariantCultureIgnoreCase)
            .Take(2)
            .ToArray();

        if (rtn.Length == 0)
        {
            throw ConfigException.MissingValue(opt.Name);
        }

        if (rtn.Length > 1)
        {
            throw new ConfigException($"Too many values ({rtn[0]};{rtn[1]}) to '{opt.Name}'");
        }

        if (rtn[0] == ExtraHelp)
        {
            throw new ShowSyntaxException(opt);
        }

        return rtn[0];
    }

    public static string[] GetUniqueTexts(IEnumerable<string> args, int max,
        IParse opt, bool ignoreExtraHelp = false)
    {
        var rtn = Helper.CommonSplit(args)
            .Take(max+1)
            .ToArray();

        if (rtn.Length == 0)
        {
            throw ConfigException.MissingValue(opt.Name);
        }

        if (rtn.Length > max)
        {
            throw new ConfigException($"Too many values to '{opt.Name}'");
        }

        if (ignoreExtraHelp) return rtn;

        if (rtn.Any((it) => it == ExtraHelp))
        {
            throw new ShowSyntaxException(opt);
        }

        return rtn;
    }
}

public class ShowSyntaxException: Exception
{
    internal ShowSyntaxException(IParse parser)
        : base(MyOptions.GetHelpText(parser))
    {
    }
}

internal class ConfigException: Exception
{
    public record Info(ArgType Type, string Source, Exception Error);

    public ConfigException(string message) : base(message)
    {
    }

    static readonly IList<Info> Errors = new List<Info>();

    static public void Add(ArgType type, string source, Exception e)
    {
        Errors.Add(new Info(type, source, e));
    }

    static public IEnumerable<Info> GetErrors()
    {
        return Errors;
    }

    static internal ConfigException MissingValue(string name)
    {
        var msg = new StringBuilder();// Shortcut:
        msg.AppendLine($"Missing value to '{name}'");
        var shortcutFound = MyOptions.GetShortCut(name);
        if (string.IsNullOrEmpty(shortcutFound))
        {
            shortcutFound = name;
        }
        msg.Append($"Try 'dir2 {shortcutFound} +?'");
        return new(msg.ToString());
    }
}
