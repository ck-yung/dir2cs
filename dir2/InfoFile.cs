namespace dir2;

public record InfoDir(string Name,
    string Extension,
    string FullName,
    DateTime CreationTime,
    DateTime LastWriteTime,
    string LinkTarget)
{
    static internal readonly InfoDir Fake = new (string.Empty
        , string.Empty, string.Empty
        , DateTime.MinValue, DateTime.MinValue, null);

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
    }
}

public record InfoFile(string Name,
    string Extension,
    string FullName,
    string DirectoryName,
    long Length,
    DateTime CreationTime,
    DateTime LastWriteTime,
    string LinkTarget)
{
    static internal readonly InfoFile Fake = new(string.Empty
        , string.Empty, string.Empty, string.Empty, 0
        , DateTime.MinValue, DateTime.MinValue, null);

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
    }
}

public class InfoSum
{
    public string Name { get; private set; }
    public int Count { get; private set; } = 0;
    public long Length { get; private set; } = 0L;
    public DateTime StartTime { get; private set; } = DateTime.MaxValue;
    public DateTime EndTime { get; private set; } = DateTime.MinValue;
    public InfoSum(string Name)
    {
        this.Name = Name;
    }

    static internal readonly InfoSum Fake = new(string.Empty);

    static public readonly Action<InfoSum> DoNothing = (_) => { };
    static public bool IsNothing(Action<InfoSum> check)
    {
        return !Object.ReferenceEquals(check, DoNothing);
    }

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
    }

    public InfoSum AddWith(InfoFile other)
    {
        Count += 1;
        Length += other.Length;
        if (StartTime > other.LastWriteTime) StartTime = other.LastWriteTime;
        if (EndTime < other.LastWriteTime) EndTime = other.LastWriteTime;
        return this;
    }

    public InfoSum AddWith(InfoSum other)
    {
        Count += other.Count;
        Length += other.Length;
        if (StartTime > other.StartTime) StartTime = other.StartTime;
        if (EndTime < other.EndTime) EndTime= other.EndTime;
        return this;
    }

    public void Print(Action<string> write, Action<string> writeLine)
    {
        write(Show.Size($"{MyOptions.LengthFormat.Invoke(Length)} "));
        write(Show.Date($"{MyOptions.DateFormat.Invoke(StartTime)} "));
        write(Show.Date($"{MyOptions.DateFormat.Invoke(EndTime)} "));
        write(Show.Count($"{Count,4} "));
        writeLine(Name);
    }
}

static public partial class Helper
{
    static public partial class System
    {
        static public Func<string, InfoDir> ToInfoDir
        { get; private set; } = toInfoDir;

        static public Func<string, InfoFile> ToInfoFile
        { get; private set; } = toInfoFile;

        static public void Init(
            Func<string, InfoDir> ToInfoDir,
            Func<string, InfoFile> ToInfoFile)
        {
            System.ToInfoDir = ToInfoDir;
            System.ToInfoFile = ToInfoFile;
        }
    }

    static InfoDir toInfoDir(string dir)
    {
        try
        {
            var rtn = new DirectoryInfo(dir);
            return new InfoDir(Name: rtn.Name,
                Extension: rtn.Extension,
                FullName: rtn.FullName,
                CreationTime: rtn.CreationTime,
                LastWriteTime: rtn.LastWriteTime,
                LinkTarget: rtn.LinkTarget ?? string.Empty);
        }
        catch
        {
            return InfoDir.Fake;
        }
    }

    static InfoFile toInfoFile(string file)
    {
        try
        {
            var rtn = new FileInfo(file);
            return new InfoFile(Name: rtn.Name,
                Extension: rtn.Extension,
                FullName: rtn.FullName,
                DirectoryName: rtn.DirectoryName ?? string.Empty,
                Length: rtn.Length,
                CreationTime: rtn.CreationTime,
                LastWriteTime: rtn.LastWriteTime,
                LinkTarget: rtn.LinkTarget ?? string.Empty);
        }
        catch
        {
            return InfoFile.Fake;
        }
    }
}
