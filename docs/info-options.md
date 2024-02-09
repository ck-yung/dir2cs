# Complete Option List

| Shortcut | Option           | Available Value         | Example |
| -------- | ------           | ---------------         | ------- |
| -?       | --help           | 
| -? cfg   | [--help cfg](https://github.com/ck-yung/dir2cs/blob/main/docs/info-config-file.md) | 
| -v       | --version        | 
|          | --dir            | ```both```｜```off```｜```only```｜[```only-link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md)｜```tree```|```--dir tree```
|          | --sub            | ```off```｜```all``` | ```--sub all```
|          | --cfg-off        |
|          | --utf8           |
|          | [--color](https://github.com/ck-yung/dir2cs/blob/main/docs/info-color.md) | COLOR[,INTEGER,COLOR-OF-TOTAL-LINE] | ```--color green,10,cyan```
| -w       | [--within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE and SIZE | ```-w 12Mb``` ```-w 3days```
| -W       | [--not-within](https://github.com/ck-yung/dir2cs/blob/main/docs/info-within.md) | DATE and SIZE | ```-W 10k``` ```-W 2hr```
|          |                  |              | ```-w 30days -W +7days```
| -x       | --excl           | WILD[,WILD ..]  | ```-x *.tmp,*.temp,:link```    |
|          |                  | [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md) |
| -X       | --excl-dir       | WILD[,WILD ..]  | ```-X obj,bin,:link```           |
|          |                  | [```:link```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-dir.md) |
|          | --regex          |
| -c       | --case-sensitive |
|          | --creation-date  |
| -k       | --keep-dir       |
|          | --no-ext         | ```incl```｜```excl```｜```only``` | ```--no-ext excl```
|          | --hidden         | ```incl```｜```excl```｜```only``` | ```--hidden excl```
|          | [--size-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-size-format.md) | INTEGER[,```commna```]｜```short```｜```+short```  | ```--size-format 12,comma```
|          |                  |                         | ```--size-format short```
|          | --count-format   | INTEGER[,```commna```]｜```short``` | ```--size-format 12,comma```
| -D       | [--date-format](https://github.com/ck-yung/dir2cs/blob/main/docs/info-date-format.md) | ```short```｜```unix```｜```unix+```｜FORMAT ｜```utc```OFFSET | ```--date-format yyyy-MMM-ddTHH:mm:ss```
|          |                  |                               | ```--D yy-MM-dd%20HH:mm ``` [See also](https://github.com/ck-yung/dir2cs/blob/main/docs/info-encode-char.md)
|          |                  |                               | ```--D s ```
|          |                  |                               | ```--date-format utc+8 ```
|          |                  |                               | ```-Z +8```
|          | --creation-date  |
|          | --total          | ```off```｜```only```｜```always``` | ```--total always```
|          | --hide           | ```date,size,count,mode,owner,last,link``` | ```--show link```
|          | --show           | ```date,size,count,mode,owner,last,link,link-size,link-date``` | ```--hide link```
|          | --link           | ```incl```｜[```only```](https://github.com/ck-yung/dir2cs/blob/main/docs/info-link-file.md) | ```--link incl```
|          | --excl-none      | | Clear all ```--excl``` and ```--excl-dir``` |
| -o       | --sort           | ```off```｜```name,size,date,ext,count,last``` [up to 2 columns] | ```-o date```
| -r       | --reverse        | ```off```｜```on``` | ```--reverse off```
|          | --take           | COUNT｜SIZE  | ```--take 10```
|          |                  |              | ```--take 500Mb```
|          | --total          | ```off```｜```only```｜```always``` | ```--total always```
|          | [--sum](https://github.com/ck-yung/dir2cs/blob/main/docs/info-sum.md) | ```ext```｜```dir```｜```+dir```｜```year``` | ```--sum ext```
|          | --end-time       | DATE-FORMAT  | ```--end-time %22TimeZone%20%22zz``` [See also](https://github.com/ck-yung/dir2cs/blob/main/docs/info-encode-char.md)
|          |                  |              | ```--end-time %22Done%20at%20%22yyyy-MMM-dd%20HH:mm%20zz```
|          | --ouput          | ```csv```    | ```--output csv```
| -Z       |                  | ```+hh:mm``` | ```-Z +08``` to Hong Kong time zone ```+08:00```

 [Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
