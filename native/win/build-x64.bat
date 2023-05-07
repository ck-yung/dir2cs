ml64.exe /nologo /Fo libdir2-x64.obj /c libdir2-x64.asm
if not exist ..\..\dir2\nul: md ..\..\dir2
if not exist ..\..\dir2\runtimes\nul: md ..\..\dir2\runtimes
link.exe /nologo /subsystem:windows /dll /def:libdir2.def libdir2-x64.obj /out:..\..\dir2\runtimes\libdir2-x64.dll /machine:x64
