# Complete Option List

| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | [--help cfg](https://github.com/ck-yung/dir2cs/blob/main/docs/info-config-file.md) | 
| -v       | --version        | 
|          | --dir            | ```both``` ```off``` ```only``` ```only-link``` ```tree```
|          | --sub            | ```off``` ```all```
|          | --cfg-off        |
|          | --utf8           |
| -w       | [--within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE or SIZE | ```-w 12m``` ```-w 3day```
| -W       | [--not-within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE or SIZE | ```-W 10k``` ```-W 2hour```
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
|          | [--size-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-size-format.md) | INTEGER  ```commna``` ```short``` ```+short```  | ```--size-format 12,comma```
|          |                  |                         | ```--size-format short```
|          | [--date-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) | ```short``` ```unix``` FORMAT | ```--date-format yyyy-MMM-ddTHH:mm:ss```
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
|          | [--sum](https://github.com/ck-yung/dir2cs/blob/main/docs/info-sum.md) | ```ext``` ```dir``` ```+dir``` ```year```

 [Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
