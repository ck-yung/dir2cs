namespace dir2;
public interface IParse
{
    string Name { get; }
    string Help { get; }
    public IEnumerable<(bool, string)> Parse(
        IEnumerable<(bool, string)> args);
}

public interface IInovke<T, R>
{
    R Invoke(T arg);
}

public interface IParseInvoke<T, R> : IInovke<T, R>, IParse
{
    Func<T, R>? ParseValues(IEnumerable<string> args);
}

public class ImplicitBool
{
    protected bool Flag { get; set; } = false;
    static public implicit operator bool(ImplicitBool the)
    {
        return the.Flag;
    }
}
