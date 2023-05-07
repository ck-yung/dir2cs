    AREA |.text|, CODE, READONLY, ALIGN=4, CODEALIGN
    EXPORT _DllMainCRTStartup
    EXPORT Init
    EXPORT GetFileOwner

_DllMainCRTStartup PROC
    mov w0, 1
    ret
    ENDP

Init PROC
    mov w0, 1
    ret
    ENDP

GetFileOwner PROC
    mov w0, 1
    ret
    ENDP

    END

