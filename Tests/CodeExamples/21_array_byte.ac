class program
{
	public main()
	{
		ByteArray arr = new ByteArray()
		
		arr.set(0, 0x11)
		arr.set(1, 0x22)
		arr.set(2, 0x33)
		
		return
	}
}
class ByteArray
{
    public set(int index, byte value)
    {
        ptr pointer = self.to_ptr()
        
        pointer.address = pointer.address + index
        
        pointer.set(value)
    }
}

---

0