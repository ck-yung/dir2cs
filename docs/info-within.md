# Options ```--within``` and ```--not-within```

## ```--within```
Option ```--within``` (shortcut ```-w```) select files as follows.

| Postfix  | Example   | Example Description                                    |
| -------  | -------   | -------------------                                    |
| Kb,Mb,Gb | -w 100Kb  | Show file if its size is smaller than 100 Kb.          |
| k,m,g    | -w 10m    | Show file if its size is smaller than 10 Mb.           |
| min      | -w 20min  | Show file if its last write-time is within 10 minutes. |
| minutes  | -w 20minutes | |
| hours    | -w 3hours | Show file if its last write-time is within 3 hours.    |
| days     | -w 14days | Show file if its last write-time is within 14 days.    |
| | | |
|          | -w 2014-12-25 | Show file if its last write-time is NOT before X'Mas. |
|          | -w 13:30      | Show file if its last write-time is NOT before today 13:30. |
|          | -w 2019-06-12T15:20 |                                              |

## ```--not-within```
Option '--not-within' (shortcut '-W' upper case) select files as follows.

| Postfix  | Example   | Example Description                                    |
| -------  | -------   | -------------------                                    |
| Kb,Mb,Gb | -W 90k    | Show file if its size is bigger than 90 Kb.            |
| k,m,g    | -W 2m     | Show file if its size is bigger than 2 Mb.             |
| min      | -W 15min  | Show file if its last write-time is before 15 minutes. |
| minutes  | -w 20minutes | |
| hours    | -W 2hour  | Show file if its last write-time is before 2 hours.    |
| days     | -W 7day   | Show file if its last write-time is before 7 days.     |
| | | |
|          | -W 2014-12-25 | Show file if its last write-time is before X'Mas.  |
|          | -W 13:30      | Show file if its last write-time is before today 13:30. |
|          | -W 2019-06-12T15:20 |                                              |
| | | |
|          | -W +16min  | Show file if its last write-time is within 16 min after '--within' option |
|          | -W +3hours | Show file if its last write-time is within 3 hours after '--within' option |
|          | -W +6days  | Show file if its last write-time is within 6 days after '--within' option |

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
