class program
{
	public main(): int
	{
		Array arr = new Array()

		arr.write(0)

		return 1
	}
}

class Array 
{
	public write(int index)
	{
		ptr pointer = self.to_ptr()

		pointer.set(123)

		pointer.shift(8)
		pointer.set(124)

		return
	}
}

---

1