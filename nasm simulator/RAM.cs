public class RAM
{
    public byte[] bytes = new byte[0xB8000 + 80 * 25 * 2];

    public Endianness endianness = Endianness.LittleEndian;

    public void WriteAs(long address, long value, int bytesToWrite)
    {
        if (address < 0 || address + bytesToWrite >= bytes.Length)
        {
            throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");
        }

        byte[] valueBytes = BitConverter.GetBytes(value);

        Endianyze(valueBytes);

        if (bytesToWrite > valueBytes.Length) throw new Exception($"Failed to WriteAs to RAM due to bytesToWrite ({bytesToWrite}) is larger than value bytes ({valueBytes.Length})");


        if (endianness == Endianness.BigEndian)
        {
            for (int i = 0; i < bytesToWrite; i++)
            {
                int valueIndex = valueBytes.Length - bytesToWrite + i;
                long addressIndex = address + i;
            
                bytes[addressIndex] = valueBytes[valueIndex];
            }
        }
        else
        {
            for (int i = 0; i < bytesToWrite; i++)
            {
                int valueIndex = i;
                long addressIndex = address + i;
            
                bytes[addressIndex] = valueBytes[valueIndex];
            }
        }
    }

    public void WriteByte(long address, byte value)
    {
        bytes[address] = value;
    }

    public void Write64(long address, long value)
    {
        if (address < 0 || address + 7 >= bytes.Length) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = BitConverter.GetBytes(value);
        Endianyze(valueBytes);

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
        
        // for (int i = 0; i < bytesToRead; i++)
        // {
        //     int valueIndex = i;
        //     long addressIndex = address + 8 - bytesToRead + i;
        //
        //     valueBytes[valueIndex] = bytes[addressIndex];
        // }

        Endianyze(valueBytes);
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

        Endianyze(valueBytes);
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

    private void Endianyze(byte[] bytes)
    {
        if (endianness == Endianness.CurrentMachineEndiannes)
        {
            // Do nothing (do not reverse bytes via BitConverter)
            return;
        }

        // Current machine is Little-Endian, but simulation forced to Big-Endian
        if (BitConverter.IsLittleEndian && endianness == Endianness.BigEndian)
        {
            Array.Reverse(bytes);
        }

        // Current machine is Big-Endian, but simulation forced to Little-Endian
        if (BitConverter.IsLittleEndian == false && endianness == Endianness.LittleEndian)
        {
            Array.Reverse(bytes);
        }
    }
}
public enum Endianness
{
    BigEndian,
    LittleEndian,
    CurrentMachineEndiannes
}