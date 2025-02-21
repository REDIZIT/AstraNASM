class program
{
	public main(): int
	{
		Test test = new Test()
		
		test.a = 44		
		test.a = test.a + 24
		
		return test.a
	}
}
class Test 
{
    int a
}

---

68