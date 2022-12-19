# Dir2
**v2.0.0**

## Syntax:
```
dir2 [DIR] [WILD ..] [OPT ..]
```

### Example:
```
dir2 bin -s *.exe *.dll --within 10m
```

## Syntax:
```
dir2 DIR/WILD [OPT ..]
```

### Example:
```
dir2 obj/*.dll -s
```

## Syntax:
```
dir2 --help TOPICS
```

### Example:
```
dir2 --help sort
```

## Syntax to list help topics:
```
dir2 -? ?
dir2 --help help
```

## Options:

| Shortcut | Option           | Example                 | Shortcut Example |
| -------- | ------           | -------                 | ---------------- |
| -?       | --help           |                         |                  |
| -? ?     | --help help      |                         |                  |
| -? TOPICS| --help TOPICS    | --help sort             | -? sort          | 
|          | --cfg-off        |                         |                  |
|          | --utf8           |                         |                  |
|          | --within         | --within 10m            |                  |
|          |                  | --within 2hr            |                  |
|          | --not-within     | --not-within 20k        |                  |
|          |                  | --not-within 3day       |                  |
|          | --regex          |                         |                  |
|          | --case-sensitive |                         |                  |
|          | --create-date    |                         |                  |
|          | --relative       |                         |                  |
|          | --count-comma    |                         |                  |
|          | --no-ext         | --no-ext only           |                  |
|          | --hidden         | --hidden only           |                  |
|          | --size-format    | --size-format short     |                  |
|          | --count-width    | --count-width 10        |                  |
|          | --date-format    | --date-format MM.dd     |                  |
|          | --total          | --total only            |                  |
|          | --hide           | --hide date             |                  |
| -o       | --sort           | --sort size             | -o date          |
|          | --take           | --take 100m             |                  |
|          | --dir            | --dir sub               |                  |
|          | --sum            | --sum ext               |                  |
| -x       | --excl-file      | --excl-file \*.pdb      | -x \*.pdb        |
| -X       | --excl-dir       | --excl-dir obj          | -X obj           |

## Options will be removed in later version.
| Shortcut | Option           | Example                 | Shortcut Example |
| -------- | ------           | -------                 | ---------------- |
|          | --size-beyond    | --size-beyond 9k        |                  |
|          | --size-within    | --size-within 10m       |                  |
|          | --date-beyond    | --date-beyond 3h        |                  |
|          | --date-within    | --date-within 9d        |                  |


## Daily Shortcut:

| Shortcut | Stand for              | Description                    |
| -------- | ---------              | -----------                    |
| -s       | --dir=sub              | Recursively sub-directory      |
| -f       | --dir=off              | List file only                 |
| -d       | --dir=only             | List dir name only             |
| -T       | --dir=tree             | List dir tree                  |
| -c       | --case-sensitive       |                                |
| -b       | --total=off            | List filename (with path) only |
|          | --hide=size,date,count |                                |
| -t       | --total=only           | Display total line only        |

# Setup by Config File & Environment
 
Config file "dir2.opt" will be referred before envir var "dir2".

Environment "dir2" will be referred before command line parameters.

See:
```
	dir2 -? cfg
	dir2 -? envr | more
```

Yung, Chun Kau

<yung.chun.kau@gmail.com>

2023 ???
