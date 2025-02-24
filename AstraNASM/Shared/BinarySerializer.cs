using System.Reflection;
using FieldInfo = System.Reflection.FieldInfo;

namespace Astra.Shared;

public static class BinarySerializer
{
    public static List<byte> Serialize<T>(T value)
    {
        BinaryFile file = new();
        
        Serialize(value, file);
    
        return file.bytes;
    }

    public static T Deserialize<T>(BinaryFile file)
    {
        return (T)Deserialize(typeof(T), file);
    }
    public static object Deserialize(Type type, BinaryFile file)
    {
        object o = Activator.CreateInstance(type);

        foreach (FieldInfo f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            Type t = f.FieldType;
            object value = DeserializeValue(t, file);
            f.SetValue(o, value);
        }

        return o;
    }

    private static void Serialize(object o, BinaryFile file)
    {
        foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            Type t = f.FieldType;
            object value = f.GetValue(o);
            SerializeValue(value, file);
        }
    }

    private static void SerializeValue(object value, BinaryFile file)
    {
        if (value is string str)
        {
            file.Add(str);
        }
        else if (value is byte b)
        {
            file.Add(b);
        }
        else if (value is short s)
        {
            file.AddShort(s);
        }
        else if (value is int integer)
        {
            file.AddInt(integer);
        }
        else if (value is uint ui)
        {
            file.AddInt((int)ui);
        }
        else if (value is long l)
        {
            file.AddLong(l);
        }
        else if (value is bool bl)
        {
            file.Add(bl ? (byte)1 : (byte)0);
        }
        else if (value is Array array)
        {
            file.AddInt(array.Length);
            Type elementType = array.GetType().GetElementType();
            
            if (elementType.IsPrimitive)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    SerializeValue(element, file);
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    Serialize(element, file);
                }
            }
        }
        else
        {
            Serialize(value, file);
        }
    }

    private static object DeserializeValue(Type t, BinaryFile file)
    {
        if (t == typeof(string))
        {
            return file.NextString();
        }
        else if (t == typeof(byte))
        {
            return file.Next();
        }
        else if (t == typeof(short))
        {
            return file.NextShort();
        }
        else if (t == typeof(int))
        {
            return file.NextInt();
        }
        else if (t == typeof(uint))
        {
            return (uint)file.NextInt();
        }
        else if (t == typeof(long))
        {
            return file.NextLong();
        }
        else if (t == typeof(bool))
        {
            return file.Next() > 0;
        }
        else if (t.IsArray)
        {
            int length = file.NextInt();

            Type elementType = t.GetElementType();
            Array array = Array.CreateInstance(elementType, length);

            if (elementType.IsPrimitive)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object element = DeserializeValue(elementType, file);
                    array.SetValue(element, i);
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object element = Deserialize(elementType, file);
                    array.SetValue(element, i);
                }
            }
                
            return array;
        }
        else
        {
            return Deserialize(t, file);
        }
    }
}