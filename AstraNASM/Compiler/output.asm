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
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, 0 ; arg[0] = index
	push rax
	mov rax, 11 ; arg[1] = value
	push rax
	call write
	add rsp, 24
	
; -- Node_FieldAccess.write()
; -- arguments generation
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, 1 ; arg[0] = index
	push rax
	mov rax, 22 ; arg[1] = value
	push rax
	call write
	add rsp, 24
	
; -- Node_FieldAccess.write()
; -- arguments generation
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, 2 ; arg[0] = index
	push rax
	mov rax, 33 ; arg[1] = value
	push rax
	call write
	add rsp, 24
	
; -- Node_FieldAccess.write()
; -- arguments generation
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, 3 ; arg[0] = index
	push rax
	mov rax, 44 ; arg[1] = value
	push rax
	call write
	add rsp, 24
	
	
	sub rsp, 8
	mov [rbp-16], 1
	
	mov rax, [rbp-16]
	mov rsp, rbp
	pop rbp
	ret
	
	
	
write:
	push rbp
	mov rbp, rsp
	
; -- ToPtr self (heap data)
	mov rax, rbp
	add rax, 32
	mov [rbp-8], [rax]
	
	sub rsp, 8
	mov [rbp-16], 8
	
	mov rax, [rbp+24]
	mov rbx, [rbp-16]
	mul rax, rbx
	mov [rbp-24], rax
	
; -- Shift pointer pointer by anon_3
	mov rax, [rbp-8]
	mov rbx, [rbp-24]
	add rax, rbx
	mov [rbp-8], rax
	
; -- Set value to pointer
	mov rax, [rbp-8]
	mov [rax], [rbp+16]
	
	
	mov rsp, rbp
	pop rbp
	ret