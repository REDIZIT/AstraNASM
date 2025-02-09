public class RAM
{
    public IAddressableMemory memory = new ArrayMemory();

    public Endianness endianness = Endianness.LittleEndian;

    public void WriteAs(long address, long value, int bytesToWrite)
    {
        if (memory.IsOutOfBounds(address, bytesToWrite))
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
                
                memory.Write(addressIndex, valueBytes[valueIndex]);
            }
        }
        else
        {
            for (int i = 0; i < bytesToWrite; i++)
            {
                int valueIndex = i;
                long addressIndex = address + i;
            
                memory.Write(addressIndex, valueBytes[valueIndex]);
            }
        }
    }

    public void WriteByte(long address, byte value)
    {
        memory.Write(address, value);
    }

    public void Write64(long address, long value)
    {
        //if (address < 0 || address + 7 >= bytes.Count) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = BitConverter.GetBytes(value);
        Endianyze(valueBytes);

        memory.Write(address + 0, valueBytes[0]);
        memory.Write(address + 1, valueBytes[1]);
        memory.Write(address + 2, valueBytes[2]);
        memory.Write(address + 3, valueBytes[3]);
        memory.Write(address + 4, valueBytes[4]);
        memory.Write(address + 5, valueBytes[5]);
        memory.Write(address + 6, valueBytes[6]);
        memory.Write(address + 7, valueBytes[7]);
    }

    public long ReadAs(long address, int bytesToRead)
    {
        //if (address < 0 || address + bytesToRead >= bytes.Count) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = new byte[8]; // 8 bytes for long

        for (int i = 0; i < bytesToRead; i++)
        {
            valueBytes[i] = memory.Read(address + i);
        }
        
        Endianyze(valueBytes);
        return BitConverter.ToInt64(valueBytes);
    }
    public long Read64(long address)
    {
        //if (address < 0 || address + 7 >= bytes.Count) throw new Exception($"Address 0x{address.ToString("X")} is out of RAM bounds.");

        byte[] valueBytes = new byte[8];

        valueBytes[0] = memory.Read(address + 0);
        valueBytes[1] = memory.Read(address + 1);
        valueBytes[2] = memory.Read(address + 2);
        valueBytes[3] = memory.Read(address + 3);
        valueBytes[4] = memory.Read(address + 4);
        valueBytes[5] = memory.Read(address + 5);
        valueBytes[6] = memory.Read(address + 6);
        valueBytes[7] = memory.Read(address + 7);

        Endianyze(valueBytes);
        return BitConverter.ToInt64(valueBytes);
    }

    public byte ReadByte(long address)
    {
        return memory.Read(address);
    }

    public byte[] ReadBytes(long address, int count)
    {
        byte[] slice = new byte[count];
        for (int i = 0; i < count; i++)
        {
            slice[i] = memory.Read(address + i);
        }
        return slice;
    }
    
    public void Dump(string filepath)
    {
        memory.Dump(filepath);
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