class program
{
	public main(): int
	{
		Array arr = new Array()

		arr.write(0, 11)
		arr.write(1, 22)
		arr.write(2, 33)
		arr.write(3, 44)

		return arr.read(0) + arr.read(1) + arr.read(2) + arr.read(3)
	}
}

class Array 
{
	public write(int index, int value)
	{
		ptr pointer = self.to_ptr()

		pointer.shift(index * 4)
		pointer.set(value);

		return
	}
	public read(int index): int
	{
		ptr pointer = self.to_ptr()
		pointer.shift(index * 4)

		return pointer.get_int()
	}
}

---

110