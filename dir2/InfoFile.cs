using static dir2.MyOptions;

namespace dir2;

public record InfoBase(string Name,
    DateTime CreationTime,
    DateTime LastWriteTime);

public record InfoDir(string Name,
    string Extension,
    string FullName,
    DateTime CreationTime,
    DateTime LastWriteTime,
    string LinkTarget)
    : InfoBase(Name, CreationTime, LastWriteTime)
{
    static internal readonly InfoDir Fake = new (string.Empty
        , string.Empty, string.Empty
        , DateTime.MinValue, DateTime.MinValue, string.Empty);

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
    FileAttributes Attributes,
    string LinkTarget)
    : InfoBase(Name, CreationTime, LastWriteTime)
{
    static internal readonly InfoFile Fake = new(string.Empty
        , string.Empty, string.Empty, string.Empty, 0
        , DateTime.MinValue, DateTime.MinValue
        , FileAttributes.Normal, string.Empty);

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

    public InfoSum(string Name, bool extra)
    {
        this.Name = Name;
        StartTime = new DateTime(year: 1, month: 1, day: 1);
        EndTime= StartTime;
    }

    static internal readonly InfoSum Fake = new(string.Empty);
    static internal readonly Action<InfoSum> DoNothing = Helper.DoNothing;

    static public bool IsNothing(Action<InfoSum> check)
    {
        return Object.ReferenceEquals(check, DoNothing);
    }

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
    }

    public InfoSum AddWith(InfoFile other)
    {
        Count += 1;
        Length += other.Length;
        var dateOther = Show.GetDate(other);
        if (StartTime > dateOther) StartTime = dateOther;
        if (EndTime < dateOther) EndTime = dateOther;
        return this;
    }

    public InfoSum AddWith(InfoSum other)
    {
        Count += other.Count;
        Length += other.Length;
        if (EndTime < other.EndTime) EndTime= other.EndTime;
        if (StartTime > other.StartTime && other.StartTime.Year > 1)
            StartTime = other.StartTime;
        return this;
    }

    public void Print(Action<string> write, Action<string> writeLine)
    {
        write(Show.Size(LengthFormatOpt.Invoke(Length)));
        write(Show.Date($"{Show.DateFormatOpt.Invoke(StartTime)} "));
        write(Show.Date($"- "));
        write(Show.Date($"{Show.DateFormatOpt.Invoke(EndTime)} "));
        write(Show.Count(Show.CountFormat.Invoke(Count)));
        writeLine(Name);
    }
}

static public partial class Helper
{
    static public partial class io
    {
        static public Func<string, InfoDir> ToInfoDir
        { get; private set; } = toInfoDir;

        static public Func<string, InfoFile> ToInfoFile
        { get; private set; } = toInfoFile;

        static public void Init(
            Func<string, InfoDir> ToInfoDir,
            Func<string, InfoFile> ToInfoFile)
        {
            io.ToInfoDir = ToInfoDir;
            io.ToInfoFile = ToInfoFile;
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
                Attributes: rtn.Attributes,
                LinkTarget: rtn.LinkTarget ?? string.Empty);
        }
        catch
        {
            return InfoFile.Fake;
        }
    }

    static private bool IsHidden(this InfoFile arg)
    {
        if (arg.Attributes.HasFlag(FileAttributes.Hidden)) return true;
        if (string.IsNullOrEmpty(arg.Name)) return true;
        if (arg.Name[0] == '.') return true;
        return false;
    }

    static internal readonly IInovke<InfoFile, bool> IsHiddenOpt =
        new ParseInvoker<InfoFile, bool>("--hidden", help: "excl | incl | only",
            init: (it) => false == it.IsHidden(), resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                switch (aa[0])
                {
                    case "excl":
                        parser.SetImplementation((it) => false == it.IsHidden());
                        break;
                    case "incl":
                        parser.SetImplementation(Always<InfoFile>.True);
                        break;
                    case "only":
                        parser.SetImplementation((it) => it.IsHidden());
                        break;
                    default:
                        throw new ArgumentException($"'{aa[0]}' is bad value to {parser.Name}");
                }
            });
}
