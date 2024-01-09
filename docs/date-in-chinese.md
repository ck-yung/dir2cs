# 顯示中文日期

## 設定

* 選項```--date-format```需指定為```short```。

* 以下是設定文件```~\.local\dir2.date-short.opt```的一個範例。
```
culture=zh-TW
just = 剛剛%20%09
today= tth:mm%09
ysday= 昨th時%09
week = dddtt%09
year = MMMd日%09
else = yy年MMM%09
```

## 格式注解

| 格式      | 語言設定 | 成果範例 |
| ---      | ----- | --- |
| HH:mm    ||  13:24  |
| tth:mm   | en-US | PM1:24 |
|          | zh-TW | 下午1:24 |
| th:mm    | en-US | P1:24 |
|          | zh-TW | 下1:24 |
| ddd tt   | en-US | Tue PM |
|          | zh-TW | 週二 下午
| MMM dd   | en-US | Sept 17 |
|          | zh-TW | 9月 17 |
| yyyy MMM | en-US | 2023 Sept |
|          | zh-TW | 2023 9月 |

* 字符```%```之注解
    - ```%20```即為空格
    - ```%09```即為Tab

* 成果範例
```
 145M 12年9月    年少無知◎公民廣場.mp4
  52K 11月22日   《清室退位詔書》愛新覺羅．溥儀.1912.txt
   4K 週一上午   軍官學校-黃埔軍校訓詞.1924.txt
   3M 昨下10時   Nothing to Envy 我們最幸福-韓人民北的真實生活.2009.epub
 333M 上午11:26  百變1991告別舞台-梅艷芳.mp4
 151K 剛剛       On Liberty 論自由（嚴復《群己權界論》）Mill.1859.epub
 ```

# 細緻時段

除了能夠顯示「上午／下午」之外，以下設定可以表示類似『晨早午晚』。

| 分類  | 格式  | 格式範例 | 範例 |
| ---- | ---  | -------------- | ------- |
| 啟動設定 | ```TodayHours:FLAG``` | ```TodayHours:Yes```  | |
|        |                       | ```TodayHours:True``` | |
| 今日 00:00 ~ 00:59 | ```Today00~01=FORMAT``` | ```Today00~01=今00:mm%20%20%09``` | ```今00:07``` |
| 今日 01:00 ~ 05:59 | ```Today01~06=FORMAT``` | ```Today01~06=今晨h:mm%09``` | ```今晨3:04``` |
| 今日 06:00 ~ 12:59 | ```Today06~12=FORMAT``` | ```Today06~12=今早h:mm%09``` | ```今早10:04``` |
| 今日 12:00 ~ 17:59 | ```Today12~18=FORMAT``` | ```Today12~18=今午h:mm%09``` | ```今午4:05``` |
| 今日 18:00 ~ 23:59 | ```Today18~24=FORMAT``` | ```Today00~01=今晚h:mm%09``` | ```今晚11:23``` |
| 昨日 00:00 ~ 00:59 | ```Ysday00~01=FORMAT``` | ```Ysday00~01=昨零時%20%20%09``` | ```昨零時``` |
| 昨日 01:00 ~ 05:59 | ```Ysday01~06=FORMAT``` | ```Ysday01~06=昨晨h時%09``` | ```昨晨3時``` |
| 昨日 06:00 ~ 12:59 | ```Ysday06~12=FORMAT``` | ```Ysday06~12=昨早h時%09``` | ```昨早10時``` |
| 昨日 12:00 ~ 17:59 | ```Ysday12~18=FORMAT``` | ```Ysday12~18=昨午h時%09``` | ```昨午4時``` |
| 昨日 18:00 ~ 23:59 | ```Ysday18~24=FORMAT``` | ```Ysday00~01=昨晚h時%09``` | ```昨晚11時``` |

* 設定範例

```
todayHours:yes
today00~01=今00:mm%20%20%09
today01~06=今晨h:mm%09
today06~12=今早h:mm%09
today12~18=今午h:mm%09
today18~24=今晚h:mm%09
ysday00~01=昨零時%20%20%20%09
ysday01~06=昨晨h時%20%09
ysday06~12=昨早h時%20%09
ysday12~18=昨午h時%20%09
ysday18~24=昨晚h時%20%09```

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
