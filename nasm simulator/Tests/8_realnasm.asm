	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	#
	mov [rbp-8], 42
	#
; -- ToPtr a
	mov rax, rbp
	add rax, -8
	mov [rbp-16], rax
	#
	sub rsp, 8
	mov [rbp-24], 123
	#
; -- Set anon_2 to pointer
	mov rax, [rbp-16]
	mov [rax], [rbp-24]
	#
; -- Get value from pointer
	mov rax, [rbp-16]
	mov [rbp-32], [rax]
	#
	
	mov rax, [rbp-8]
	mov rsp, rbp
	pop rbp
	ret