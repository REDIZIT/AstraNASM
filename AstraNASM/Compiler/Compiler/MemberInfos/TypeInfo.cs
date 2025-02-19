namespace Astra.Compilation;

public class TypeInfo
{
    public string name;
    public List<FieldInfo> fields = new();
    public List<FunctionInfo> functions = new();
    public bool isValueType;

    public int sizeInBytes
    {
        get
        {
            if (_sizeInBytes < 0) throw new Exception("SizeInBytes is not calculated yet. This field is available only after Resolver's stage.");
            return _sizeInBytes;
        }
    }

    public int refSizeInBytes
    {
        get
        {
            if (isValueType) return _sizeInBytes;
            return PrimitiveTypes.PTR.sizeInBytes;
        }
    }

    private int _sizeInBytes = -1;
    
    public void CalculateSizeInBytes()
    {
        isValueType = PrimitiveTypes.IsPrimitiveOrPtr(this);

        if (isValueType)
        {
            _sizeInBytes = Utils.GetSizeInBytes(this);
        }
        else
        {
            _sizeInBytes = fields.Sum(f => Utils.GetSizeInBytes(this));   
        }
    }
    
    public override string ToString()
    {
        return name;
    }

}