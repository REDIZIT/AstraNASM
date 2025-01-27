public class RAM
{
    public byte[] bytes = new byte[256];

    public void Write64(long address, long value)
    {
        byte[] valueBytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);

        bytes[address + 0] = valueBytes[0];
        bytes[address + 1] = valueBytes[1];
        bytes[address + 2] = valueBytes[2];
        bytes[address + 3] = valueBytes[3];
        bytes[address + 4] = valueBytes[4];
        bytes[address + 5] = valueBytes[5];
        bytes[address + 6] = valueBytes[6];
        bytes[address + 7] = valueBytes[7];
    }
    public long Read64(long address)
    {
        byte[] valueBytes = new byte[8];

        valueBytes[0] = bytes[address + 0];
        valueBytes[1] = bytes[address + 1];
        valueBytes[2] = bytes[address + 2];
        valueBytes[3] = bytes[address + 3];
        valueBytes[4] = bytes[address + 4];
        valueBytes[5] = bytes[address + 5];
        valueBytes[6] = bytes[address + 6];
        valueBytes[7] = bytes[address + 7];

        if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);
        return BitConverter.ToInt64(valueBytes);
    }

    public byte[] ReadBytes(long address, int count)
    {
        byte[] slice = new byte[count];
        for (int i = 0; i < count; i++)
        {
            slice[i] = bytes[address + i];
        }
        return slice;
    }

    public void Dump(string filepath)
    {
        File.WriteAllBytes(filepath, bytes);
    }
}