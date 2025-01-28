	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
; -- new Array
	sub rsp, 8
; -- heap alloc
	mov [rbp-8], 0x110
	mov rax, [0x100]
	add rax, 1
	mov [0x100], rax
	
; -- Node_FieldAccess.write()
; -- arguments generation
	sub rsp, 8
	mov [rbp-16], 0
	
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, [rbp-16] ; arg[0] = index
	push rax
	call write
	
	
	sub rsp, 8
	mov [rbp-40], 1
	
	mov rax, [rbp-40]
	mov rsp, rbp
	pop rbp
	ret
	
	
	
write:
	push rbp
	mov rbp, rsp
	
; -- ToPtr self (heap data)
	mov rax, rbp
	add rax, 24
	mov [rbp-8], [rax]
	
	sub rsp, 8
	mov [rbp-16], 123
	
; -- Set anon_2 to pointer
	mov rax, [rbp-8]
	mov [rax], [rbp-16]
	
	sub rsp, 8
	mov [rbp-24], 8
	
; -- Shift pointer pointer by anon_3
	mov rax, [rbp-8]
	mov rbx, [rbp-24]
	add rax, rbx
	mov [rbp-8], rax
	
	sub rsp, 8
	mov [rbp-32], 124
	
; -- Set anon_4 to pointer
	mov rax, [rbp-8]
	mov [rax], [rbp-32]
	
	
	mov rsp, rbp
	pop rbp
	ret