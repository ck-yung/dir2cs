# Sample Configuration for --date-format short

* Filename: <fixed>dir2.date-short.opt</fixed>
* 'dir2' program loads the opt file in location <fixed>~/.local</fixed> or <fixed>%userprofile%\\.local</fixed>

## Default Configuration
<fixed>
Pad=8<br/>
Just=Just<br/>
Today=hh:mmtt<br/>
YsDay="Yd" hhtt<br/>
Week=ddd hhtt<br/>
Year=MMM dd<br/>
Else= yyyy MMM<br/>
</fixed>

## Demo Configuration for zn-tw Display
<fixed>
pad=7,tab<br/>
just=剛剛<br/>
today=tth:mm<br/>
ysday=昨tt<br/>
week=dddtt<br/>
year=MMMd日<br/>
else=yy年MMM<br/>
</fixed>

## Demo Configuration for zn-hk Display
<fixed>
pad=10,tab<br/>
just=剛剛<br/>
today=tth:mm<br/>
ysday=昨tth點<br/>
week=dddtth點<br/>
year=MMMd日<br/>
else=yyyy年MMM<br/>
</fixed>

## Output Sample
| Format   | Output |
| ---      | --- |
| HH:mm    | 13:24  |
| tth:mm   | PM1:24 |
|          | 下午1:24 |
| ddd tt   | Tue PM |
|          | 週二 下午
| MMM dd   | Sept 17 |
|          | 9月 17 |
| yyyy MMM | 2023 Sept |
|          | 2023 9月 |
