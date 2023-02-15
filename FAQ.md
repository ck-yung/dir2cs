## Difference bewteen ```*.txt``` and ```"*.txt"```

Shell of Linux/MacOSX provides glob feature. Therefore, if you issue

```dir2 -s *.txt```

and, if current folder contains ```info.txt``` and ```readme.txt``` only, command

```dir2 -s info.txt readme.txt```

is issueed actually. So ```docs/history.txt``` will NOT be found.


Therefore, below command find more files,

```dir2 -s "*.txt"```


## Command ```dir2 typehex\*cs zip2\*cs``` shows error on Windows!
Command ```dir2 typehex\*cs``` lists ```*cs``` on ```typehex```, but it CANNOT mix to a different directory.

## Command ```dir2 -s typehex/*cs zip2/*cs``` CANNOT visits sub-dir on Unix/MacOS!
The command lists files by  glob feature. And sub-dir feature must be applied to a single directoy.



