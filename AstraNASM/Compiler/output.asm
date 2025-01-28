	call main
	mov 0x00, rax
	exit
	
	
	
main:
	push rbp
	mov rbp, rsp
	
; -- new Pointer
	sub rsp, 8
	
; -- pointer.address
	sub rsp, 8
	mov rax, rbp
	add rax, -8
	mov [rbp-16], rax
	sub rsp, 8
	mov [rbp-24], 160
	
; -- anon_1 = anon_2
	mov rax, [rbp-16]
	mov [rax], [rbp-24]
	
; -- new Array
	sub rsp, 8
	
; -- arr.pointer
	sub rsp, 8
	mov rax, rbp
	add rax, -32
	mov [rbp-40], rax
	
; -- anon_3 = pointer
	mov rax, [rbp-40]
	mov [rax], [rbp-8]
	
; -- Node_FieldAccess.write
	sub rsp, 8
	mov [rbp-48], 777
	
	mov rax, [rbp-48]
	push rax
	call write
	
	
	sub rsp, 8
	mov [rbp-64], 1
	
	mov rax, [rbp-64]
	mov rsp, rbp
	pop rbp
	ret
	
	
	
write:
	push rbp
	mov rbp, rsp
	
; -- print index
	mov rax, [rbp+16]
	print [rax]
	
	
	mov rsp, rbp
	pop rbp
	ret