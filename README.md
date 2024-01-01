# Dir2
**v2.1.1.1**

## Syntax:
```
dir2 [DIR/WILD ..] [OPT ..]
```

### Example:
```
dir2 ~/Projects/dir2cs --sub all --excl-dir bin,obj --excl "*.user" --within 3days --not-within 2Mb
```
or
```
dir2 ~\Proj*\dir* -s -X bin,obj -x *.user -w 3days -W 2Mb
```
```
## Complete Help Topics
[Link](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)

### On-line help

| Command | Description |
| ------- | ----------- |
| ```dir2 -?```         | To list common-used options
| ```dir2 -??```        | To list all options
| ```dir2 -? cfg```     | To list options loading by config file ```~\.local\dir2.opt```

### Common used

| Command | Description |
| ------- | ----------- |
| ```dir2 -fo size```          | To list files only and order by file size
| ```dir2 -sw 13days```        | To list all files whose time-stamp is within (including) 13 days ago
| ```dir2 -W 4hours```         | To list files whose time-stamp is before 4 hours ago ('w' in upper case)
| ```dir2 -W 123Mb -w 2Gb```   | To list files whose size is larger than 123 Mb and smaller (including) than 2 Gb
| ```dir2 -x *.tmp,*.obj```    | To list excluding some files
| ```dir2 -X .vs,obj```        | To list excluding some dir ('x' in upper case)
| ```dir2 -sH```               | To list all hidden files (excluding file-link, and file in link-dir)

### Advanced common used

| Command | Description |
| ------- | ----------- |
| ```dir2 -so date --sum dir``` | To list dir-sum and order by the earliest of written dates
| ```dir2 -so last --sum ext``` | To list file-ext-sum and order by the last of written dates
| ```dir2 -R```                 | To list all directories in a tree structure
| ```dir2 -kb Docu*```          | To list filename with the directory name
| ```dir2 -sx :link```          | To list all files excluding file-link
| ```dir2 -sdX :link```         | To list all directories excluding link-dir
| ```dir2 -s --link only```     | To list all file-links
| ```dir2 -s --dir only-link``` | To list all link-dirs

### Provide '--files-from' on pipeline operation
Store '.cs' files from all sub dir on 'my_proj' into a new tar file, or, a new zip file.
```
dir2 my_proj *.cs -bks | tar -cf ..\backup\today.tar --files-from -
dir2 my_proj *.cs -bks | zip2 -cf ..\backup\today.zip -T -
```

### Example as Unix ls:
[Link to "unix-ls" example](https://github.com/ck-yung/dir2cs/blob/main/Unix-LS)
```
dir2 obj/*.dll --show mode,owner

Yung, Chun Kau

<yung.chun.kau@gmail.com>

2024 Jan
