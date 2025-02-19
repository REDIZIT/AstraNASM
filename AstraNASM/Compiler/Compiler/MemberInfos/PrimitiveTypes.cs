﻿namespace Astra.Compilation;

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
}