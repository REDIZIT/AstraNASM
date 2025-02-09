class program
{
	public main(): int
	{
		ByteArray arr1 = ByteArray.FromSize(16)
		ByteArray arr2 = ByteArray.FromSize(8)

		arr1.set(0, 1)
		arr1.set(1, 2)

		arr2.set(0, 10)
		arr2.set(1, 20)

		arr1.set(15, 0xaa) -- 15 is the last element of arr1
		arr1.set(16, 0xbb) -- overwrite arr2.length 1st byte
		arr1.set(17, 0xcc) -- overwrite arr2.length 2nd byte
		arr1.set(18, 0xdd) -- overwrite arr2.length 3rd byte
		arr1.set(19, 0xee) -- overwrite arr2.length 4th byte
		arr1.set(20, 0xff) -- overwrite arr2[0] from 10 to 0xff

		return arr1.get(0) + arr2.get(0)
	}
}

class ByteArray
{
	int length

	public static FromSize(int length): ByteArray
	{
		ByteArray instance = alloc(4 + length)
		instance.length = length
		return instance
	}

	public get(long index): byte
	{
	    if (index >= self.length) -- silently prevent out of bounds
        {
            return 0;
        }

		ptr pointer = self.to_ptr()
		pointer.shift(4 + index)

		byte b = pointer.get_byte()

		return b
	}
	public set(long index, byte value)
	{
	    if (index >= self.length) -- silently prevent out of bounds
        {
            return;
        }
	
		ptr pointer = self.to_ptr()
		pointer.shift(4 + index)

		pointer.set(value)
		return
	}
}

---

11