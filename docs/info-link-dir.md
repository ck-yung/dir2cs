# Dir Link Feature

* The following examples are based on
```
> dir2 d*
DIR 2022 Nov Default
DIR 2019 Dec Default User -> C:\Users\Default
2 dir are found.
```

## Skip link

### Do not show any link.
* Example:

```
> dir2 d* --excl-dir :link
DIR 2022 Nov Default
```

or
```
> dir2 d* -X :link
DIR 2022 Nov Default
```

### Hide link name

* Do not show the target link name.
* Example:
```
> dir2 d* --hide link
DIR 2022 Nov Default
DIR 2019 Dec Default User
2 dir are found.
```
## Show link only

* Example:

```
> dir2 d* --dir only-link
DIR 2019 Dec Default User -> C:\Users\Default
```

## Show Date time of the link

* Example:
```
> dir2 d*
DIR 2022 Nov Default
DIR 2022 Nov Default User -> C:\Users\Default
2 dir are found.
```

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
