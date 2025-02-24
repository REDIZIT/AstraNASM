namespace Astra.Compilation;

public class FunctionInfo
{
    public string name;
    public TypeInfo owner;
    public bool isStatic, isAbstract;

    public List<FieldInfo> arguments = new();
    public List<TypeInfo> returns = new();

    public int inModuleIndex;
    public int pointedOpCode;

    public string GetCombinedName()
    {
        return owner.name + "." + name;
    }
}