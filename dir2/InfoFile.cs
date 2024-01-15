using System.Runtime.InteropServices;
using System.Text;
using static dir2.MyOptions;

namespace dir2;

public record InfoBase(string Name
    , string FullName
    , DateTimeOffset CreationTime
    , DateTimeOffset LastWriteTime
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

    public bool IsLinked { get => false == String.IsNullOrEmpty(LinkTarget); }
    public bool IsNotLinked { get => String.IsNullOrEmpty(LinkTarget); }
}

public record InfoDir(string Name
    , string Extension
    , string FullName
    , DateTimeOffset CreationTime
    , DateTimeOffset LastWriteTime
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
        , DateTimeOffset.MinValue, DateTimeOffset.MinValue
        , FileAttributes.Directory
        , UnixFileMode: UnixFileMode.None
        , LinkTarget: string.Empty);

    public bool IsFake { get => Object.ReferenceEquals(Fake, this); }
    public bool IsNotFake { get => false == Object.ReferenceEquals(Fake, this); }


    #region Call Enumerator Function Safely
    static readonly IEnumerator<string> EmptyEnumStrings
        = Enumerable.Empty<string>().GetEnumerator();
    static IEnumerator<string> SafeGetFileEnumerator(string dirname)
    {
        try { return Directory.EnumerateFiles(dirname).GetEnumerator(); }
        catch { return EmptyEnumStrings; }
    }

    static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
    {
        try { return Directory.EnumerateDirectories(dirname).GetEnumerator(); }
        catch { return EmptyEnumStrings; }
    }

    static bool SafeMoveNext(IEnumerator<string> it)
    {
        try { return it.MoveNext(); }
        catch { return false; }
    }

    static string SafeGetCurrent(IEnumerator<string> it)
    {
        try { return it.Current; }
        catch { return string.Empty; }
    }
    #endregion

    /// <summary>
    /// Get sub-directories on the directory
    /// </summary>
    /// <returns>Non-empty name and NotFake InfoDir</returns>
    public IEnumerable<InfoDir> GetDirectories()
    {
        var itr = SafeGetDirectoryEnumerator(FullName);
        while (SafeMoveNext(itr))
        {
            var thisFullpath = SafeGetCurrent(itr);
            if (String.IsNullOrEmpty(thisFullpath)) continue;

            var nameThe = Path.GetFileName(thisFullpath);
            if (Wild.ExclDirNameOpt.Invoke(nameThe)) continue;

            var infoCurrent = Helper.ToInfoDir(thisFullpath);
            if (infoCurrent.IsFake) continue;
            if (false == CheckDirLink(infoCurrent)) continue;
            yield return infoCurrent;
        }
    }

    public IEnumerable<string> GetFiles()
    {
        var itr = SafeGetFileEnumerator(FullName);
        while (SafeMoveNext(itr))
        {
            var current = SafeGetCurrent(itr);
            if (String.IsNullOrEmpty(current)) continue;
            yield return current;
        }
    }
}

public record InfoFile(string Name
    , string Extension
    , string FullName
    , string DirectoryName
    , long Length
    , DateTimeOffset CreationTime
    , DateTimeOffset LastWriteTime
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
        , DateTimeOffset.MinValue, DateTimeOffset.MinValue
        , FileAttributes.Normal
        , UnixFileMode: UnixFileMode.None
        , LinkTarget: string.Empty);

    public bool IsFake { get => Object.ReferenceEquals(Fake, this); }
    public bool IsNotFake { get => false == Object.ReferenceEquals(Fake, this); }
}

public class InfoSum
{
    public string Name { get; private set; }

    bool isBase { get; set; } = false;
    string GetName()
    {
        if (isBase)
        {
            var rptTime = Show.ReportTime.Invoke(true);
            if (string.IsNullOrEmpty(rptTime)) return Name;
            return Name + " " + rptTime;
        }
        return Name;
    }

    public int Count { get; private set; } = 0;
    public long Length { get; private set; } = 0L;
    public DateTimeOffset StartTime { get; private set; } = DateTimeOffset.MaxValue;
    public DateTimeOffset EndTime { get; private set; } = DateTimeOffset.MinValue;

    public InfoSum(string Name)
    {
        this.Name = Name;
    }

    public InfoSum(string Name, bool extra)
    {
        this.Name = Name;
        StartTime = new DateTimeOffset(
            year: 1, month: 1, day: 1,
            0, 0, 0, TimeSpan.Zero);
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
        isBase = IsBase;
    }

    static internal readonly InfoSum Fake = new(string.Empty);
    static internal readonly Action<string, string[], InfoSum> DoNothing
        = Helper.DoNothing;

    static public bool IsNothing(Action<InfoSum> check)
    {
        return Object.ReferenceEquals(check, DoNothing);
    }

    public bool IsFake { get => Object.ReferenceEquals(Fake, this); }
    public bool IsNotFake { get => false == Object.ReferenceEquals(Fake, this); }

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
        write(Show.Date(Show.Last.Invoke($"- ")));
        write(Show.Date(Show.Last.Invoke($"{Helper.DateFormatOpt.Invoke(EndTime)} ")));
        write(Show.Count(Show.CountFormat.Invoke(Count)));
        writeLine(GetName());
    }
}

static public partial class Helper
{
    static internal InfoDir ToInfoDir(string dir)
    {
        try
        {
            if (String.IsNullOrEmpty(dir))
            {
                return InfoDir.Fake;
            }

            var rtn = new DirectoryInfo(dir);
            var a2 = rtn.UnixFileMode;
            if (a2.HasFlag(UnixFileMode.GroupExecute))
            {

            }
            var info2 = Show.GetInfoLink(rtn);
            return new InfoDir(Name: rtn.Name
                , Extension: rtn.Extension
                , FullName: rtn.FullName
                , CreationTime: FromUtcToReportTimeZone(info2.CreationTime)
                , LastWriteTime: FromUtcToReportTimeZone(info2.LastWriteTime)
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
            if (String.IsNullOrEmpty(file))
            {
                return InfoFile.Fake;
            }
            var rtn = new FileInfo(file);
            var info2 = Show.GetInfoLink(rtn);
            return new InfoFile(Name: rtn.Name
                , Extension: rtn.Extension
                , FullName: rtn.FullName
                , DirectoryName: rtn.DirectoryName ?? string.Empty
                , Length: Show.GetViewSize(rtn)
                , CreationTime: FromUtcToReportTimeZone(info2.CreationTime)
                , LastWriteTime: FromUtcToReportTimeZone(info2.LastWriteTime)
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
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe)
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
                        throw new ArgumentException($"'{argThe}' is bad value to {argThe}");
                }
            });

    static internal readonly IInovke<InfoFile, bool> IsLinkFileOpt =
        new ParseInvoker<InfoFile, bool>("--link", help: "incl | only",
            init: Always<InfoFile>.True, resolve: (parser, args) =>
            {
                var argThe = Helper.GetUnique(args, parser);
                switch (argThe)
                {
                    case "incl":
                        parser.SetImplementation(Always<InfoFile>.True);
                        break;
                    case "only":
                        parser.SetImplementation((it) => it.IsLinked);
                        break;
                    default:
                        throw new ArgumentException($"'{argThe}' is bad value to {parser.Name}");
                }
            });
}
