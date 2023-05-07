@rem *** SET PATH=%PATH%;c:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.35.32215\bin\Hostx86\x86

ml.exe /nologo /Fo libdir2-x86.obj /coff /c libdir2-x86.asm
if not exist ..\..\dir2\nul: md ..\..\dir2
if not exist ..\..\dir2\runtimes\nul: md ..\..\dir2\runtimes
link.exe /nologo /subsystem:windows /dll /def:libdir2.def libdir2-x86.obj /out:..\..\dir2\runtimes\libdir2-x86.dll /machine:x86
