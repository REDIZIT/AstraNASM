namespace Astra.Compilation;

public class ResolvedModule
{
    public Dictionary<string, TypeInfo> classInfoByName = new();

    public TypeInfo GetType(string name)
    {
        return classInfoByName[name];
    }
}
