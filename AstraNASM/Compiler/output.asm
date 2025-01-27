	call main
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	sub rsp, 8
	mov [rbp-16], 42
	
	mov [rbp-8], [rbp-16]
	
	sub rsp, 8
	mov [rbp-32], 3
	
	mov [rbp-24], [rbp-32]
	
	sub rsp, 8
	mov [rbp-48], 2
	
	mov rax, [rbp-8]
	mov rbx, [rbp-24]
	mul rax, rbx
	mov [rbp-56], rax
	
	mov rax, [rbp-48]
	mov rbx, [rbp-56]
	add rax, rbx
	mov [rbp-64], rax
	
	mov [rbp-40], [rbp-64]
	
	
	mov rax, [rbp-40]
	mov rsp, rbp
	pop rbp
	ret