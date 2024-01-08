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

* 成果範例
```
 145M 12年9月    年少無知◎公民廣場.mp4
  52K 11月22日   《清室退位詔書》愛新覺羅．溥儀.1912.txt
   4K 週一上午   軍官學校-黃埔軍校訓詞.1924.txt
   3M 昨下10時   Nothing to Envy 我們最幸福-韓人民北的真實生活.2009.epub
 333M 上午11:26  百變1991告別舞台-梅艷芳.mp4
 151K 剛剛       On Liberty 論自由（嚴復《群己權界論》）Mill.1859.epub
 ```

## 格式注解
| 格式     | 成果範例 |
| ---      | --- |
| HH:mm    | 13:24  |
| tth:mm   | PM1:24 |
|          | 下午1:24 |
| th:mm    | M1:24 |
|          | 下1:24 |
| ddd tt   | Tue PM |
|          | 週二 下午
| MMM dd   | Sept 17 |
|          | 9月 17 |
| yyyy MMM | 2023 Sept |
|          | 2023 9月 |

* 字符```%```之注解
    - ```%20```即為空格
    - ```%09```即為Tab

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
