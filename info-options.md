# Complete Option List

| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | --help cfg       | 
| -v       | --version        | 
|          | --dir            | ```both``` ```off``` ```only``` ```only-link``` ```tree```
|          | --sub            | ```off``` ```all```
|          | --cfg-off        |
|          | --utf8           |
| -w       | --within         | DATE or SIZE | ```-w 12m``` ```-w 3day```
| -W       | --not-within     | DATE or SIZE | ```-W 10k``` ```-W 2hour```
|          |                  |              | ```-w 14day -W +7day```
| -x       | --excl           | WILD[,WILD ..]  | ```-x *.tmp,*.temp```    |
|          |                  | ```:link```     |
| -X       | --excl-dir       | WILD[,WILD ..]  | ```-X obj,bin```           |
|          |                  | ```:link```     |
|          | --regex          |
| -c       | --case-sensitive |
|          | --creation-date  |
| -k       | --keep-dir       |
|          | --no-ext         | ```incl``` ```excl``` ```only```
|          | --hidden         | ```incl``` ```excl``` ```only```
|          | --size-format    | INTEGER  ```commna``` ```short``` ```+short```  | ```--size-format 12,comma```
|          |                  |                         | ```--size-format short```
|          | --count-format   | INTEGER  ```commna``` ```short```  | ```--count-format 6,comma```
|          | --date-format    | ```short``` ```unix``` FORMAT | ```--date-format yyyy-MMM-ddTHH:mm:ss```
|          |                  |                               | ```--date-format yy-MM-dd%20HH:mm ```
|          | --creation-date  |
|          | --total          | ```off``` ```only```
|          | --hide           | ```date,size,cout,mode,owner,link```
|          | --show           | ```date,size,cout,mode,owner,link,link-size,link-date```
|          | --link           | ```incl``` ```only```
|          | --excl-none      | [Clear all ```--excl``` and ```--excl-dir```]
| -o       | --sort           | ```off``` ```name,size,date,ext,count,last``` [up to 2 columns] | ```-o date```
| -r       | --reverse        | ```off``` ```on```
|          | --take           | INTEGER  SIZE  | ```--take 10```
|          |                  |                | ```--take 500Mb```
|          | --total          | ```off``` ```only```
|          | --sum            | ```ext``` ```dir``` ```+dir``` ```year```
