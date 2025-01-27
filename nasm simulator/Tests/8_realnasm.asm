	call main
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	
	sub rsp, 8
	mov [rbp-8], 42
	
	mov rax, 
	mov rsp, rbp
	pop rbp
	ret