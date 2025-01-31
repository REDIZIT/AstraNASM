namespace Astra.Compilation;

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
