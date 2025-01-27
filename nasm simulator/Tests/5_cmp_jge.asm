mov rax, 7
mov rbx, 2

; rax >= rbx ?
cmp rax, rbx
jge if_true:
mov rdx 0 ; false
jmp if_end:
if_true:
mov rdx 1 ; true
if_end:

---
rax = 7
rbx = 2
rdx = 1