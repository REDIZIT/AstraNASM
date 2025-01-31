namespace Astra.Compilation;

public class FunctionInfo
{
    public string name;
    public ClassTypeInfo owner;

    public List<FieldInfo> arguments = new();
    public List<TypeInfo> returns = new();
}
