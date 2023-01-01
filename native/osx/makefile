# Synax: make PLATFORM=x64
# Synax: make PLATFORM=arm64

BinDir=../../dir2/runtimes
ObjDir=obj-$(PLATFORM)

DllFile=$(BinDir)/libdir2-$(PLATFORM).dylib

objForDll=$(ObjDir)/dir2native.o

$(DllFile): $(objForDll)
	@mkdir -p $(BinDir)
	gcc -dynamiclib -o $(DllFile) $(objForDll)

$(ObjDir)/dir2native.o: ../dir2native.h ../dir2native.c
	@mkdir -p $(ObjDir)
	gcc -c ../dir2native.c -o $(ObjDir)/dir2native.o

