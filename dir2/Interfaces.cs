namespace dir2;

public record ParseArg(bool flag, ArgType type, string arg);
public record TypedArg(ArgType type, string arg);

public interface IParse
{
    string Name { get; }
    string Help { get; }
    public IEnumerable<ParseArg> Parse(IEnumerable<ParseArg> args);
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
