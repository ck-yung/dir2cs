armasm64.exe -nologo libdir2-arm64.asm libdir2-arm64.obj
if not exist ..\..\dir2\nul: md ..\..\dir2
if not exist ..\..\dir2\runtimes\nul: md ..\..\dir2\runtimes
link.exe /nologo /subsystem:windows /dll /def:libdir2.def libdir2-arm64.obj /out:..\..\dir2\runtimes\libdir2-arm64.dll /machine:arm64
