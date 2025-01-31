namespace Astra.Compilation;

public class ClassTypeInfo : TypeInfo
{
    public List<FieldInfo> fields = new();
    public List<FunctionInfo> functions = new();
    public bool isStruct;
}
