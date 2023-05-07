  .386
  .model flat, stdcall
  option casemap :none

  return MACRO arg
    mov eax, arg
    ret
  ENDM

.code

LibMain proc hInstDll:DWORD, reason:DWORD, unused:DWORD
  return 1
LibMain ENDP

Init proc
  return 1
Init ENDP

GetFileOwner proc
  return 1
GetFileOwner ENDP

End LibMain
