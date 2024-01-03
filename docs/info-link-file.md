# File Link Feature

* The following examples are based on
```
> dir2 d*

  77  Just     demo.txt
   0  Just     demo2.txt -> c:\temp\demo.txt
  77  Just     - Just         2 Users
```

## Skip link

### Do not show any link.
* Example:

```
> dir2 d* --excl :link
  77  04:45PM  demo.txt
```

or
```
> dir2 d* -x :link
  77  04:45PM  demo.txt
```

### Hide link name

* Do not show the target link name.
* Example:
```
> dir2 d*

  77  Just     demo.txt
   0  Just     demo2.txt
  77  Just     - Just         2 Users
```
## Show link only

* Example:

```
> dir2 d* --link only
462  04:44PM  demo2.txt -> c:\temp\demo.txt
```

## Show Date time and size of the link

* Example:
```
> dir2 d* --show link-size,link-date

  77  Just     demo.txt
 462  04:44PM  demo2.txt -> c:\temp\demo.txt
 539  04:44PM  - Just         2 Users
```

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
