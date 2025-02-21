namespace Astra.Compilation;

public static class PrimitiveTypes
{
    public static TypeInfo BOOL;
    public static TypeInfo BYTE;
    public static TypeInfo SHORT;
    public static TypeInfo INT;
    public static TypeInfo LONG;

    public static TypeInfo PTR;
    public static TypeInfo STRING;

    public static bool IsPrimitive(TypeInfo type)
    {
        return
            type == BOOL || type == BYTE || type == SHORT ||
            type == INT || type == LONG;
    }
    
    public static bool IsPrimitiveOrPtr(TypeInfo type)
    {
        return IsPrimitive(type) || type == PTR;
    }

    public static byte GetIndex(TypeInfo type)
    {
        if (type == BOOL) return 0;
        if (type == BYTE) return 1;
        if (type == SHORT) return 2;
        if (type == INT) return 3;
        if (type == LONG) return 4;
        if (type == PTR) return 5;
        if (type == STRING) return 6;

        throw new Exception($"Failed to get index for type '{type.name}' due to it is not primitive");
    }
}