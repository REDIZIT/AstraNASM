class program
{
	public main()
	{
		string str = "Hello from Astra"
		
		ptr str_pointer = str.to_ptr()
		ptr vga_pointer
		
		vga_pointer.address = 0xB8000
		
		byte c = 0x2f
		
		for (int i; i < str.length(); i = i + 1)
		{
		    byte b = str.get(i)
		
		    vga_pointer.set(b)
		    vga_pointer.shift(1)
		    
		    vga_pointer.set(c)
		    vga_pointer.shift(1)
		}

		return
	}
}

---

vga = "Hello from Astra"