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
	
	sub rsp, 8
	mov [rbp-24], 11
	
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, [rbp-16] ; arg[0] = index
	push rax
	mov rax, [rbp-24] ; arg[1] = value
	push rax
	call write
	
; -- Node_FieldAccess.write()
; -- arguments generation
	sub rsp, 8
	mov [rbp-56], 1
	
	sub rsp, 8
	mov [rbp-64], 22
	
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, [rbp-56] ; arg[0] = index
	push rax
	mov rax, [rbp-64] ; arg[1] = value
	push rax
	call write
	
; -- Node_FieldAccess.write()
; -- arguments generation
	sub rsp, 8
	mov [rbp-96], 2
	
	sub rsp, 8
	mov [rbp-104], 33
	
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, [rbp-96] ; arg[0] = index
	push rax
	mov rax, [rbp-104] ; arg[1] = value
	push rax
	call write
	
; -- Node_FieldAccess.write()
; -- arguments generation
	sub rsp, 8
	mov [rbp-136], 3
	
	sub rsp, 8
	mov [rbp-144], 44
	
; -- arguments pushing
	mov rax, [rbp-8] ; self
	push rax
	mov rax, [rbp-136] ; arg[0] = index
	push rax
	mov rax, [rbp-144] ; arg[1] = value
	push rax
	call write
	
	
	sub rsp, 8
	mov [rbp-176], 1
	
	mov rax, [rbp-176]
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