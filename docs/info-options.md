# Complete Option List

| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | [--help cfg](https://github.com/ck-yung/dir2cs/blob/main/docs/info-config-file.md) | 
| -v       | --version        | 
|          | --dir            | ```both``` ```off``` ```only``` [```only-link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md) ```tree```
|          | --sub            | ```off``` ```all```
|          | --cfg-off        |
|          | --utf8           |
| -w       | [--within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE or SIZE | ```-w 12Mb``` ```-w 3days```
| -W       | [--not-within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE or SIZE | ```-W 10k``` ```-W 2hr```
|          |                  |              | ```-w 30days -W +7days```
| -x       | --excl           | WILD[,WILD ..]  | ```-x *.tmp,*.temp```    |
|          |                  | [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md) |
| -X       | --excl-dir       | WILD[,WILD ..]  | ```-X obj,bin```           |
|          |                  | [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md) |
|          | --regex          |
| -c       | --case-sensitive |
|          | --creation-date  |
| -k       | --keep-dir       |
|          | --no-ext         | ```incl``` ```excl``` ```only```
|          | --hidden         | ```incl``` ```excl``` ```only```
|          | [--size-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-size-format.md) | INTEGER  ```commna``` ```short``` ```+short```  | ```--size-format 12,comma```
|          |                  |                         | ```--size-format short```
|          | [--date-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) | ```short``` ```unix``` FORMAT ｜```utc```OFFSET | ```--date-format yyyy-MMM-ddTHH:mm:ss```
|          |                  |                               | ```--date-format yy-MM-dd%20HH:mm ```
|          |                  |                               | ```--date-format utc+8 ```
|          | --creation-date  |
|          | --total          | ```off``` ```only```
|          | --hide           | ```date,size,cout,mode,owner,last,link```
|          | --show           | ```date,size,cout,mode,owner,last,link,link-size,link-date```
|          | --link           | ```incl``` [```only```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md)
|          | --excl-none      | [Clear all ```--excl``` and ```--excl-dir```]
| -o       | --sort           | ```off``` ```name,size,date,ext,count,last``` [up to 2 columns] | ```-o date```
| -r       | --reverse        | ```off``` ```on```
|          | --take           | COUNT  SIZE  | ```--take 10```
|          |                  |              | ```--take 500Mb```
|          | --total          | ```off``` ```only```
|          | [--sum](https://github.com/ck-yung/dir2cs/blob/main/docs/info-sum.md) | ```ext``` ```dir``` ```+dir``` ```year```
|          | --end-time    | FORMAT       | ```--end-time %22TimeZone%20%22zz```
|          |                  |              | ```--end-time %22Done%20at%20%22yyyy-MMM-dd%20HH:mm%20zz```

* The following features will be implemented at ```v2.1.2``` (2024 Feb).
    - Option ```--end-time```
    - ```utf```OFFSET to ```--date-format```
    - ```last``` to options ```--show``` and ```--hide```

 [Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
