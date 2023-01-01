# Open: "x64 Native Tools Command Prompt", and, "x86 Native Tools Command Prompt"
#
# Run: nmake
#
BinDir=../../dir2/runtimes
ObjDir=obj-$(PLATFORM)

DllFile=$(BinDir)/libdir2-$(PLATFORM).dll
objForDll=$(ObjDir)/dir2native.obj

$(DllFile): $(objForDll)
  if not exist $(BinDir) md $(BinDir)
  link /dll /def:dir2native.def /out:$(DllFile) $(objForDll) /MACHINE:$(PLATFORM)

$(ObjDir)/dir2native.obj: ../dir2native.h ../dir2native.c
  if not exist $(ObjDir) md $(ObjDir)
  cl /c ../dir2native.c /Fo:$(ObjDir)/dir2native.obj
