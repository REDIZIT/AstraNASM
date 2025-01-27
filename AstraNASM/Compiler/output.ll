;
; Structs
;
%program = type {  }


;
; Methods
;
define i32 @main()
{
	%ptr_0_int = alloca i32
	store i32 2, ptr %ptr_0_int
	
	%a = alloca i32
	
	; -- ptr %a = ptr %ptr_0_int
	%tmp_0_ptr = load i32, ptr %ptr_0_int
	store i32 %tmp_0_ptr, ptr %a
	
	; -- ptr_name = %ptr_1_ptr for a
	%ptr_1_ptr = alloca i32*
	store i32* %a, i32** %ptr_1_ptr
	
	%pointer = alloca ptr
	
	; -- ptr %pointer = ptr %ptr_1_ptr
	%tmp_1_ptr = load ptr, ptr %ptr_1_ptr
	store ptr %tmp_1_ptr, ptr %pointer
	
	%ptr_2_int = alloca i32
	store i32 1, ptr %ptr_2_int
	
	; -- Shift pointer pointer by %ptr_2_int
	%tmp_2_int = load i32, ptr %ptr_2_int
	%tmp_3_ptr = load i32, i32* %pointer
	%tmp_4_int = add i32 %tmp_2_int, %tmp_3_ptr
	store i32 %tmp_4_int, i32* %pointer
	
	%ptr_3_int = alloca i32
	store i32 512, ptr %ptr_3_int
	
	; -- Set %ptr_3_int to pointer
	%tmp_5_int = load i32, ptr %ptr_3_int
	%tmp_6_ptr = load i32*, i32* %pointer
	store i32 %tmp_5_int, i32* %tmp_6_ptr
	
	%tmp_7_int = load i32, ptr %a
	ret i32 %tmp_7_int
}



define void @program__ctor(ptr %self)
{
	ret void
}