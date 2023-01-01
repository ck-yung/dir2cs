#ifndef _DIR2_NATIVE_
#define _DIR2_NATIVE_
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int Init();

int GetFileOwner(int maxLenOwner, char *szPath, char *szOwner);
#endif

