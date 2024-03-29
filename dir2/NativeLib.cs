using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace dir2;

static class NativeLib
{
    [DllImport("dir2.dll")]
    static public extern int Init();

    [DllImport("dir2.dll",
    CallingConvention = CallingConvention.Cdecl,
    CharSet = CharSet.Ansi)]
    static extern int GetFileOwner(int length,
    [MarshalAs(UnmanagedType.LPStr)] string message,
    [Out] byte[] lpString);

    static public string FileOwner(string message)
    {
        int length = 128;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length + 1);
        length = GetFileOwner(length, message, buffer);
        return Encoding.ASCII.GetString(buffer, 0, length);
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
        var aa = rid.Split(['-'], 2);
        if (libraryName == "dir2.dll")
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
