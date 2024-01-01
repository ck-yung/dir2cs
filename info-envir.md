# Inforamton of Environment Variable ```dir2```

## Skip config file

Config file ```dir2.opt``` will be skipped if ```--cfg-off``` is found in Envir ```dir2``` or Command-Lin Options.
 
| OS  | Example |
| --  | ------- |
| Win | ```set dir2=--cfg-off```
| else| ```export dir2=--cfg-off```


## Common used

All options will be referred by Envir ```dir```

| OS  | Example |
| --  | ------- |
| Win | ```set "dir2=--size-format 12; --date-format u; --excl *txt"```
| else| ```export "dir2=--size-format 12; --date-format u; --excl *txt"```


## The loading order of options

1. If Envir ```dir``` and Command-Lin Parameter do *NOT* contain ```--cfg-off```, ```dir2.opt``` will be loaded at first.

2. Envir ```dir``` is parsed secondly.

3. Commad-line options is parsed finally.


## Examples of the loading order of options

* Example 1,

|      | Option |
| ---- | ------ |
| Cfg File     | ```--date-format o```
| Envir Var    | ```--date-format unix```
| Command-Line | ```--date-format u```

Result: ```2023-12-31 13:08:30Z LICENSE ``` due to format ```u```


* Example 2,

|      | Option |
| ---- | ------ |
| Cfg File     | ```--date-format o```
| Envir Var    | ```--date-format unix```
| Command-Line | ```--date-format``` is not defined.

Result: ```1703999310 LICENSE ``` due to format ```unix```


* Example 3,

|      | Option |
| ---- | ------ |
| Cfg File     | ```--date-format o```
| Envir Var    | ```--date-format``` is not defined.
| Command-Line | ```--date-format``` is not defined.

Result: ```2023-12-31T13:08:30.7340976+08:00 LICENSE ``` due to format ```o```

