namespace Astra.Compilation;

public class ResolvedModule
{
    public Dictionary<string, ClassTypeInfo> classInfoByName = new();

    public ClassTypeInfo GetType(string name)
    {
        return classInfoByName[name];
    }
}
