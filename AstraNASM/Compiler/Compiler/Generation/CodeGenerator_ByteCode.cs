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
        InsertAddress("program.main");
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
        // int rbpOffset = 2 * 4 + function.arguments.Count * 4;
        // if (function.isStatic == false) rbpOffset += 4;
        
        int rbpOffset = 2 * 4 + function.arguments.Sum(a => Utils.GetSizeInBytes(a.type));
        if (function.owner != null) rbpOffset += 4;
        
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(rbpOffset);
        
        Add((byte)1);
        AddInt(variable.rbpOffset);

        AddSize(variable);
    }

    public override void Call(string functionName)
    {
        Add(OpCode.Call);
        InsertAddress(functionName);
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
        Add((byte)0);
        
        Add(sizeInBytes);

        for (int i = 0; i < sizeInBytes; i++)
        {
            Add((byte)0);
        }

        return variable;
    }

    public override void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate)
    {
        Add(OpCode.Allocate_Heap);
        Add((byte)0);
        AddInt(storageOfPointerToHeap.rbpOffset);
        AddInt(bytesToAllocate);
    }

    public override void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable)
    {
        Add(OpCode.Allocate_Heap);
        Add((byte)1);
        AddInt(storageOfPointerToHeap.rbpOffset);
        AddInt(bytesToAllocateVariable.rbpOffset);
        AddSize(bytesToAllocateVariable);
    }

    public override void PushToStack(Variable variable, string comment = null)
    {
        Add(OpCode.Allocate_Stack);
        Add((byte)1);
        AddInt(variable.rbpOffset);
        AddSize(variable);
    }

    public override void PushToStack(string value, string comment = null)
    {
        Add(OpCode.Allocate_Stack);
        Add((byte)0);
        
        if (byte.TryParse(value, out byte b))
        {
            Add((byte)1);
            Add(b);
        }
        else if (short.TryParse(value, out short s))
        {
            Add((byte)2);
            AddRange(BitConverter.GetBytes(s));
        }
        else if (int.TryParse(value, out int i))
        {
            Add((byte)4);
            AddRange(BitConverter.GetBytes(i));
        }
        else if (long.TryParse(value, out long l))
        {
            Add((byte)8);
            AddRange(BitConverter.GetBytes(l));
        }
        else
        {
            throw new NotImplementedException(value);
        }
    }

    public override void Deallocate(int sizeInBytes)
    {
        Add(OpCode.Deallocate_Stack);
        AddInt(sizeInBytes);
    }


    public override void SetValue(Variable variable, string value)
    {
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(variable.rbpOffset);

        Add((byte)2);
        byte sizeInBytes = Utils.GetSizeInBytes(variable.type);
        Add(sizeInBytes);

        byte[] bytes = Utils.ParseNumber(value, sizeInBytes);
        
        AddRange(bytes);
    }

    public override void SetValue(Variable destination, Variable value)
    {
        if (Utils.AreSameSize(destination, value) == false)
        {
            throw new Exception("Failed to set value due to different variables sizes");
        }
        
        
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(destination.rbpOffset);

        Add((byte)1);
        AddInt(value.rbpOffset);
        
        AddSize(destination);
    }

    public override void SetValue(Variable destination, int address)
    {
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(destination.rbpOffset);
        
        Add((byte)4);
        AddInt(address);
        
        AddSize(destination);
    }

    public override void SetValueBehindPointer(Variable destination, string value)
    {
        Add(OpCode.Mov);
        
        Add((byte)2);
        AddInt(destination.rbpOffset);
        
        Add((byte)2);
        byte sizeInBytes = Utils.GetSizeInBytes(destination.type);
        Add(sizeInBytes);

        byte[] bytes = Utils.ParseNumber(value, sizeInBytes);
        
        AddRange(bytes);
    }

    public override void SetValueBehindPointer(Variable destination, Variable value)
    {
        if (destination.type != PrimitiveTypes.PTR) throw new Exception("Failed to set value behind pointer: destination is not a pointer");
        
        Add(OpCode.Mov);
        
        Add((byte)2);
        AddInt(destination.rbpOffset);
        
        Add((byte)1);
        AddInt(value.rbpOffset);
        
        AddSize(destination);
    }

    public override void FieldAccess(int baseOffset, TypeInfo fieldType, int fieldOffset, Variable result, bool isGetter)
    {
        if (fieldOffset < 0) throw new Exception("Negative fieldOffset is not allowed.");
        
        Add(OpCode.FieldAccess);
        AddInt(baseOffset);
        AddInt(fieldOffset);
        AddSize(fieldType);
        Add(isGetter ? (byte)1 : (byte)0);
        AddInt(result.rbpOffset);
    }


    public override void Return_Void()
    {
        Add(OpCode.Return);
    }
    public override void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        OpCode op;
        if (@operator.asmOperatorName == "add") op = OpCode.Add;
        else if (@operator.asmOperatorName == "sub") op = OpCode.Sub;
        else if (@operator.asmOperatorName == "mul") op = OpCode.Mul;
        else if (@operator.asmOperatorName == "div") op = OpCode.Div;
        else if (@operator.asmOperatorName == "<<") op = OpCode.LeftBitShift;
        else if (@operator.asmOperatorName == ">>") op = OpCode.RightBitShift;
        else if (@operator.asmOperatorName == "&") op = OpCode.BitAnd;
        else if (@operator.asmOperatorName == "|") op = OpCode.BitOr;
        else throw new Exception($"Unknown operator name '{@operator.asmOperatorName}'");
        
        Add(op);
        
        AddInt(a.rbpOffset);
        AddInt(b.rbpOffset);
        AddInt(result.rbpOffset);

        byte sizeInBytes = Utils.GetSizeInBytes(a.type);

        if (Utils.GetSizeInBytes(a.type) != Utils.GetSizeInBytes(b.type) || Utils.GetSizeInBytes(b.type) != Utils.GetSizeInBytes(result.type))
        {
            throw new Exception("Can not apply math operator to different sized types");
        }
        
        Add(sizeInBytes);
    }

    public override void Negate(Variable a, Variable result)
    {
        Utils.AssertSameSize(a, result);
        
        Add(OpCode.Negate);
        AddInt(a.rbpOffset);
        AddInt(result.rbpOffset);

        byte size = Utils.GetSizeInBytes(result.type);
        Add(size);
    }

    
    public override void ToPtr_Primitive(Variable askedVariable, Variable result)
    {
        Add(OpCode.ToPtr_ValueType);
        AddInt(askedVariable.rbpOffset);
        AddInt(result.rbpOffset);
    }
    
    public override void ToPtr_Heap(Variable askedVariable, Variable result)
    {
        Add(OpCode.ToPtr_RefType);
        AddInt(askedVariable.rbpOffset);
        AddInt(result.rbpOffset);
    }

    public override void PtrAddress(Variable pointer, Variable result, bool isGetter)
    {
        if (isGetter) ToPtr_Heap(pointer, result);
        else ToPtr_Primitive(pointer, result);
    }

    public override void PtrGet(Variable pointerVariable, Variable result)
    {
        Add(OpCode.PtrGet);
        AddInt(pointerVariable.rbpOffset);
        AddInt(result.rbpOffset);
        
        AddSize(result);
    }

    public override void PtrSet(Variable pointerVariable, Variable targetVariable)
    {
        Add(OpCode.PtrSet);
        AddInt(pointerVariable.rbpOffset);
        AddInt(targetVariable.rbpOffset);
        
        AddSize(targetVariable);
    }

    public override void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0)
    {
        Add(OpCode.PtrShift);
        Add((byte)1);
        AddInt(pointerVariable.rbpOffset);
        AddInt(shiftVariable.rbpOffset);
        AddInt(additionalShift);

        AddSize(shiftVariable);
    }

    public override void PtrShift(Variable pointerVariable, int shift)
    {
        Add(OpCode.PtrShift);
        Add((byte)0);
        AddInt(pointerVariable.rbpOffset);
        AddInt(shift);
    }

    public override void Compare(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        Utils.AssertSameSize(a, b);
        if (result.type != PrimitiveTypes.BOOL) throw new Exception($"Failed to compare variables due to result variable is not bool, but '{result.type.name}'");
        
        Add(OpCode.Compare);
        AddInt(a.rbpOffset);
        AddInt(b.rbpOffset);
        AddSize(a);
        AddInt(result.rbpOffset);

        byte op = 0;
        if (@operator.asmOperatorName == "e") op = 0;
        else if (@operator.asmOperatorName == "ne") op = 1;
        else if (@operator.asmOperatorName == "g") op = 2;
        else if (@operator.asmOperatorName == "ge") op = 3;
        else if (@operator.asmOperatorName == "l") op = 4;
        else if (@operator.asmOperatorName == "le") op = 5;
        else throw new Exception($"Unknown comprassion operator '{@operator.asmOperatorName}'");
        
        Add(op);
    }

    public override void JumpIfFalse(Variable condition, string label)
    {
        Add(OpCode.JumpIfFalse);
        InsertAddress(label);
        AddInt(condition.rbpOffset);
        AddSize(condition);
    }

    public override void JumpIfFalse(string reg, string label)
    {
        throw new Exception("This method is not allowed");
    }

    public override void JumpToLabel(string label)
    {
        Add(OpCode.Jump);
        InsertAddress(label);
    }


    public override void Space(int lines = 1)
    {
    }
    public override void Comment(string comment)
    {
    }
    public override void Comment(string comment, int bookmarkDistance)
    {
    }


    private void Add(byte b)
    {
        byteCode.Add(b);
    }

    private void AddSize(Variable variable)
    {
        AddSize(variable.type);
    }
    private void AddSize(TypeInfo type)
    {
        Add(Utils.GetSizeInBytes(type));
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

        if (b.lines.Count != 0)
        {
            throw new Exception($"Code generator's string builder is not empty and contains: '{b.BuildString()}'");
        }
        
        return byteCode.ToArray();
    }
}