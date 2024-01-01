# Inforamton of Configuration File ```dir2.opt```

## Sample Configuration File

```
--utf8
--date-format short
--size-format +short
--excl .*
--excl-dir .*
```

## Configuration File
* Options will be loaded from ```%USERPROFILE%\.local\dir2.opt``` or ```~/.local/dir2.opt``` before parsing command line.
* But the file will be skipped if ```--cfg-off``` is found in Envir ```dir``` or Command-Lin Options.
* Run command ```dir2 -? cfg``` to find your personal configuration file.
* Short-cut of option is **NOT** loaded from the configuration file.
* The configuration file contains the following options only.

| Option | Description |
| --- | ---- |
| --sort | Ordering |
| --show | Column selection |
| --hide | Column selection |
| --utf8 | Text Encoding |
| --regex | Regular Expression |
| --case-sensitive | |
| --date-format | |
| --size-format | |
| --count-format | |
| --hidden | Hidden file selection |
| --reverse | Ordering |
| --excl | Excluding files |
| --excl-dir | Excluding directories |
