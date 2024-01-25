
# Date time format

* The example date/time is ```2019-06-12T15:47:10+08:00```
* A literal string (fixed-text) must be enclosed by double quotation mark by ```%22```.
* A tailing space must be enclosed as ```%20```.
* A horizontal tab character must be enclosed as ```%09```.
* If ```%20``` is used to ```--within``` or ```--not-wihtin``` options, a plus sign (Time-Zone) must be encoded as ```%2b```.
Please check [encoding-list](https://github.com/ck-yung/dir2cs/blob/main/docs/info-encode-char.md) for more information.
* [Custom configuartion to ```--date-format short```](https://github.com/ck-yung/dir2cs/blob/main/docs/date-short-cfg.md)
* [顯示中文日期](https://github.com/ck-yung/dir2cs/blob/main/docs/date-lang-code.md)

## Format specifier

| Format | Description | Example |
| ------ | ----------- | ------- |
| ```d``` | The day of the month, from 1 to 31. | ```12``` |
| ```dd``` | The day of the month, from 01 to 31. | ```12``` |
| ```ddd``` | The abbreviated name of the day of the week. | ```Wed``` |
| ```dddd``` | The full name of the day of the week. | ```Wednesday``` |
| ```h``` | The hour, using a 12-hour clock from 1 to 12. | ```3``` |
| ```hh``` | The hour, using a 12-hour clock from 01 to 12. | ```03``` |
| ```HH``` | The hour, using a 24-hour clock from 01 to 23. | ```15``` |
| ```mm``` | The minute, from 00 to 59. | ```47``` |
| ```M``` | The month, from 1 to 12. | ```6``` |
| ```MM``` | The month, from 01 to 12. | ```06``` |
| ```MMM``` | The abbreviated name of the month. | ```Jun``` |
| ```MMMM``` | The full name of the month. | ```June``` |
| ```ss``` | The second, from 00 to 59. | ```10``` |
| ```t``` | The first character of the AM/PM designator. | ```P``` |
| ```tt``` | The AM/PM designator. | ```PM``` |
| ```yy``` | The year, from 00 to 99. | ```19``` |
| ```yyyy``` | The year as a four-digit number. | ```2019``` |
| ```z``` | Hours offset from UTC. | ```+8``` |
| ```zz``` | Hours offset from UTC, with a leading zero for a single-digit value. | ```+08``` |
| ```zzz``` | Hour:Minute offset from UTC. | ```+08:00``` |

## Standard pattern / Single char format

| Format  | Pattern    | Example |
| ------  | -------    | ------- |
| ```d``` | Short date | ```12-Jun-19```
| ```D``` | Long Date | ```Wednesday, June 12, 2019```|
| ```f``` | Full short | ```Wednesday, June 12, 2019 03:47 PM``` |
| ```F``` | Full long | ```Wednesday, June 12, 2019 15:47:10```|
| ```g``` | General short   | ```12-Jun-19 03:47 PM``` |
| ```G``` | General long | ```12-Jun-19 15:47:10```|
| ```m``` | Month      | ```June 12``` |
| ```M``` | Month | ```June 12```|
| ```o``` | Round-trip | ```2019-06-12T15:47:10.0000000``` |
| ```O``` | Round-trip | ```2019-06-12T15:47:10.0000000```|
| ```s``` | Sortabl | ```2019-06-12T15:47:10``` |
| ```t``` | Short time | ```03:47 PM``` |
| ```T``` | Long time | ```15:47:10```|
| ```u``` | Universal sortable | ```2019-06-12 15:47:10Z``` |
| ```U``` | Universal full | ```Wednesday, June 12, 2019 07:47:10```|
| ```y``` | Year | ```June 2019``` |
| ```Y``` | Year | ```June 2019```|

# Time zone format

* The feature will be implemented at ```v2.1.2``` (2024 Feb).

* Define "Standard Time" (STD) offsets from "UTC" in hours and minutes.

| Format | STD Offset | Example |
| ------ | ----------- | ------- |
| ```--date-time timezone=```FORMAT | Asia/Hong Kong | ```--date-time UTC+8``` |
| | Asia/Japan | ```--date-time UTC+9``` |
| | US/Central | ```--date-time UTC-6``` |
| | Australia/Yancowinna | ```--date-time UTC+09:30``` |

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
