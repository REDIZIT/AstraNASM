class program
{
	public main(): int
	{
		ByteArray arr1 = ByteArray.FromSize()
		ByteArray arr2 = ByteArray.FromSize()

		arr1.set(0, 1 as byte)
		arr1.set(1, 2 as byte)

		arr2.set(0, 10 as byte)
		arr2.set(1, 20 as byte)

		byte v = arr1.get(0) + arr2.get(0)

		return v
	}
}

class ByteArray
{
	public static FromSize(): ByteArray
	{
		ByteArray pointer = alloc(8)
		return pointer
	}

	public get(int index): byte
	{
		ptr pointer = self.to_ptr()
		pointer.shift(index)

		byte b = pointer.get_byte()

		return b
	}
	public set(int index, byte value)
	{
		ptr pointer = self.to_ptr()
		pointer.shift(index)

		pointer.set(value)
		return
	}
}

---

11