	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
; -- new Array
	sub rsp, 8
	
; -- Node_FieldAccess.write
	sub rsp, 8
	mov [rbp-16], 0
	
	mov rax, [rbp-16] ; arg[0] = index
	push rax
	call write
	
	
	sub rsp, 8
	mov [rbp-32], 1
	
	mov rax, [rbp-32]
	mov rsp, rbp
	pop rbp
	ret
	
	
	
write:
	push rbp
	mov rbp, rsp
	
; -- self.to_ptr
	sub rsp, 8
	mov rax, rbp
	add rax, 16
	mov [rbp-8], rax
	
	sub rsp, 8
	mov [rbp-16], 123
	
	sub rsp, 8
	mov [rbp-24], 1
	
	mov rax, [rbp-16]
	mov rbx, [rbp-24]
	add rax, rbx
	mov [rbp-32], rax
	
; -- Set anon_4 to pointer
	mov rax, [rbp-8]
	mov [rax], [rbp-32]
	
	
	mov rsp, rbp
	pop rbp
	ret