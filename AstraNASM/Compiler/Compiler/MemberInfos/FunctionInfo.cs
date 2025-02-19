﻿namespace Astra.Compilation;

public class FunctionInfo
{
    public string name;
    public TypeInfo owner;
    public bool isStatic;

    public List<FieldInfo> arguments = new();
    public List<TypeInfo> returns = new();

    public string GetCombinedName()
    {
        return owner.name + "." + name;
    }
}