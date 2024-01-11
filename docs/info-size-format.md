# Option ```--size-format``` and ```--count-format```

## ```--count-format```
| Format | Example Format | Example Result |
| ------ | -------------- | -------------- |
| ```short``` | ```--count-format short``` | ```5K``` |
| WIDTH | ```--count-format 5``` | ```5851``` |
| ```comma,```WIDTH | ```--count-format comma,5``` | ```5,851``` |


## ```--size-format```
| Format | Example Format | Example Result |
| ------ | -------------- | -------------- |
| ```"short"```  | ```--size-format short``` | ```130M``` |
| ```"+short"``` | ```--size-format +short``` | ```130M``` |
| WIDTH | ```--size-format 9``` | ```135785851``` |
| ```comma,```WIDTH | ```--size-format comma,9``` | ```135,785,851``` |

## Difference between ```--size-format short``` and  ```--size-format +short```

### ```--size-format short```
| Unit | Max Value | Result of Max Value |
| ---- | --------- | ------------------- |
| b    | <div align="right">9999</div>       | ```9999``` |
| Kb   | <div align="right">10,238,976</div>  | ```9999K``` |
| Mb   | <div align="right">10,484,711,424</div> | ```9999M``` |
| Gb   | <div align="right">10,736,344,498,176</div> | ```9999G``` |

### ```--size-format +short```
| Unit | Max Value | Result of Max Value |
| ---- | --------- | ------------------- |
| b    | <div align="right">520</div>       | ```520``` |
| Kb   | <div align="right">655,360</div>  | ```640K``` |
| Mb   | <div align="right">838,860,800</div> | ```800M``` |
| Gb   | <div align="right">10,736,344,498,176</div> | ```9999G``` |

[Back to Help Topics](https://github.com/ck-yung/dir2cs/blob/main/docs/HELP.md)
