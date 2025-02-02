namespace Astra.Compilation;

public class TypeInfo
{
    public string name;
    public List<FieldInfo> fields = new();
    public List<FunctionInfo> functions = new();
    public bool isStruct;

    public override string ToString()
    {
        return name;
    }
}