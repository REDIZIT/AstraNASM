class program
{
	public main(): int
	{
		int a = 42

		ptr pointer = a.to_ptr()

		pointer.set(123)

		int b = pointer.get_int()

		return b
	}
}

---

123