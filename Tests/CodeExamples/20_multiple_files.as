class program
{
	public main(): int
	{
		Test test = new Test()
		
		test.a = 2
		test.b = 4
		test.c = 16
		
		return test.a + test.b + test.c
	}
}
class Test 
{
    int a
    int b
    int c
}

---

22