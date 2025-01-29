	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	mov [rbp-8], 2
	
	sub rsp, 8
	mov [rbp-16], 2
	
	sub rsp, 8
	mov rax, [rbp-8]
	mov rbx, [rbp-16]
	cmp rax, rbx
	mov rax, 0
	setne al
	mov [rbp-24], rax
	
; -- if anon_2
	mov rax, [rbp-24]
	cmp rax, 0
	jle if_false
	
	
	sub rsp, 8
	mov [rbp-32], 10
	
	mov rax, [rbp-32]
	mov [rbp+24], rax
	mov rsp, rbp
	pop rbp
	ret
	jmp if_end
if_false:
	
	
	sub rsp, 8
	mov [rbp-40], 9
	
	mov rax, [rbp-40]
	mov [rbp+24], rax
	mov rsp, rbp
	pop rbp
	ret
if_end:
	
	
	sub rsp, 8
	mov [rbp-48], 1
	
	mov rax, [rbp-48]
	mov [rbp+24], rax
	mov rsp, rbp
	pop rbp
	ret