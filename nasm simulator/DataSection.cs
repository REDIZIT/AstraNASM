public class DataSection
{
    public int address;
    public int endAddress;
    public Dictionary<string, long> addressByLabel = new();

    public DataSection(int address)
    {
        this.address = address;
        this.endAddress = this.address;
    }
}