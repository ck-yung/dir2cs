# Dir2
**v2.1.1**

## Syntax:
```
dir2 [DIR] [WILD ..] [OPT ..]
```

### Example:
```
dir2 bin -s *.exe *.dll --within 10m
```

## Syntax:
```
dir2 DIR/WILD [OPT ..]
```

### Example:
```
dir2 obj/*.dll -s
```
[Link to complete example](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)

[Link to ```--date-format``` example](https://github.com/ck-yung/dir2cs/blob/main/docs/date-format.md)

### Example as Unix ls:
```
dir2 obj/*.dll --show mode,owner
```
[Link to example](https://github.com/ck-yung/dir2cs/blob/main/Unix-LS)

### Daily Samples

| Command | Description |
| ------- | ----------- |
| ```dir2 -sH```               | To list hidden files in sub-dir excluding in link dir
| ```dir2 -w 13day```          | To list files whose time-stamp is within 13 days ago  (including 13 days ago)
| ```dir2 -W 4day```           | To list files whose time-stamp is before 4 days ago
| ```dir2 -w 16day -W +6day``` | To list files whose time-stamp is within 16 days ago and before 10 days ago
| ```dir2 -w 123m```           | To list files whose size is smaller than 123 Mb (including 123Mb)
| ```dir2 -W 4g```             | To list files whose size is larger than 4 Gb
| ```dir2 -x *.tmp,*.obj```    | To list excluding some files
| ```dir2 -x :link```          | To list excluding link files
| ```dir2 -X .vs,obj```        | To list excluding some dir
| ```dir2 -x :link```          | To list excluding link dir
| ```dir2 -s --sum ext```      | To list in sub-dir and grouping by file extension

## Options:

| Command            | Shortcut | Description                  | Example
| -------            | -------- | -----------                  | ---------------
| --help             | -?       | Short help                   |
| --sub all          | -s       | Show on all sub-dir          |
| --sort *OPT*       | -o       | Sort                         | -o size,ext
| --excl *OPT*       | -x       | Exclude some specified file  | -x \*.dll;\*.lib
| --excl-dir *OPT*   | -X       | Exclude some specified dir   | -X bin;obj;.vs
| --within *OPT*     | -w       | Specify selection condition  | -w 14day
|                    |          |                              | -w 100k
| --not-within *OPT* | -W       | Specify selection condition  | -W 7day
|                    |          |                              | -W +3hour
|                    |          |                              | -W 50k
| --keep-dir         | -k       | Keep given dir name
| |
|                    | -b       | Show filename only

### --within -w
Option '--within' (shortcut '-w') select files as follows.

| Postfix | Example   | Example Description                                    |
| ------- | -------   | -------------------                                    |
| k,m,g   | -w 100k   | Show file if its size is smaller than 100 Kb.          |
|         | -w 10m    | Show file if its size is smaller than 10 Mb.           |
| min     | -w 20min  | Show file if its last write-time is within 10 minutes. |
| hour    | -w 3hour  | Show file if its last write-time is within 3 hours.    |
| day     | -w 14day  | Show file if its last write-time is within 14 days.    |
| | | |
|         | -w 2022-12-25 | Show file if its last write-time is NOT before X'Mas. |
|         | -w 13:30      | Show file if its last write-time is NOT before today 13:30. |
|         | -w 2019-06-12T15:20 |                                              |

### --not-within -W
Option '--not-within' (shortcut '-W' upper case) select files as follows.

| Postfix | Example   | Example Description                                    |
| ------- | -------   | -------------------                                    |
| k,m,g   | -W 90k    | Show file if its size is bigger than 90 Kb.            |
|         | -W 2m     | Show file if its size is bigger than 2 Mb.             |
| min     | -W 15min  | Show file if its last write-time is before 15 minutes. |
| hour    | -W 2hour  | Show file if its last write-time is before 2 hours.    |
| day     | -W 7day   | Show file if its last write-time is before 7 days.     |
| | | |
|         | -W 2022-12-25 | Show file if its last write-time is before X'Mas.  |
|         | -W 13:30      | Show file if its last write-time is before today 13:30. |
|         | -W 2019-06-12T15:20 |                                              |
| | | |
|         | -W +16min | Show file if its last write-time is within 16 min after '--within' option |
|         | -W +3hour | Show file if its last write-time is within 3 hours after '--within' option |
|         | -W +6day | Show file if its last write-time is within 6 days after '--within' option |

### Sum-up by File Extension, or, Dir Name

| Option        | Description |
| ---------     | ----------- |
| --sum ext     | Sum up by file extension (current dir only) |
| -s --sum ext  | Sum up by file extension from all sub dir.  |
| -s --sum dir  | Sum up by dir.                              |
| -s --sum +dir | Sum up by dir including "no file found".    |
| -s --sum year | Sum up by year.                             |

### Sum-up Dir Usage
List all dir which has no any '.cs' file.
```
dir2 *.cs -s --sum +dir -o count --take 1
```

### Provide '--files-from' on pipeline operation
Store '.cs' files from all sub dir on 'my_proj' into a new tar file, or, a new zip file.
```
dir2 my_proj *.cs -bks | tar -cf ..\backup\today.tar --files-from -
dir2 my_proj *.cs -bks | zip2 -cf ..\backup\today.zip -T -
```

List files but excluding any file link or dir link.
```
dir2 my_proj *.cs -bks -X :link -x :link
```

# Complete Option List by -??
### ```--HELP```
| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | --help cfg       | 
| -v       | --version        | 
|          | --dir            | ```both``` ```off``` ```only``` ```only-link``` ```tree```
|          | --sub            | ```off``` ```all```
|          | --cfg-off        |
|          | --utf8           |
| -w       | --within         | DATE or SIZE | ```-w 12m``` ```-w 3day```
| -W       | --not-within     | DATE or SIZE | ```-W 10k``` ```-W 2hour```
|          |                  |              | ```-w 14day -W +7day```
| -x       | --excl           | WILD[,WILD ..]  | ```-x *.tmp,*.temp```    |
|          |                  |                 | ```-x :link```
| -X       | --excl-dir       | WILD[,WILD ..]  | ```-X obj,bin```           |
|          |                  |                 | ```-X :link```
|          | --regex          |
| -c       | --case-sensitive |
|          | --creation-date  |
| -k       | --keep-dir       |
|          | --no-ext         | ```incl``` ```excl``` ```only```
|          | --hidden         | ```incl``` ```excl``` ```only```
|          | --size-format    | INTEGER  ```commna``` ```short``` ```+short```  | ```--size-format 12,comma```
|          |                  |                         | ```--size-format short```
|          | --count-format   | INTEGER  ```commna``` ```short```  | ```--size-format 6,comma```
|          |                  |                         | ```--size-format```
|          | --date-format    | ```short``` ```u+20``` ```yyyy-MMM-ddTHH:mm:ss``` ```yy-MM-dd%20HH:mm``` ```unix```
|          | --creation-date  |
|          | --total          | ```off``` ```only```
|          | --hide           | ```date,size,cout,mode,owner,link```
|          | --show           | ```date,size,cout,mode,owner,link,link-size,link-date```
|          | --link           | ```incl``` ```only```
|          | --excl-none      | [Clear all ```--excl``` and ```--excl-dir```]
| -o       | --sort           | ```off``` ```name,size,date,ext,count,last``` [up to 2 columns] | ```-o date```
| -r       | --reverse        | ```off``` ```on```
|          | --take           | INTEGER  SIZE  | ```--take 10```
|          |                  |                | ```--take 500m```
|          | --total          | ```off``` ```only```
|          | --sum            | ```ext``` ```dir``` ```+dir``` ```year```

## Daily Shortcut:

| Shortcut | Stand for              | Description                    |
| -------- | ---------              | -----------                    |
| -s       | --sub all              | Recursively sub-directory      |
| -f       | --dir off              | List file only                 |
| -d       | --dir only             | List dir name only             |
| -R       | --dir tree             | List dir tree                  |
| -b       | --total off            | List filename (with path) only |
|          | --hide date,size,count,mode,owner,link |                |
| -t       | --total only           | Display total line only        |
| -H       | --excl-none --hidden only --excl :link --exc-dir :link | Proper list hidden files    |

# Setup by Config File & Environment
 
Config file "dir2.opt" will be referred before envir var "dir2".

Environment "dir2" will be referred before command line parameters.

See:
```
dir2 -? cfg
```

[Link to FAQ](https://github.com/ck-yung/dir2cs/blob/main/FAQ.md)

Yung, Chun Kau

<yung.chun.kau@gmail.com>

2023 Nov
