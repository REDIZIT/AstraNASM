class program
{
	public main(): int
	{
	    ptr a
	    ptr b
	    
	    a.address = 123
	    
	    int value = 777
	    
	    a.set(value)
	    
	    b.address = 123
	    
	    return b.get_int()
	}
}

---

777