	call main
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	sub rsp, 8
	mov [rbp-16], 42
	
	mov [rbp-8], [rbp-16]
	
	mov rax, rbp
	add rax, -8
	mov [rbp-32], rax
	mov [rbp-24], [rbp-32]
	
	sub rsp, 8
	mov [rbp-40], 123
	
; -- Set anon_3 to pointer
	mov [rbp-24], [rbp-40]
	
; -- Get value from pointer
	mov [rbp-56], [rbp-24]
	
	mov [rbp-48], [rbp-56]
	
	
	mov rax, [rbp-48]
	mov rsp, rbp
	pop rbp
	ret