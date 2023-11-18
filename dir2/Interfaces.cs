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

public enum IncludingOption
{
    All,
    Only,
    Included,
    Excluded,
}
