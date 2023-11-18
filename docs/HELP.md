# On-line Help of DIR2

## Daily Examples

| Command | Description |
| ------- | ----------- |
| ```dir2 -s``` | Scan all directories |
| ```dir2 -sX bin,obj``` | Scan but excluding some directories |
| ```dir2 -sx *.tmp,*.temp``` | Scan but excluding some files |
| ```dir2 -sb``` | Scan for file name only |
| | |
| ```dir2 -sw 7day -W 1day``` | Scan for file within 7 days but not yesterday |
| ```dir2 -sw 7day -W 24hour``` | |
| ```dir2 -sw 7day -W +6day``` | |
| | |
| ```dir2 -sw 10m -W 5g``` | Scan for file size between 10Mb and 5Gb |

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
* Run command ```dir2 -? cfg``` to find your personal configuration file.
* Short-cut of option is **NOT** valid in the configuration file.
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

**End**
