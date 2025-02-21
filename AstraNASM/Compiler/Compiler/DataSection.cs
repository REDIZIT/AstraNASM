using System.Text;

namespace Astra.Compilation;

public class DataSection
{
    public List<byte> data = new();

    public Dictionary<int, string> stringByAddress = new();
    
    public string RegisterString(string str)
    {
        int address = data.Count;
        
        data.AddRange(BitConverter.GetBytes((int)str.Length));
        data.AddRange(Encoding.ASCII.GetBytes(str));
        
        stringByAddress.Add(address, str);
        
        return address.ToString();
    }

    public void Generate(Generator.Context ctx)
    {
        ctx.gen.SectionData(data.ToArray());
    }
}