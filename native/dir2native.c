/* *** dir2native.c *** */

#ifdef _MSC_VER
/* do nothing */
#else
#include <sys/stat.h>
#include <sys/types.h>
#include <pwd.h>
#include <grp.h>
#endif

#include "dir2native.h"

int Init()
{
    int rtn = 1;
#ifdef _MSC_VER
    rtn += 10;
#else
    /* do nothing */
#endif
    return rtn;
}

int GetFileOwner( int maxRefText, char* szPath, char* szOwner)
{
#ifdef _MSC_VER
    /* do nothing */
#else
    int maxLength = 0;
    int nowLength = 0;
    struct group* pGrp = NULL;
    struct passwd* pPwd = NULL;
    struct stat fStat = { 0 };

    if (1 > maxRefText) return 0;
    szOwner[0] = '\0';
    if (stat(szPath, &fStat) < 0) return 0;

    maxLength = maxRefText;
    pPwd = getpwuid(fStat.st_uid);
    strncpy(szOwner, pPwd->pw_name, maxLength);

    nowLength = strlen(szOwner);
    szOwner[nowLength] = '/'; nowLength += 1;

    maxLength = maxRefText - nowLength;
    pGrp = getgrgid(fStat.st_gid);
    strncpy(szOwner + nowLength, pGrp->gr_name, maxLength);
    return strlen(szOwner);
#endif
    return 1;
}
/* eof dir2native.c *** */
