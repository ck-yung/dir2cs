# Why closing marks are required?

## Difference bewteen ```*.txt``` and ```"*.txt"```

Shell of Linux/MacOSX provides a ```glob``` feature. Therefore, if you issue

```dir2 -s *.txt```

and, if current folder contains ```info.txt``` and ```readme.txt``` only, command

```dir2 -s info.txt readme.txt```

is issued actually. So ```docs/history.txt``` will NOT be found.


Therefore, below command finds more files,

```dir2 -s "*.txt"```

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
