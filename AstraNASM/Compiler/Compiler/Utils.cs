﻿namespace Astra.Compilation;

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
        if (t == PrimitiveTypes.INT) return "dword";
        if (t == PrimitiveTypes.LONG || t == PrimitiveTypes.PTR) return "qword";

        throw new Exception($"Failed to get nasm type for type '{t}'");
    }
}