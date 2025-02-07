public class MemoryStorage : IStorage
{
    public long address;
    public RAM ram;
    
    public long Read()
    {
        return ram.Read64(address);
    }

    public void Write(long value)
    {
        ram.Write64(address, value);
    }
}