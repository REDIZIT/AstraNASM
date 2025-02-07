public class RegistryStorage : IStorage
{
    public Reg64 reg;

    public long Read()
    {
        return reg.value;
    }

    public void Write(long value)
    {
        reg.value = value;
    }
}