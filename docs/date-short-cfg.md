# Custom configuartion to ```--date-format short```

## Default configuartion

| Type       | Description      | Default Value   | Demo Format            |
| ----       | -----------      | --------------  | -----------            |
| Culture    | Language         | ```"en-US"```   | ```culture=zh-TW```    |
| Just now   | Within 2 minutes | ```"Just"```    | ```just="Now"%20%20``` |
| Today      | Within today     | ```hh:mmtt```   | ```today=HH:mm:ss```   |
| Yesterday  | Within yesterday | ```"Yd" hhtt``` | ```yesterday=%22Yesterday%22 HH``` |
| This week  | Within 6 days    | ```ddd hhtt```  | ```week=ddd HH:mm```   |
| This month | Within the month | ```MMM dd```    | ```month=MMM ddtt```   |
| This year  | Within the year  | ```MMM dd```    | ```year=MMM dd```      |
| else       | Else             | ```yyyy MMM```  | ```else=yyyy-MM```     |

* The formats of "Today AM" and "Today PM" are referred to "Today".

* The formats of "Yesterday AM" and "Yesterday PM" are referred to "Yeserday".


## Configuration file of ```~\.local\dir2-date-short.txt```

| Type       | Format              | Default Format        | Demo Display   |
| ----       | -----------         | --------------        | ------------   |
| Culture    | ```Culture=TEXT```  | ```Culture=en-US```   |                |
| Just now   | ```Just=TEXT```     | ```Just="Just"```     | ```Just```     |
| Today      | ```Today=```FORMAT  | ```Today=hh:mmtt```   | ```03:47PM```  |
| Yesterday  | ```YsDay=```FORMAT  | ```YsDay="Yd" hhtt``` | ```Yd 03PM```  |
| This week  | ```Week=```FORMAT   | ```Week=ddd hhtt```   | ```Wed 03PM``` |
| This month | ```Month=```FORMAT  | ```Month=MMM dd```    | ```Jun 12```   |
| This year  | ```Year=```FORMAT   | ```Year=MMM dd```     | ```Jun 12```   |
| else       | ```Else=```FORMAT   | ```Else=yyyy MMM```   | ```2019 Jun``` |

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

## Detailed hour description of TODAY and YESERDAY.

* The feautre will be releaesd in version 2.1.2 (2024 Feb).
* Other than 'AM/PM' description, you can find a hour break-down in ```Today``` and ```Yeserday```.


| Type | Format | Default Format | Example Format | Example |
| ---- | ------ | -------------- | -------------- | ------- |
| Flag | ```TodayHours=```FLAG | | ```TodayHours=Yes```  | |
| Today 00:00 ~ 00:59 | ```Today00~01=```FORMAT | ```hh:mmtt```| ```Today00~01=%22This%20Midnight%22%20%09``` | ```This Midnight``` |
| Today 01:00 ~ 05:59 | ```Today01~06=```FORMAT | ```hh:mmtt```| ```Today01~06=%22This%20Early%22%20h09``` | ```This Early 8``` |
| Today 06:00 ~ 12:59 | ```Today06~12=```FORMAT | ```hh:mmtt```| ```Today06~12=%22This%20Morning%22%20h%09``` | ```This Morning 11``` |
| Today 12:00 ~ 17:59 | ```Today12~18=```FORMAT | ```hh:mmtt```| ```Today12~18=%22This%20Afternoon%22%20h%09``` | ```This Afternoon 3``` |
| Today 18:00 ~ 23:59 | ```Today18~24=```FORMAT | ```hh:mmtt```| ```Today00~01=%22This%20Night%22%20h%09``` | ```This Night 11``` |
| Flag | ```YsdayHours=```FLAG | | ```YsdayHours=TRUE```  | |
| Yeserday 00:00 ~ 00:59 | ```Ysday00~01=```FORMAT | ```"Yd" hhtt```| ```Ysday00~01=%22Yeserday%20Midnight%22%20%09``` | ```Yeserday Midnight``` |
| Yeserday 01:00 ~ 05:59 | ```Ysday01~06=```FORMAT | ```"Yd" hhtt```| ```Ysday01~06=%22Yeserday%20Early%22%20h09``` | ```Yeserday Early 8``` |
| Yeserday 06:00 ~ 12:59 | ```Ysday06~12=```FORMAT | ```"Yd" hhtt```| ```Ysday06~12=%22Yeserday%20Morning%22%20h%09``` | ```Yeserday Morning 11``` |
| Yeserday 12:00 ~ 17:59 | ```Ysday12~18=```FORMAT | ```"Yd" hhtt```| ```Ysday12~18=%22Yeserday%20Afternoon%22%20h%09``` | ```Yeserday Afternoon 3``` |
| Yeserday 18:00 ~ 23:59 | ```Ysday18~24=```FORMAT | ```"Yd" hhtt```| ```Ysday00~01=%22Yeserday%20Night%22%20h%09``` | ```Yeserday Night 11``` |
| Flag | ```WkdayHours=```FLAG | | ```WkdayHours=Yes```  | |
| 00:00 ~ 00:59 | ```Wkday00~01=```FORMAT | ```hh:mmtt```| ```Wkday00~01=%22The%20Midnight%22%20%09``` | ```The Midnight``` |
| 01:00 ~ 05:59 | ```Wkday01~06=```FORMAT | ```hh:mmtt```| ```Wkday01~06=%22The%20Early%22%20h09``` | ```The Early 8``` |
| 06:00 ~ 12:59 | ```Wkday06~12=```FORMAT | ```hh:mmtt```| ```Wkday06~12=%22The%20Morning%22%20h%09``` | ```The Morning 11``` |
| 12:00 ~ 17:59 | ```Wkday12~18=```FORMAT | ```hh:mmtt```| ```Wkday12~18=%22The%20Afternoon%22%20h%09``` | ```The Afternoon 3``` |
| 18:00 ~ 23:59 | ```Wkday18~24=```FORMAT | ```hh:mmtt```| ```Wkday00~01=%22The%20Night%22%20h%09``` | ```The Night 11``` |


[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
