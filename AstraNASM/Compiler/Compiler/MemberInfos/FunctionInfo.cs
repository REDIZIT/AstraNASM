namespace Astra.Compilation;

public class FunctionInfo
{
    public string name;
    public TypeInfo owner;

    public List<FieldInfo> arguments = new();
    public List<TypeInfo> returns = new();
}