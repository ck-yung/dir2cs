/* *** dir2native.c *** */

#ifdef _MSC_VER
/* do nothing */
#else
#include <sys/stat.h>
#include <sys/types.h>
#include <pwd.h>
#include <grp.h>
#endif

#ifdef _OSX_
#include <stdio.h>
#include <string.h>
#include <sys/attr.h>
#include <sys/time.h>
#include <time.h>
#include <unistd.h>
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
    if (0 == pPwd) return 0;
    strncpy(szOwner, pPwd->pw_name, maxLength);
    nowLength = strlen(szOwner);

    pGrp = getgrgid(fStat.st_gid);
    if (0 == pGrp) return nowLength;
    szOwner[nowLength] = '/'; nowLength += 1;
    maxLength = maxRefText - nowLength;

    strncpy(szOwner + nowLength, pGrp->gr_name, maxLength);
    return strlen(szOwner);
#endif
    return 1;
}

#ifdef _OSX_
struct attr_list_date_added {
    u_int32_t length;
    struct timespec date_added;
};

int getFileAddedDate(char* szPath,
    int* year, int* month, int* day,
    int* hour, int* minute, int* second) {
    struct attr_list_date_added data;
    struct attrlist attrList;

    memset(&attrList, 0, sizeof(attrList));
    attrList.bitmapcount = ATTR_BIT_MAP_COUNT;
    attrList.commonattr = ATTR_CMN_ADDEDTIME;

    int result = getattrlist(szPath, &attrList, &data, sizeof(data), 0);

    if (result == 0)
    {
        return 0;
    }

    struct tm *ts;
    char buf[80];
    ts = localtime(&data.date_added.tv_sec);
    *year  = ts->tm_year;
    *month = ts->tm_mon;
    *day   = ts->tm_wday;
    *hour   = ts->tm_hour;
    *second = ts->tm_sec;

    return 1;
}
#endif
/* eof dir2native.c *** */
