# Dir2
**v2.1.2.0**

## Syntax:
```
dir2 [DIR/WILD ..] [OPT ..]
```

### Features

* Version 2.1.2 will be released on 2024 February.

* Performs MSDOS DIR and UNIX LS command.

* Group by: File-Ext, or, Group by Dir

* Filter on: Date, Size, Symbolic-Link

* Column Selection: File Attribute, File Owner, File Size, Last Written Date, Creation Date, Date of Target Link, Size of Target Link

* Custom Display Format: Size and Date-Time

* Color: Column, Total Line

### Example:
```
dir2 ~/Projects/dir2cs --sub all --excl-dir bin,obj --excl "*.user" --within 3days --not-within 2Mb
```
or
```
dir2 ~\Proj*\dir* -s -X bin,obj -x *.user -w 3days -W 2Mb --color green
```

### On-line help

| Command           | Description |
| -------           | ----------- |
| ```dir2 -?```     | To list common-used options
| ```dir2 -??```    | [To list all options](https://github.com/ck-yung/dir2cs/blob/main/docs/info-options.md)
| ```dir2 -? cfg``` | To list options loading by config file [```dir2-cfg.txt```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-config-file.md)
| ```dir2 -? -```   | To list short-cut options
| ```dir2 -? +```   | To list options support quick-help ```+?```

### [Quick help](https://github.com/ck-yung/dir2cs/blob/main/docs/info-short-help.md) to an option

| Sample Command | Sample Ouput |
| -------------  | ------------ |
| ```dir2 --sum +?``` | ```--sum ext | dir | +dir | year``` |
| ```dir2 -w size```  | ```--within 123kb``` |
|                     | ```--wihtin 123mb``` |

### Common used

| Command | Description |
| ------- | ----------- |
| ```dir2 -fo size```          | To list files only and order by file size
| ```dir2 -sw 13days```        | To list all files whose time-stamp is [within (including) 13 days ago](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md).
| ```dir2 -W 4hours```         | To list files whose time-stamp is [before 4 hours ago](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) ('w' in upper case).
| ```dir2 -W 123Mb -w 2Gb```   | To list files whose size is [larger than 123 Mb and smaller (including) than 2 Gb](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md).
| ```dir2 -x *.tmp,*.obj```    | To list excluding some files
| ```dir2 -X .vs,obj```        | To list excluding some dir ('x' in upper case)
| ```dir2 -sH```               | To list all hidden files (excluding file-link, and file in link-dir)

### Advanced common used

| Command | Description |
| ------- | ----------- |
| ```dir2 -so date --sum dir``` | To list [dir-sum](https://github.com/ck-yung/dir2cs/blob/main/docs/info-sum.md) and order by the earliest of written dates
| ```dir2 -so last --sum ext``` | To list [ext-sum](https://github.com/ck-yung/dir2cs/blob/main/docs/info-sum.md) and order by the last of written dates
| ```dir2 -R```                 | To list all directories in a tree structure
| ```dir2 -kb Docu*```          | To list filename with the directory name
| ```dir2 -sx :link```          | To list all files excluding [file-link](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md)
| ```dir2 -sdX :link```         | To list all directories excluding [link-dir](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md)
| ```dir2 -s --link only```     | To list all [file-links](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md)
| ```dir2 -s --dir only-link``` | To list all [link-dirs](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md)

### Provide '--files-from' on pipeline operation
Store '.cs' files from all sub dir on 'my_proj' into a new tar file, or, a new zip file.
```
dir2 my_proj *.cs -bks | tar -cf ..\backup\today.tar --files-from -
dir2 my_proj *.cs -bks | zip2 -cf ..\backup\today.zip -T -
```

Yung, Chun Kau

<yung.chun.kau@gmail.com>

2024 Feb
