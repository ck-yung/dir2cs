# Default DateTime Short Format

## 設定

* Option ```--date-format short```
    - Value ```short``` should be assigned to the option ```--date-format```.
    - 選項```--date-format```需指定為```short```。
    - オプション ```--date-format``` は ```short``` として指定する必要があります。

* Language Code (Culture Info) in  ```dir2-date-short.txt```
    - English ```culture=en-US``` is the default value.
    - 漢字 Han ```culture=zh-TW```
    - 日文 Kanji ```culture=ja-JP```

* Default Configuration
    -  Remark to escape character ```%```
        - ```%09``` is a tab char.
        - ```%20``` is a space.
        - ```%22``` is a double closing mark.
        - Any literal text is required to be enclosed by double closing marks.
    - The defaul configuration is same to below ```~\.local\dir2.date-short.opt```.

```
culture=en-US
just = Just%20%20%20%20
today= hh:mmtt%20
ysday= %22Yd%22%20hhtt
week = ddd%20hhtt
month= MMM%20dd%20%20
year = MMM%20dd%20%20
else = yyyy%20MMM
```


* 漢字設定 / Configuration File ```dir2-date-short.txt``` in Han.
    -  字符```%```之注解
        - ```%09```即為 Tab 字符
        - ```%20```即為空格
    - 以下是設定文件```~\.local\dir2-date-short.txt```的一個範例。

```
culture=zh-TW
just = 剛剛%20%09
today= tth:mm%09
ysday= 昨th時%09
week = dddtt%09
month= dd日tt%09
year = MMMdd日%09
else = yy年MMM%09
```

* 漢字成果範例
```
 145M 12年9月    年少無知◎公民廣場.mp4
  52K 11月22日   《清室退位詔書》愛新覺羅．溥儀.1912.txt
  33M 本月22日   《在台上我覓理想》梅艷芳.1984.mp3
   4K 週一上午   軍官學校-黃埔軍校訓詞.1924.txt
   3M 昨下10時   Nothing to Envy 我們最幸福-韓人民北的真實生活.2009.epub
 333M 上午11:26  百變1991告別舞台-梅艷芳.mp4
 151K 剛剛       On Liberty 論自由（嚴復《群己權界論》）Mill.1859.epub
 ```


* 日字設定 / Configuration File ```dir2-date-short.txt``` in Japanese.
    - 以下は設定ファイル```~\.local\dir2-date-short.txt```の例です。

```
culture=ja-JP
just = 剛剛%20%09
today= tth:mm%09
ysday= 昨tth%09
week = dddtt%09
month= dd日tt%09
year = MMMdd日%09
else = yy年MMM%09
```


## 格式注解

| 格式      | 語言設定 | 成果範例 |
| ---      | ----- | ---- |
| ```HH:mm```    | ```en-US``` | ```15:47``` |
|                | ```zh-TW``` | ```15:47``` |
|                | ```ja-JP``` | ```15:47``` |
| ```tth:mm```   | ```en-US``` | ```PM3:47``` |
|                | ```zh-TW``` | ```下午3:47``` |
|                | ```ja-JP``` | ```午後3:47``` |
| ```th:mm```    | ```en-US``` | ```P3:47``` |
|                | ```zh-TW``` | ```下3:47``` |
|                | ```ja-JP``` | ```午3:47``` |
| ```ddd tt```   | ```en-US``` | ```Wed PM``` |
|                | ```zh-TW``` | ```週三 下午``` |
|                | ```ja-JP``` | ```水 午後``` |
| ```ddddt```    | ```en-US``` | ```WedensdayP``` |
|                | ```zh-TW``` | ```星期三下``` |
|                | ```ja-JP``` | ```水曜日午``` |
| ```MMM dd```   | ```en-US``` | ```Jun 12``` |
|                | ```zh-TW``` | ```6月 12``` |
|                | ```ja-JP``` | ```6月 12``` |
| ```yyyy MMM``` | ```en-US``` | ```2023 Jun``` |
|                | ```zh-TW``` | ```2023 6月``` |
|                | ```ja-JP``` | ```2023 6月``` |


* 文字%に関するコメント
    - ```%20```はスペースです。
    - ```%20```はTabです。

# 細緻時段

* 此功能將於版本 v2.1.2 (2024年2月) 實現。
* 除了能夠顯示「上午／下午」之外，以下設定可以表示類似『晨早午晚』。

| 分類  | 格式  | 格式範例 | 範例 |
| ---- | ---  | -------------- | ------- |
| 啟動設定 | ```TodayHours=```FLAG | ```TodayHours=Yes```  | |
| 今日 00:00 ~ 05:59 | ```Today00~06=```FORMAT | ```Today00~06=今晨hh:mm%09``` | ```今晨03:04``` |
| 今日 06:00 ~ 12:59 | ```Today06~12=```FORMAT | ```Today06~12=今早hh:mm%09``` | ```今早10:04``` |
| 今日 12:00 ~ 17:59 | ```Today12~18=```FORMAT | ```Today12~18=今午hh:mm%09``` | ```今午04:05``` |
| 今日 18:00 ~ 23:59 | ```Today18~24=```FORMAT | ```Today00~01=今晚hh:mm%09``` | ```今晚11:23``` |
| 啟動設定 | ```YsdayHours=```FLAG | ```YsdayHours=TRUE```  | |
| 昨日 01:00 ~ 05:59 | ```Ysday00~06=```FORMAT | ```Ysday00~06=昨晨hh時%20%09``` | ```昨晨03時``` |
| 昨日 06:00 ~ 12:59 | ```Ysday06~12=```FORMAT | ```Ysday06~12=昨早hh時%20%09``` | ```昨早10時``` |
| 昨日 12:00 ~ 17:59 | ```Ysday12~18=```FORMAT | ```Ysday12~18=昨午hh時%20%09``` | ```昨午04時``` |
| 昨日 18:00 ~ 23:59 | ```Ysday18~24=```FORMAT | ```Ysday00~01=昨晚hh時%20%09``` | ```昨晚11時``` |
| 啟動設定 | ```WkdayHours=```FLAG | ```WkdayHours=TRUE```  | |
| 本週 00:00 ~ 05:59 | ```Wkday00~06=```FORMAT | ```Wkday00~06=ddd晨%20%09``` | ```週三晨``` |
| 本週 06:00 ~ 12:59 | ```Wkday06~12=```FORMAT | ```Wkday06~12=ddd早%20%09``` | ```週三早``` |
| 本週 12:00 ~ 17:59 | ```Wkday12~18=```FORMAT | ```Wkday12~18=ddd午%20%09``` | ```週三午``` |
| 本週 18:00 ~ 23:59 | ```Wkday18~24=```FORMAT | ```Wkday00~01=ddd晚%20%09``` | ```週三晚``` |


* 設定範例

```
culture=zh-TW
just = 剛剛%20%09
month= dd日tt%20%09
year = MMMd日%09
else = yy年MMM%09
todayHours=yes
today00~06=今晨HH:mm%09
today06~12=今早hh:mm%09
today12~18=今午hh:mm%09
today18~24=今晚hh:mm%09
ysdayHours=yes
ysday00~06=昨晨HH時%20%09
ysday06~12=昨早hh時%20%09
ysday12~18=昨午hh時%20%09
ysday18~24=昨晚hh時%20%09
wkdayHours=yes
wkday00~06=dddd晨%09
wkday06~12=dddd早%09
wkday12~18=dddd午%09
wkday18~24=dddd晚%09
```


* 成果範例
```
 145M 12年9月    年少無知◎公民廣場.mp4
  52K 11月22日   《清室退位詔書》愛新覺羅．溥儀.1912.txt
  33M 23日上午   《在台上我覓理想》梅艷芳.1984.mp3
   4K 星期一早   軍官學校-黃埔軍校訓詞.1924.txt
   3M 昨晚10時   Nothing to Envy 我們最幸福-韓人民北的真實生活.2009.epub
 333M 今早11:26  百變1991告別舞台-梅艷芳.mp4
 151K 剛剛       On Liberty 論自由（嚴復《群己權界論》）Mill.1859.epub
 ```

 
# Weekday Format／週日名稱格式／日曜日の名前の形式

| Format | English | 漢字 Han | 日文Kanji |
| ------ | ------- | ------- | -------- |
| dddd | | |
| | Sunday  | 星期日 | 日曜日 |
| | Monday | 星期一 | 月曜日 |
| | Tuesday | 星期二 | 火曜日 |
| | Wednesday | 星期三 | 水曜日 |
| | Thursday | 星期四 | 木曜日 |
| | Friday | 星期五 | 金曜日 |
| | Saturday | 星期六 | 土曜日 |
| ddd | | |
| | Sun | 週日 | 日 |
| | Mon | 週一 | 月 |
| | Tue | 週二 | 火 |
| | Wed | 週三 | 水 |
| | Thu | 週四 | 木 |
| | Fri | 週五 | 金 |
| | Sat | 週六 | 土 |

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
