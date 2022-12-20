namespace dir2;

public record InfoDir(string Name,
    string Extension,
    string FullName,
    DateTime CreationTime,
    DateTime LastWriteTime,
    string? LinkTarget)
{
    static public readonly InfoDir Fake = new (string.Empty
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
    string? DirectoryName,
    long Length,
    DateTime CreationTime,
    DateTime LastWriteTime,
    string? LinkTarget)
{
    static public readonly InfoFile Fake = new(string.Empty
        , string.Empty, string.Empty, string.Empty, 0
        , DateTime.MinValue, DateTime.MinValue, null);

    public bool IsNotFake()
    {
        return !Object.ReferenceEquals(Fake, this);
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
                LinkTarget: rtn.LinkTarget);
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
                DirectoryName: rtn.DirectoryName,
                Length: rtn.Length,
                CreationTime: rtn.CreationTime,
                LastWriteTime: rtn.LastWriteTime,
                LinkTarget: rtn.LinkTarget);
        }
        catch
        {
            return InfoFile.Fake;
        }
    }
}
