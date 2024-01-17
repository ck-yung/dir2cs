using static dir2.MyOptions;

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
            .Distinct()
            .Take(2)
            .ToArray();

        if (rtn.Length == 0)
        {
            throw new ArgumentException($"Missing value to '{opt.Name}'");
        }

        if (rtn.Length > 1)
        {
            throw new ArgumentException($"Too many values ({rtn[0]};{rtn[1]}) to '{opt.Name}'");
        }

        if (rtn[0] == ExtraHelp)
        {
            throw new ShowSyntaxException(opt);
        }

        return rtn[0];
    }

    public static string[] GetUniqueTexts(IEnumerable<string> args, int max, IParse opt)
    {
        var rtn = Helper.CommonSplit(args)
            .Take(max+1)
            .ToArray();

        if (rtn.Length == 0)
        {
            throw new ArgumentException($"Missing value to '{opt.Name}'");
        }

        if (rtn.Length > max)
        {
            throw new ArgumentException($"Too many values to '{opt.Name}'");
        }

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
        : base($"Syntax: {parser.Name} {parser.Help}")
    {
    }
}
