# Custom configuartion to ```--date-format short```

## Default configuartion

| Type      | Description      | Default Format  | Example        |
| ----      | -----------      | --------------  | -------        |
| Culture   | Language         | ```"en-US"```   |                |
| Just now  | Within 2 minutes | ```"Just"```    | ```Just```     |
| Today     | Within today     | ```hh:mmtt```   | ```02:46PM```  |
| Yesterday | Within yesterday | ```"Yd" hhtt``` | ```Yd 03PM```  |
| This week | Within 6 days    | ```ddd hhtt```  | ```Mon 04PM``` |
| This year | Within the year  | ```MMM dd```    | ```Dec 29```   |
| else      | Else             | ```yyyy MMM```  | ```2023 Dec``` |

* The formats of "Today AM" and "Today PM" are referred to "Today".

* The formats of "Yesterday AM" and "Yesterday PM" are referred to "Yeserday".


## Configuration file of ```~\.local\dir2.date-short.opt```

| Type      | Format              | Example               |
| ----      | -----------         | ------                |
| Culture   | ```Culture=TEXT```  | ```Culture=en-US```   |
| Just now  | ```Just=TEXT```     | ```Just=Just```       |
| Today     | ```Today=FORMAT```  | ```Today=hh:mmtt```   |
| Yesterday | ```YsDay=FORMAT```  | ```YsDay="Yd" hhtt``` |
| This week | ```Week=FORMAT```   | ```Week=ddd hhtt```   |
| This year | ```Year=FORMAT```   | ```Year=MMM dd```     |
| else      | ```Else=FORMAT```   | ```Else=yyyy MMM```   |

## Today AM/PM
* The formats of "Today AM" and "Today PM" can be different if
one of the followings is found in the ```opt``` file.
| Type | Format | Example |
| ---- | ------ | ------- |
| Init | ```TodayAmPm:Flag``` | ```TodayAMPM:Yes```  |
|      |                      | ```TodayAMPM:True``` |
| AM   | ```TodayAM=FORMAT``` | ```TodayAM=h:mm"a"``` |
| PM   | ```TodayPM=FORMAT``` | ```TodayPM=h:mm"p"``` |


## Yeserday AM/PM
* The formats of "Yeserday AM" and "Today PM" can be different if
one of the followings is found in the ```opt``` file.
| Type | Format | Example |
| ---- | ------ | ------- |
| Init | ```YsdayAmPm:Flag``` | ```YsdayAMPM:Yes```  |
|      |                      | ```YsdayAMPM:True``` |
| AM   | ```YsdayAM=FORMAT``` | ```YsdayAM="Yesdy AM"``` |
| PM   | ```YsdayPM=FORMAT``` | ```YsdayPM="Yesdy PM"``` |

## Remark to ```FORMAT``` text

* A literal string (fixed-text) must be enclosed by double quotation mark or ```%22```.
* A tailing space must be enclosed as ```%20```.
* A horizontal tab character must be enclosed as ```%09```.

## Date time format text

| Format | Description | Example |
| ------ | ----------- | ------- |
| ```d``` | The day of the month, from 1 to 31. | 2019-06-12 -> ```12``` |
| ```dd``` | The day of the month, from 01 to 31. | 2019-06-12 -> ```12``` |
| ```ddd``` | The abbreviated name of the day of the week. | 2019-06-12 -> ```Wed``` |
| ```dddd``` | The full name of the day of the week. | 2019-06-12 -> ```Wednesday``` |
| ```h``` | The hour, using a 12-hour clock from 1 to 12. | 2019-06-12T15:47:10 -> ```3``` |
| ```hh``` | The hour, using a 12-hour clock from 01 to 12. | 2019-06-12T15:47:10 -> ```03``` |
| ```HH``` | The hour, using a 24-hour clock from 01 to 23. | 2019-06-12T15:47:10 -> ```15``` |
| ```mm``` | The minute, from 00 to 59. | 2019-06-12T15:47:10 -> ```47``` |
| ```M``` | The month, from 1 to 12. | 2019-06-12T15:47:10 -> ```6``` |
| ```MM``` | The month, from 01 to 12. | 2019-06-12T15:47:10 -> ```06``` |
| ```MMM``` | The abbreviated name of the month. | 2019-06-12T15:47:10 -> ```Jun``` |
| ```MMMM``` | The full name of the month. | 2019-06-12T15:47:10 -> ```June``` |
| ```ss``` | The second, from 00 to 59. | 2019-06-12T15:47:10 -> ```10``` |
| ```t``` | The first character of the AM/PM designator. | 2019-06-12T15:47:10 -> ```P``` |
| ```tt``` | The AM/PM designator. | 2019-06-12T15:47:10 -> ```PM``` |
| ```yy``` | The year, from 00 to 99. | 2019-06-12T15:47:10 -> ```19``` |
| ```yyyy``` | The year as a four-digit number. | 2019-06-12T15:47:10 -> ```2019``` |
| ```zz``` | Hours offset from UTC, with a leading zero for a single-digit value. | 2019-06-12T15:47:10+08:00 -> ```+08``` |

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
