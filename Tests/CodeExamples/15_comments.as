class program
{
	public main(): int
	{
		-- Single-line comment

		-- And another sinle-line
		
		ptr p
		
		p.address = 0x42

		-- Single-line comment

		byte c = 0xff -- comment

		---
		Multi-line comment
		---
		p.set(c)

		return p.get_int()
	}
}

---

255