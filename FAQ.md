## Difference bewteen ```*.txt``` and ```"*.txt"```

Shell of Linux/MacOSX provides glob feature. Therefore, if you issue

```dir2 -s *.txt```

and, if current folder contains ```info.txt``` and ```readme.txt``` only, command

```dir2 -s info.txt readme.txt```

is issued actually. So ```docs/history.txt``` will NOT be found.


Therefore, below command finds more files,

```dir2 -s "*.txt"```

**End**
