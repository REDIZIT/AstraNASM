	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
	mov [rbp-8], 42
	
; -- new Pointer
	sub rsp, 8
	
; -- pointer.address
	sub rsp, 8
	mov rax, rbp
	add rax, -16
	mov [rbp-24], rax
	sub rsp, 8
	mov [rbp-32], 160
	
; -- anon_1 = anon_2
	mov rax, [rbp-24]
	mov [rax], [rbp-32]
	sub rsp, 8
	mov [rbp-40], 1
	
; -- Set anon_3 to pointer
	mov rax, [rbp-16]
	mov [rax], [rbp-40]
	
	
; -- pointer.address
	sub rsp, 8
	mov rax, rbp
	add rax, -16
	mov [rbp-48], rax
	
	mov rbx, [rbp-48]
	mov rax, [rbx]
	mov rsp, rbp
	pop rbp
	ret