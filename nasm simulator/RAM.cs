public class RAM
{
    public byte[] bytes = new byte[0xB8000 + 80 * 25 * 2];

    public void WriteAs(long address, long value, int bytesToWrite)
    {
        if (address < 0 || address + bytesToWrite >= bytes.Length) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);

        if (bytesToWrite > valueBytes.Length) throw new Exception($"Failed to WriteAs to RAM due to bytesToWrite ({bytesToWrite}) is larger than value bytes ({valueBytes.Length})");

        //int startByte = valueBytes.Length - bytesToWrite;

        for (int i = 0; i < valueBytes.Length; i++)
        {
            bytes[address + i] = valueBytes[i];
        }
    }

    public void Write64(long address, long value)
    {
        if (address < 0 || address + 7 >= bytes.Length) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

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

    public long ReadAs(long address, int bytesToRead)
    {
        if (address < 0 || address + bytesToRead >= bytes.Length) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = new byte[bytesToRead];

        for (int i = 0; i < bytesToRead; i++)
        {
            valueBytes[i] = bytes[address + i];
        }

        if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);
        return BitConverter.ToInt64(valueBytes);
    }
    public long Read64(long address)
    {
        if (address < 0 || address + 7 >= bytes.Length) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

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