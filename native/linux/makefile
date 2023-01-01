# Synax: make PLATFORM=x64
# Synax: make PLATFORM=arm64

BinDir=../../dir2/runtimes
ObjDir=obj-$(PLATFORM)

DllFile=$(BinDir)/libdir2-$(PLATFORM).so
objForDll=$(ObjDir)/dir2native.o

$(DllFile): $(objForDll)
	@mkdir -p $(BinDir)
	gcc -shared -o $(DllFile) $(objForDll)

$(ObjDir)/dir2native.o: ../dir2native.h ../dir2native.c
	@mkdir -p $(ObjDir)
	gcc -fPIC -c ../dir2native.c -o $(ObjDir)/dir2native.o

