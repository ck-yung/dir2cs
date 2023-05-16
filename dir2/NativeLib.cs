using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace dir2;

static class NativeLib
{
    [DllImport("dir2native.dll")]
    static public extern int Init();

    [DllImport("dir2native.dll",
    CallingConvention = CallingConvention.Cdecl,
    CharSet = CharSet.Ansi)]
    static extern int GetFileOwner(int lenOutput,
    [MarshalAs(UnmanagedType.LPStr)] string pathname,
    [Out] byte[] lpOutOwner);

    static public string FileOwner(string pathname)
    {
        int length = 128;
        byte[] output = ArrayPool<byte>.Shared.Rent(length + 1);
        length = GetFileOwner(length, pathname, output);
        return Encoding.ASCII.GetString(output, 0, length);
    }

    static NativeLib()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(
                Assembly.GetExecutingAssembly(),
                MyDllImportResolver);
        }
        catch (Exception ee)
        {
            Console.Error.WriteLine(ee.Message);
        }
    }

    static bool isInited { get; set; } = false;
    static string LibName { get; set; } = "?";

    static public string LibraryName()
    {
        var tmp = Init() > 0 ? "" : "!";
        return LibName + tmp;
    }

    static IntPtr MyDllImportResolver(string libraryName,
        Assembly assembly, DllImportSearchPath? searchPath)
    {
        var rid = RuntimeInformation.RuntimeIdentifier;
        var aa = rid.Split(new char[] { '-' }, 2);
        if (libraryName == "dir2native.dll")
        {
            if (isInited)
            {
                return NativeLibrary.Load(
                    Path.Join("runtimes", LibName),
                    assembly, searchPath);
            }

            var nameThe = $"libdir2-{aa[1]}.so";
            if (aa[0].StartsWith("win"))
            {
                nameThe = $"libdir2-{aa[1]}.dll";
            }
            else if (aa[0].StartsWith("osx"))
            {
                nameThe = $"libdir2-{aa[1]}.dylib";
            }

            LibName = nameThe;
            isInited = true;

            return NativeLibrary.Load(
                Path.Join("runtimes", nameThe),
                assembly, searchPath);
        }
        return IntPtr.Zero;
    }
}
