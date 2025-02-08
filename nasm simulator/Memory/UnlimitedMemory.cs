public class UnlimitedMemory : IAddressableMemory
{
    public Dictionary<long, byte> bytes = new();

    public bool IsOutOfBounds(long address, int bytesToWrite)
    {
        return false;
    }

    public void Write(long address, byte value)
    {
        if (bytes.ContainsKey(address) == false)
        {
            bytes.Add(address, value);
        }
        else
        {
            bytes[address] = value;
        }
    }

    public byte Read(long address)
    {
        if (bytes.TryGetValue(address, out byte value))
        {
            return value;
        }

        return 0;
    }

    public void Dump(string filepath)
    {
        Console.Error.WriteLine("Dump is not available for UnlimitedMemory");
    }
}