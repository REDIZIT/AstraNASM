using System.Text;

namespace Astra.Shared;

public class BinaryFile
{
    public List<byte> bytes;
    private int current;

    public BinaryFile()
    {
        Load(new());
    }
    public BinaryFile(List<byte> bytes)
    {
        Load(bytes);
    }

    public void Load(List<byte> bytes)
    {
        this.bytes = bytes;
        current = 0;
    }

    public void Add(byte b)
    {
        bytes.Add(b);
    }
    public void AddShort(short v)
    {
        bytes.AddRange(BitConverter.GetBytes(v));
    }
    public void AddInt(int v)
    {
        bytes.AddRange(BitConverter.GetBytes(v));
    }
    public void AddLong(long v)
    {
        bytes.AddRange(BitConverter.GetBytes(v));
    }

    public byte Next()
    {
        byte b = bytes[current];
        current++;
        return b;
    }
    public short NextShort()
    {
        short value = BitConverter.ToInt16(NextRange(sizeof(short)));
        return value;
    }
    public int NextInt()
    {
        int value = BitConverter.ToInt32(NextRange(sizeof(int)));
        return value;
    }
    public long NextLong()
    {
        long value = BitConverter.ToInt64(NextRange(sizeof(long)));
        return value;
    }
    public void AddRange(IEnumerable<byte> b)
    {
        bytes.AddRange(b);
    }

    public byte[] NextRange(int count)
    {
        if (current + count - 1 >= bytes.Count)
        {
            throw new Exception($"Failed to get bytes range (current..{current + count}) due to out of bounds of bytes ({bytes.Count})");
        }
        
        byte[] b = new byte[count];
        for (int i = 0; i < count; i++)
        {
            b[i] = bytes[current + i];
        }

        current += count;
        return b;
    }

    public void Add(string str)
    {
        byte[] b = Encoding.ASCII.GetBytes(str);
        AddInt(b.Length);
        AddRange(b);
    }

    public string NextString()
    {
        int length = NextInt();
        byte[] b = NextRange(length);
        return Encoding.ASCII.GetString(b);
    }

    public void Add<T>(IEnumerable<T> list, Action<T> encodeFunc)
    {
        AddInt(list.Count());
        foreach (T element in list)
        {
            encodeFunc(element);
        }
    }
    public List<T> NextList<T>(Func<T> decodeFunc)
    {
        int count = NextInt();
        List<T> ls = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            ls.Add(decodeFunc());
        }

        return ls;
    }
}