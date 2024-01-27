# Inforamton of Environment Variable ```dir2```

## Skip config file

Config file ```dir2-cfg.txt``` will be skipped if ```--cfg-off``` is found in Envir ```dir2``` or Command-Line Options.
 
| OS  | Example of Envir Var |
| --  | -------------------- |
| Win | ```set dir2=--cfg-off```
| else| ```export dir2=--cfg-off```


## Common used

All options will be referred by Envir ```dir2```

| OS  | Example of Envir Var |
| --  | -------------------- |
| Win | ```set "dir2=--size-format 12; --date-format u; --excl *txt"```
| else| ```export "dir2=--size-format 12; --date-format u; --excl *txt"```


## The loading order of options

1. If Envir ```dir2``` and Command-Lin Parameter do *NOT* contain ```--cfg-off```, ```dir2-cfg.txt``` will be loaded at first.

2. Envir ```dir2``` is parsed secondly.

3. Commad-line options is parsed finally.


## Examples of the loading order of options

* Example 1,

| Type                        | Option |
| --------------------------- | ------ |
| Cfg File ```dir2-cfg.txt``` | ```--date-format o```
| Envir Var ```dir2```        | ```--date-format unix```
| Command-Line                | ```--date-format u```

Result: ```2023-12-31 13:08:30Z LICENSE ``` due to format ```u``` in command line.


* Example 2,

| Type                        | Option |
| --------------------------- | ------ |
| Cfg File ```dir2-cfg.txt``` | ```--date-format o```
| Envir Var ```dir2```        | ```--date-format unix```
| Command-Line                | ```--date-format``` is not defined.

Result: ```1703999310 LICENSE ``` due to format ```unix``` in envir ```dir2```.


* Example 3,

| Type                        | Option |
| --------------------------- | ------ |
| Cfg File ```dir2-cfg.txt``` | ```--date-format o```
| Envir Var ```dir2```        | ```--date-format``` is not defined.
| Command-Line                | ```--date-format``` is not defined.

Result: ```2023-12-31T13:08:30.7340976+08:00 LICENSE ``` due to format ```o```


[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
