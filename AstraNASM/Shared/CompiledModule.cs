namespace Astra.Shared;

public class CompiledModule
{
    public MetaTable metaTable;
    public ManagedCode managedCode;
}

public class MetaTable
{
    public TypeInfo_Blit[] types;
    public FunctionInfo_Blit[] functions;
}

public class TypeInfo_Blit
{
    public string name;
    public bool isValueType;
    public FieldInfo_Blit[] fields;
    public InModuleIndex[] functions;
}

public class FieldInfo_Blit
{
    public string name;
    public InModuleIndex type;
}

public class FunctionInfo_Blit
{
    public string name;
    public bool isStatic;
    public InModuleIndex ownerType;
    public FieldInfo_Blit[] arguments;
    public InModuleIndex[] returns;
    public int pointedOpCode;
}

public struct InModuleIndex
{
    public uint value;

    public InModuleIndex()
    {
        value = 0;
    }

    public InModuleIndex(int value)
    {
        this.value = (uint)value;
    }

    public InModuleIndex(uint value)
    {
        this.value = value;
    }

    public static implicit operator uint(InModuleIndex index)
    {
        return index.value;
    }
    public static implicit operator InModuleIndex(int value)
    {
        return new InModuleIndex(value);
    }
}

public class ManagedCode
{
    public byte[] byteCode;
}