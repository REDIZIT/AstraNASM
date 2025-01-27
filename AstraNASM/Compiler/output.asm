	call main
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	
	sub rsp, 8
	mov [rbp-8], 2
	
	sub rsp, 8
	mov [rbp-16], 2
	
	sub rsp, 8
	mov [rbp-24], 3
	
	mov rax, [rbp-16]
	mov rbx, [rbp-24]
	add rax, rbx
	mov [rbp-32], rax
	
	mov rax, [rbp-8]
	mov rbx, [rbp-32]
	mul rax, rbx
	mov [rbp-40], rax
	
	mov rax, [rbp-40]
	mov rsp, rbp
	pop rbp
	ret