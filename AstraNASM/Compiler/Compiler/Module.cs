namespace Astra.Compilation;

public class ResolvedModule
{
    public Dictionary<string, TypeInfo> classInfoByName = new();

    public Dictionary<string, string> stringValueByID = new();

    public TypeInfo GetType(string name)
    {
        return classInfoByName[name];
    }

    public string RegisterString(string str)
    {
        List<char> id = new List<char>() { 's', 't', 'r', '_' };

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (char.IsLetterOrDigit(c))
            {
                id.Add(c);
            }
            else if (id.Last() != '_')
            {
                id.Add('_');
            }

            if (id.Count >= 20)
            {
                break;
            }
        }

        string stringID = string.Concat(id);
        string uniqueStringID = stringID;
        
        int indexID = 0;
        while (stringValueByID.ContainsKey(uniqueStringID))
        {
            uniqueStringID = stringID + "_" + indexID;
        }
        
        stringValueByID.Add(uniqueStringID, str);

        return uniqueStringID;
    }
}