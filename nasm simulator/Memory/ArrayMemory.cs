public class ArrayMemory : IAddressableMemory
{
    public byte[] bytes = new byte[0xB8000 + 80 * 25 * 2 + 1024];

    public bool IsOutOfBounds(long address, int bytesToWrite)
    {
        return address < 0 || address + bytesToWrite >= bytes.Length;
    }

    public void Write(long address, byte value)
    {
        if (IsOutOfBounds(address, 1))
        {
            throw new Exception($"Failed to write due to out of bounds of ArrayMemory. Address: {address}");
        }

        bytes[address] = value;
    }

    public byte Read(long address)
    {
        if (IsOutOfBounds(address, 1))
        {
            throw new Exception($"Failed to read due to out of bounds of ArrayMemory. Address: {address}");
        }
        
        return bytes[address];
    }

    public void Dump(string filepath)
    {
        File.WriteAllBytes(filepath, bytes);
        Console.WriteLine("RAM dump at: " + Path.GetFullPath(filepath));
    }
}