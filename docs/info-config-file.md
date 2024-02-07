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


| Option  | Description |
| ------  | --------- |
| --sort  | Ordering  |
| --show  | Selection |
| --hide  | Selection |
| --utf8  | Text Encoding |
| --regex | Regular Expression |
| --case-sensitive | |
| [--color](https://github.com/ck-yung/dir2cs/blob/main/docs/info-color.md) | |
| [--date-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) | |
| [--size-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-size-format.md) | |
| --count-format   | |
| --hidden   | Hidden file selection |
| --reverse  | Ordering |
| --end-time | Time stamp |
| --excl     | Excluding files. Literal ```:link``` to exclude link files.|
| --excl-dir | Excluding directories. Literal ```:link``` to exclude directory links.|

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
