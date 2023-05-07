
.code
    public _DllMainCRTStartup
    public Init
    public GetFileOwner

_DllMainCRTStartup proc
    mov eax, 1
    ret
_DllMainCRTStartup endp

Init proc
    mov eax, 1
    ret
Init endp

GetFileOwner proc maxLen:DWORD, szPath:DWORD, szOwner:DWORD
    mov eax, 1
    ret
GetFileOwner endp

end
