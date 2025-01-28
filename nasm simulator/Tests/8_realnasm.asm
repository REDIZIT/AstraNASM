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
	
; -- new Array
	sub rsp, 8
	
; -- arr.pointer
	sub rsp, 8
	mov rax, rbp
	add rax, -40
	mov [rbp-48], rax
	
; -- anon_3 = pointer
	mov rax, [rbp-48]
	mov [rax], [rbp-16]
	
; -- a.to_ptr
	sub rsp, 8
	mov rax, rbp
	add rax, -8
	mov [rbp-56], rax
	
; -- print anon_4
	mov rax [rbp-56]
	print [rax]
	
	
; -- arr.pointer
	sub rsp, 8
	mov rax, rbp
	add rax, -40
	mov [rbp-64], rax
	
; -- anon_5.address
	sub rsp, 8
	mov rax, [rbp-64]
	mov [rbp-72], rax
	
	mov rbx, [rbp-72]
	mov rax, [rbx]
	mov rsp, rbp
	pop rbp
	ret