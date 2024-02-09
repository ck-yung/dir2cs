# Inforamton of Configuration File ```dir2-cfg.txt```

* Old config file ```dir2.opt``` will be loaded if ```dir2-cfg.txt``` is NOT found.
* But ```dir2.opt``` will be skipped in coming release.

## Sample Content of Configuration File

```
--utf8
--date-format short
--size-format +short
--excl .*
--excl-dir .*
```

## Configuration File
* Options will be loaded from ```%USERPROFILE%\.local\dir2-cfg.txt``` or ```~/.local/dir2-cfg.txt``` before parsing command line.
* But the file will be skipped if ```--cfg-off``` is found in Envir ```dir2``` or Command-Lin Options.
* Run command ```dir2 -? cfg``` to find your personal configuration file.
* Short-cut of option is **NOT** loaded from the configuration file.
* The configuration file contains the following options only.
* Time zone in configuration file: ```--date-format utc+```*hh:mm*
    * For exampe, ```--date-format utc+08:00```


| Option  | Available Value | Example | Description |
| ------  | --------------- | ------- | ----------- |
| --sort  | ```off``` ```name,size,date,ext,count,last``` | ```--sort count,size```| Ordering [up to 2 columns] |
| --show  | ```date,size,cout,mode,owner,last,link,link-size,link-date``` | ```--show mode,owner``` | Column Selection |
| --hide  | ```date,size,cout,mode,owner,last,link``` | ```--hide size,link``` | Column Selection |
| --utf8  | | ```--utf8``` | Text Encoding |
| --regex | | ```--regex``` | Regular Expression |
| --case-sensitive | | ```--case-sensitive``` |
| [--color](https://github.com/ck-yung/dir2cs/blob/main/docs/info-color.md) | COLOR[,INTEGER,COLOR-OF-TOTAL-LINE] | ```--color green,10,cyan``` | Color Selection |
| [--date-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) |```short```｜```unix```｜```unix+```｜FORMAT ｜```utc```OFFSET | ```--date-format yyyy-MM-ddTHH:mm:ss``` | Date Format
|         | | ```--date-format yy-MM-dd%20HH:mm ``` | [See also](https://github.com/ck-yung/dir2cs/blob/main/docs/info-encode-char.md)
|         | | ```--date-format utc+8 ``` | Time zone
| [--size-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-size-format.md) | INTEGER[,```commna```]｜```short```｜```+short```  | ```--size-format 12,comma``` | Size Format
|                  |                         | ```--size-format short```
| --count-format   | INTEGER[,```commna```]｜```short``` | ```--size-format 12,comma``` | Count Format
| --hidden         | ```incl```｜```excl```｜```only``` | ```--hidden excl``` | Hidden file selection |
| --reverse        | ```off```｜```on``` | ```--reverse off``` | Ordering |
| [--end-time](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) | FORMAT | ```--end-time Done%20yyyy-MM-ddTHH:mm```  | Time Marking
| --excl           | WILD[,WILD ..] and [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md) | ```--excl *.tmp,*.temp,:link``` | Wild Cards of Excluding Files
| --excl-dir       | WILD[,WILD ..] and [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md) | ```--excl-dir obj,bin,:link``` | Wild Cards of Excluding Files

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
