class program
{
	public static main(): int
	{
		byte b = program.getByte()
		short s = program.getShort()
		int i = program.getInt()
		long l = program.getLong()

		int sum = b as int + s as int + i + l as int

		return sum
	}
	
	public static getByte(): byte
	{
	    byte b = 1
	    return b
	}
	
	public static getShort(): short
	{
	    short s = 2
	    return s
	}
	
	public static getInt(): int
	{
	    int i = 3
	    return i
	}
	
	public static getLong(): long
	{
	    long l = 4
	    return l
	}
}

---

10