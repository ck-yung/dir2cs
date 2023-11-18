# Dir2
**v2.1.0**

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

### Example as Unix ls:
```
dir2 obj/*.dll --show mode,owner
```
[Link to example](https://github.com/ck-yung/dir2cs/blob/main/Unix-LS)


## Options:

| Shortcut | Description                  | Example         |
| -------- | -----------                  | --------------- |
| -?       | Short help                   |                 |
| -s       | Show on all sub-dir          |                 |
| -b       | Show filename only           |                 |
| -o       | Sort                         | -o size,ext     |
| -x       | Exclude some specified file  | -x \*.dll;\*.lib  |
| -X       | Exclude some specified dir   | -X bin;obj;.vs  |
| -w       | Specify selection condition  | -w 14day        |
|          |                              | -w 100k         |
| -W       | Specify selection condition  | -W 7day         |
|          |                              | -W 50k          |
| -k       | Keep specified dir name      |                 |

### --within -w
Option '--within' (shortcut '-w') select files as follows.

| Postfix | Example   | Example Description                                    |
| ------- | -------   | -------------------                                    |
| k,m,g   | -w 100k   | Show file if its size is smaller than 100 Kb.          |
|         | -w 10m    | Show file if its size is smaller than 10 Mb.           |
| min     | -w 20min  | Show file if its last write-time is within 10 minutes. |
| hour    | -w 3hour  | Show file if its last write-time is within 3 hours.    |
| day     | -w 14day  | Show file if its last write-time is within 14 days.    |
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
|         | -W 2022-12-25 | Show file if its last write-time is before X'Mas.  |
|         | -W 13:30      | Show file if its last write-time is before today 13:30. |
|         | -W 2019-06-12T15:20 |                                              |
|         | -W +16min | Show file if its last write-time is within 16 min after '--within' option |
|         | -W +3hour | Show file if its last write-time is within 3 hours after '--within' option |
|         | -W +6day | Show file if its last write-time is within 6 days after '--within' option |

### To exclude some files by '--excl' (shortcut -x)
```
dir2 -x *.obj
dir2 -x *.tmp -x *.user
dir2 -x *.tmp,*.user
```

### To exclude some dir by '--excl-dir' (shortcut -X)
```
dir2 -X .vs
dir2 -X .vs -X obj*
dir2 -X .vs,obj*
```

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

# Complete Option List by -??
### --HELP
| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | --help cfg       | 
| -v       | --version        | 
|          | --dir            | both  off  only  only-link  only-excl-link  tree  tree-excl-link
|          | --sub            | off   all   excl-link
|          | --link-dir       | incl  excl  only
|          | --cfg-off        |
|          | --utf8           |
| -w       | --within         | DATE or SIZE | -w 12m  -w 3day
| -W       | --not-within     | DATE or SIZE | -W 10k  -W 2hour
|          | --regex          |
| -c       | --case-sensitive |
|          | --creation-date  |
| -k       | --keep-dir       |
|          | --no-ext         | incl  excl  only
|          | --hidden         | incl  excl  only
|          | --size-format    | INTEGER  commna  short  | --size-format 12,comma
|          |                  |                         | --size-format short
|          | --count-format   | INTEGER  commna  short  | --size-format 6,comma
|          |                  |                         | --size-format 
|          | --date-format    | u+20, yyyy-MMM-ddTHH:mm:ss, yy-MM-dd%20HH:mm, unix
|          | --total          | off  only
|          | --hide           | date,size,cout,mode,owner,link
|          | --show           | date,size,cout,mode,owner,link,link-size,link-date
| -o       | --sort           | off,name,size,date,ext,count,last | -o date
|          | --take           | INTEGER,SIZE
|          | --sum            | ext  dir  +dir  year
| -x       | --excl           | --excl-file \*.pdb      | -x \*.pdb        |
| -X       | --excl-dir       | --excl-dir obj          | -X obj           |

## Options is renamed
| Old Option      | New Name, and, new short-cut |
| ----------      | -------         |
| --size-within   | --within -w     |
| --date-within   | --within -w     |
| --size-beyond   | --not-within -W |
| --date-beyond   | --not-within -W |


## Daily Shortcut:

| Shortcut | Stand for              | Description                    |
| -------- | ---------              | -----------                    |
| -s       | --dir sub              | Recursively sub-directory      |
| -f       | --dir off              | List file only                 |
| -d       | --dir only             | List dir name only             |
| -R       | --dir tree             | List dir tree                  |
| -c       | --case-sensitive       |                                |
| -b       | --total off            | List filename (with path) only |
|          | --hide date,size,count,mode,owner,link |                |
| -t       | --total only           | Display total line only        |

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
