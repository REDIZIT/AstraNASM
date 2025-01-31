namespace Astra.Compilation;

public class ResolvedModule
{
    public Dictionary<string, ClassTypeInfo> classInfoByName = new();

    public ClassTypeInfo GetType(string name)
    {
        return classInfoByName[name];
    }
}

public class FunctionInfo
{
    public string name;
    public ClassTypeInfo owner;

    public List<FieldInfo> arguments = new();
    public List<TypeInfo> returns = new();
}
public class FieldInfo
{
    public string name;
    public TypeInfo type;

    public FieldInfo()
    {
    }
    public FieldInfo(TypeInfo type, string name)
    {
        this.type = type;
        this.name = name;
    }

}
public class ClassTypeInfo : TypeInfo
{
    public List<FieldInfo> fields = new();
    public List<FunctionInfo> functions = new();
    public bool isStruct;

    public override string ToString()
    {
        return "%" + name;
    }
}
public class TypeInfo
{
    public string name;

    public override string ToString()
    {
        return name;
    }
}


public class VariableRawData
{
    public string name;
    public string rawType;
    public TypeInfo type;

    public void Resolve(ResolvedModule module)
    {
        type = module.GetType(rawType);
    }
}