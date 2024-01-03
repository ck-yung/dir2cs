### Sum-up by File Extension, Dir Name, and, Year

| Option              | Description                                 |
| ------              | -----------                                 |
| ```--sum ext```     | Sum up by file extension (current dir only) |
| ```-s --sum ext```  | Sum up by file extension from all sub dir.  |
| ```-s --sum dir```  | Sum up by dir.                              |
| ```-s --sum +dir``` | Sum up by dir including "no file found".    |
| ```-s --sum year``` | Sum up by year.                             |

### Sum-up Dir Usage
List all dir which has no any ```.cs``` file.
```
dir2 *.cs -s --sum +dir -o count --take 1
```

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
