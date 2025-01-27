public class Reg64
{
    public long value;

    public void Set64(long value)
    {
        this.value = value;
    }
    public void Set32(int value)
    {
        byte[] regBytes = BitConverter.GetBytes(this.value);
        byte[] valueBytes = BitConverter.GetBytes(value);

        regBytes[0] = valueBytes[0];
        regBytes[1] = valueBytes[1];
        regBytes[2] = valueBytes[2];
        regBytes[3] = valueBytes[3];

        this.value = BitConverter.ToInt64(regBytes);
    }
    public void Set16(short value)
    {
        byte[] regBytes = BitConverter.GetBytes(this.value);
        byte[] valueBytes = BitConverter.GetBytes(value);

        regBytes[0] = valueBytes[0];
        regBytes[1] = valueBytes[1];

        this.value = BitConverter.ToInt64(regBytes);
    }
    public void Set8(byte value, bool isHigh)
    {
        byte[] bytes = BitConverter.GetBytes(this.value);
        bytes[isHigh ? 1 : 0] = value;
        this.value = BitConverter.ToInt64(bytes);
    }

    public long Get64()
    {
        return value;
    }
    public int Get32()
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return BitConverter.ToInt32(bytes.AsSpan(0, 4));
    }
    public short Get16()
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return BitConverter.ToInt16(bytes.AsSpan(0, 2));
    }
    public byte Get8(bool isHigh)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return bytes[isHigh ? 1 : 0];
    }

    public string ToString(int bits)
    {
        string bin = value.ToString($"b{bits}");
        for (int i = bits; i > 0; i -= 8)
        {
            bin = bin.Insert(i, " ");
        }
        return $"{value} (0x{value.ToString($"x{bits / 16}")}, {bin})";
    }
}