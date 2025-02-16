using System.Globalization;

namespace Astra.Compilation;

public static class Utils
{
    public static string ClampRegister(string nasmType)
    {
        if (nasmType == "byte") return "dl";
        if (nasmType == "word") return "dx";
        if (nasmType == "dword") return "edx";
        if (nasmType == "qword") return "rdx";

        throw new Exception($"Failed to clamp register for nasm type '{nasmType}'");
    }
    
    public static string ClampRegister(string nasmType, string regName)
    {
        if (regName.Length != 3) throw new Exception($"Failed to clamp register '{regName}' due to it is not size of 3");

        if (nasmType == "byte") return regName[1] + "l";        // al,  bl,  cl,  dl
        if (nasmType == "word") return regName[1] + "x";        // ax,  bx,  cx,  dx
        if (nasmType == "dword") return "e" + regName[1] + "x"; // eax, ebx, ecx, edx
        if (nasmType == "qword") return "r" + regName[1] + "x"; // rax, rbx, rcx, rdx

        throw new Exception($"Failed to clamp register for nasm type '{nasmType}'");
    }

    public static string GetNASMType(TypeInfo t)
    {
        if (t == PrimitiveTypes.BYTE) return "byte";
        if (t == PrimitiveTypes.SHORT) return "word";
        if (t == PrimitiveTypes.INT || t == PrimitiveTypes.PTR) return "dword";
        if (t == PrimitiveTypes.LONG || t == PrimitiveTypes.BOOL) return "qword";

        if (t.isStruct == false) return "qword";

        throw new Exception($"Failed to get nasm type for type '{t}'");
    }

    public static byte GetSizeInBytes(TypeInfo t)
    {
        switch (GetNASMType(t))
        {
            case "byte": return 1;
            case "word": return 2;
            case "dword": return 4;
            case "qword": return 8;
            default: throw new Exception($"Failed to get size in bytes for type '{t}'");
        }
    }

    public static bool AreSameSize(params Variable[] vars)
    {
        string type = GetNASMType(vars[0].type);

        foreach (Variable var in vars)
        {
            if (GetNASMType(var.type) != type) return false;
        }

        return true;
    }

    public static void AssertSameSize(params Variable[] vars)
    {
        if (AreSameSize(vars) == false)
        {
            throw new Exception("Assert failed. Variables have different sizes: " + string.Join(", ", vars.Select(v => v.name + " (" + Utils.GetSizeInBytes(v.type) + ")")));
        }
    }

    public static void AssertSameOrLessSize(Variable result, params Variable[] vars)
    {
        byte maxSize = GetSizeInBytes(result.type);
        foreach (Variable var in vars)
        {
            byte varSize = GetSizeInBytes(var.type);
            if (varSize > maxSize)
            {
                throw new Exception($"Assert failed. Variable '{var.name}' is larger than '{result.name}'. Variables: " + string.Join(", ", vars.Select(v => v.name + " (" + Utils.GetSizeInBytes(v.type) + ")")));
            }
        }
    }

    public static byte[] ParseNumber(string str, byte sizeInBytes)
    {
        NumberStyles style = NumberStyles.Integer;
        if (str.StartsWith("0b"))
        {
            style = NumberStyles.BinaryNumber;
            str = str.Substring(2, str.Length - 2);
        }
        else if (str.StartsWith("0x"))
        {
            style = NumberStyles.HexNumber;
            str = str.Substring(2, str.Length - 2);
        }
        
        
        byte[] bytes;
        if (sizeInBytes == 1)
        {
            bytes = new byte[1] { byte.Parse(str, style) };
        }
        else if (sizeInBytes == 2)
        {
            bytes = BitConverter.GetBytes(short.Parse(str, style));
        }
        else if (sizeInBytes == 4)
        {
            bytes = BitConverter.GetBytes(int.Parse(str, style));
        }
        else
        {
            bytes = BitConverter.GetBytes(long.Parse(str, style));
        }

        return bytes;
    }
}