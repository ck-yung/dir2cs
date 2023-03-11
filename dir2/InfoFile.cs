using System.Runtime.InteropServices;
using System.Text;
using static dir2.MyOptions;

namespace dir2;

public record InfoBase(string Name
    , string FullName
    , DateTime CreationTime
    , DateTime LastWriteTime
    , FileAttributes FileAttributes
    , UnixFileMode UnixFileMode
    , string LinkTarget
    )
{
    public string AttributeText()
    {
        var rtn = new StringBuilder();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.ReadOnly)
                ? "R" : " ");
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.Hidden)
                ? "H":" ");
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.System)
                ? "S" : " ");
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.Archive)
                ? "A" : " ");
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.Normal)
                ? "N" : " ");
        }
        else
        {
            rtn.Append(
                FileAttributes.HasFlag(FileAttributes.Directory)
                ? "d" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.UserRead)
                ? "r" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.UserWrite)
                ? "w" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.UserExecute)
                ? "x" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.GroupRead)
                ? "r" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.GroupWrite)
                ? "w" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.GroupExecute)
                ? "x" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.OtherRead)
                ? "r" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.OtherWrite)
                ? "w" : "-");
            rtn.Append(
                UnixFileMode.HasFlag(UnixFileMode.OtherExecute)
                ? "x" : "-");
        }
        rtn.Append(" ");
        return rtn.ToString();
    }

    public string OwnerText()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (FileAttributes.HasFlag(FileAttributes.Directory))
                {
                    return new DirectoryInfo(FullName)
                        .GetAccessControl().GetOwner(
                        typeof(System.Security.Principal.NTAccount))
                        .ToString();
                }
                else
                {
                    return new FileInfo(FullName)
                        .GetAccessControl().GetOwner(
                        typeof(System.Security.Principal.NTAccount))
                        .ToString();
                }
            }
            else
            {
                return NativeLib.FileOwner(FullName);
            }
        }
        catch
        {
            return "? ";
        }
    }
}

public record InfoDir(string Name
    , string Extension
    , string FullName
    , DateTime CreationTime
    , DateTime LastWriteTime
    , FileAttributes FileAttributes
    , UnixFileMode UnixFileMode
    , string LinkTarget
    ) : InfoBase(Name, FullName
        , CreationTime, LastWriteTime
        , FileAttributes
        , UnixFileMode
        , LinkTarget
        )
{
    static internal readonly InfoDir Fake = new (string.Empty
        , string.Empty, string.Empty
        , DateTime.MinValue, DateTime.MinValue
        , FileAttributes.Directory
        , UnixFileMode: UnixFileMode.None
        , LinkTarget: string.Empty);

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
    }
}

public record InfoFile(string Name
    , string Extension
    , string FullName
    , string DirectoryName
    , long Length
    , DateTime CreationTime
    , DateTime LastWriteTime
    , FileAttributes FileAttributes
    , UnixFileMode UnixFileMode
    , string LinkTarget
    ) : InfoBase(Name, FullName
        , CreationTime, LastWriteTime
        , FileAttributes, UnixFileMode
        , LinkTarget)
{
    static internal readonly InfoFile Fake = new(string.Empty
        , string.Empty, string.Empty, string.Empty, 0
        , DateTime.MinValue, DateTime.MinValue
        , FileAttributes.Normal
        , UnixFileMode: UnixFileMode.None
        , LinkTarget: string.Empty);

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

    public InfoSum(bool IsBase)
    {
        Name = Helper.io.RealInitPath;
        if (Name == ("." + Path.DirectorySeparatorChar))
        {
            var a2 = Directory.GetCurrentDirectory();
            a2 = Path.GetFileName(a2);
            if (!string.IsNullOrEmpty(a2))
            {
                Name = a2;
            }
        }
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
        write(Show.Size(Show.LengthFormatOpt.Invoke(Length)));
        write(Show.Date($"{Helper.DateFormatOpt.Invoke(StartTime)} "));
        write(Show.Date($"- "));
        write(Show.Date($"{Helper.DateFormatOpt.Invoke(EndTime)} "));
        write(Show.Count(Show.CountFormat.Invoke(Count)));
        writeLine(Name);
    }
}

static public partial class Helper
{
    static internal InfoDir ToInfoDir(string dir)
    {
        try
        {
            var rtn = new DirectoryInfo(dir);
            var a2 = rtn.UnixFileMode;
            if (a2.HasFlag(UnixFileMode.GroupExecute))
            {

            }
            return new InfoDir(Name: rtn.Name
                , Extension: rtn.Extension
                , FullName: rtn.FullName
                , CreationTime: rtn.CreationTime
                , LastWriteTime: rtn.LastWriteTime
                , FileAttributes: rtn.Attributes
                , UnixFileMode: rtn.UnixFileMode
                , LinkTarget: rtn.LinkTarget ?? string.Empty
                );
        }
        catch
        {
            return InfoDir.Fake;
        }
    }

    static internal InfoFile ToInfoFile(string file)
    {
        try
        {
            var rtn = new FileInfo(file);
            return new InfoFile(Name: rtn.Name
                , Extension: rtn.Extension
                , FullName: rtn.FullName
                , DirectoryName: rtn.DirectoryName ?? string.Empty
                , Length: rtn.Length
                , CreationTime: rtn.CreationTime
                , LastWriteTime: rtn.LastWriteTime
                , FileAttributes: rtn.Attributes
                , UnixFileMode: rtn.UnixFileMode
                , LinkTarget: rtn.LinkTarget ?? string.Empty
                );
        }
        catch
        {
            return InfoFile.Fake;
        }
    }

    static private bool IsHidden(this InfoFile arg)
    {
        if (arg.FileAttributes.HasFlag(FileAttributes.Hidden))
            return true;
        if (string.IsNullOrEmpty(arg.Name)) return true;
        if (arg.Name[0] == '.') return true;
        return false;
    }

    static internal readonly IInovke<InfoFile, bool> IsHiddenFileOpt =
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

    static internal readonly IInovke<InfoFile, bool> IsLinkFileOpt =
        new ParseInvoker<InfoFile, bool>("--link", help: "incl | excl | only",
            init: Always<InfoFile>.True, resolve: (parser, args) =>
            {
                var aa = args.Where((it) => it.Length > 0).Distinct().Take(2).ToArray();
                if (aa.Length > 1)
                    throw new ArgumentException($"Too many values to {parser.Name}");
                switch (aa[0])
                {
                    case "incl":
                        parser.SetImplementation(Always<InfoFile>.True);
                        break;
                    case "excl":
                        parser.SetImplementation((it) => string.IsNullOrEmpty(it.LinkTarget));
                        break;
                    case "only":
                        parser.SetImplementation((it) => false == string.IsNullOrEmpty(it.LinkTarget));
                        break;
                    default:
                        throw new ArgumentException($"'{aa[0]}' is bad value to {parser.Name}");
                }
            });
}
