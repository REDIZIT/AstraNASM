namespace Astra.Compilation;

public class ResolvedModule
{
    private Dictionary<string, TypeInfo> classInfoByName = new();

    public List<TypeInfo> types = new();
    public List<FunctionInfo> functions = new();

    public DataSection data = new();
    
    public TypeInfo GetType(string name)
    {
        return classInfoByName[name];
    }

    public bool TryGetType(string name, out TypeInfo type)
    {
        return classInfoByName.TryGetValue(name, out type);
    }

    public void RegisterType(TypeInfo type)
    {
        type.inModuleIndex = types.Count;
        types.Add(type);
        
        classInfoByName.Add(type.name, type);

        foreach (FunctionInfo f in type.functions)
        {
            f.inModuleIndex = functions.Count;
            functions.Add(f);
        }
    }
}