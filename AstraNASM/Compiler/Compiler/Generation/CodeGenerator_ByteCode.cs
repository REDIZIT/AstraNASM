using AVM;

namespace Astra.Compilation;

public class CodeGenerator_ByteCode : CodeGeneratorBase
{
    protected List<byte> byteCode
    {
        get
        {
            if (parent == null) return _byteCode;
            else return (parent as CodeGenerator_ByteCode).byteCode;
        }
    }
    private List<byte> _byteCode = new();
    
    
    protected Dictionary<string, int> pointedOpCodeByName
    {
        get
        {
            if (parent == null) return _pointedOpCodeByName;
            else return (parent as CodeGenerator_ByteCode).pointedOpCodeByName;
        }
    }
    private Dictionary<string, int> _pointedOpCodeByName = new();
    
    protected Dictionary<int, string> labelsToInsert
    {
        get
        {
            if (parent == null) return _labelsToInsert;
            else return (parent as CodeGenerator_ByteCode).labelsToInsert;
        }
    }
    private Dictionary<int, string> _labelsToInsert = new();
    
    
    public override void PrologueForSimulation(CompileTarget target)
    {
        Allocate(PrimitiveTypes.INT);
        Add(OpCode.Call);
        InsertAddress("Program.main");
        Add(OpCode.Exit);
    }

    public override string RegisterLabel(string labelName)
    {
        return base.RegisterLabel(labelName);
    }

    public override void Label(string labelName)
    {
        if (pointedOpCodeByName.Count >= byte.MaxValue)
        {
            throw new Exception($"Failed to register label '{labelName}' due to limit exceeded. Seems, it's time to change id type from byte to short, int or long.");
        }
        
        pointedOpCodeByName.Add(labelName, byteCode.Count);
    }
    
    public override void Prologue()
    {
        Add(OpCode.FunctionPrologue);
    }
    
    public override void Epilogue()
    {
        Add(OpCode.FunctionEpilogue);
    }

    public override void Return_Variable(FunctionInfo function, Variable variable)
    {
        // Add(OpCode.Return);
        
        int rbpOffset = 2 * 4 + function.arguments.Count * 4;
        if (function.isStatic == false) rbpOffset += 4;
        
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(rbpOffset); // negate rbpOffset
        
        Add((byte)1);
        AddInt(variable.rbpOffset);

        byte sizeInBytes = Utils.GetSizeInBytes(variable.type);
        Add(sizeInBytes);
    }

    public override void Call(string functionName)
    {
        Add(OpCode.Call);
        InsertAddress(functionName);
    }

    public override Variable Allocate(TypeInfo type)
    {
        return Allocate(type, "anon");
    }

    public override Variable Allocate(TypeInfo type, string name)
    {
        if (type == null)
        {
            throw new Exception("Failed to allocated variable with null type.");
        }
        
        byte sizeInBytes = Utils.GetSizeInBytes(type);

        rbpOffset -= sizeInBytes;
        
        Variable variable = new Variable()
        {
            name = name,
            type = type,
            rbpOffset = rbpOffset,
        };
        variableByName.Add(variable.name, variable);
        variableByRBPOffset.Add(variable.rbpOffset, variable);
        variableStack.Push(variable);
        
        
        Add(OpCode.Allocate_Stack);
        
        Add(sizeInBytes);

        for (int i = 0; i < sizeInBytes; i++)
        {
            Add((byte)0);
        }

        return variable;
    }

    public override void SetValue(Variable variable, string value)
    {
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(variable.rbpOffset);

        Add((byte)2);
        byte sizeInBytes = Utils.GetSizeInBytes(variable.type);
        Add(sizeInBytes);

        byte[] bytes;
        if (sizeInBytes == 1) bytes = new byte[1] { byte.Parse(value) };
        else if (sizeInBytes == 2) bytes = BitConverter.GetBytes(short.Parse(value));
        else if (sizeInBytes == 4) bytes = BitConverter.GetBytes(int.Parse(value));
        else bytes = BitConverter.GetBytes(long.Parse(value));
        
        AddRange(bytes);
    }

    public override void Return_Void()
    {
        Add(OpCode.Return);
        AddInt(0);
    }

    public override void Comment(string comment)
    {
        
    }

    private void Add(byte b)
    {
        byteCode.Add(b);
    }
    private void AddRange(byte[] bytes)
    {
        byteCode.AddRange(bytes);
    }

    private void Add(OpCode code)
    {
        Add((byte)code);
    }
    
    private void AddInt(int value)
    {
        AddRange(BitConverter.GetBytes(value));
    }

    private void Fill(byte value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Add(value);
        }
    }

    private void InsertAddress(string labelName)
    {
        labelsToInsert.Add(byteCode.Count, labelName);
        Fill(0, sizeof(int));
    }

    private void ResolveLabelAddresses()
    {
        foreach (KeyValuePair<int, string> kv in labelsToInsert)
        {
            int pointedAddress = pointedOpCodeByName[kv.Value];
            int labelInsertAddress = kv.Key;

            byte[] bytes = BitConverter.GetBytes(pointedAddress);
            for (int i = 0; i < bytes.Length; i++)
            {
                byteCode[labelInsertAddress + i] = bytes[i];
            }
        }
    }

    public override byte[] Build()
    {
        ResolveLabelAddresses();
        
        return byteCode.ToArray();
    }
}