using AVM;

namespace Astra.Compilation;

public class CodeGenerator_ByteCode : CodeGeneratorBase
{
    private List<byte> byteCode = new();
    private Dictionary<string, int> pointedOpCodeByName = new();
    private Dictionary<int, string> labelsToInsert = new();
    
    
    public override void PrologueForSimulation(CompileTarget target, ResolvedModule module)
    {
        Variable retVar = Allocate(PrimitiveTypes.INT);

        FunctionInfo main = module.classInfoByName["program"].functions.First(f => f.name == "main");
        if (main.isStatic == false)
        {
            Variable selfStackVar = Allocate(PrimitiveTypes.PTR);
            AllocateHeap(selfStackVar, 0);
        }
        
        Call(main.GetCombinedName());

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
        currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "prologue_pushed_rbp");
        Add(OpCode.FunctionPrologue);
    }
    
    public override void Epilogue()
    {
        Add(OpCode.FunctionEpilogue);
    }
    
    public override void Call(string functionName)
    {
        // BeginSubScope(); // will be closed by return
        currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "call_pushed_instruction");
        
        Add(OpCode.Call);
        InsertAddress(functionName);
    }
    
    public override void Return()
    {
        DropSubScope();
        
        Add(OpCode.Return);
    }
    
    public override void Out_Variable(FunctionInfo function, Variable variable)
    {
        int inscopeRbpOffset = Utils.GetRBP_RetValue(function);
        
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(inscopeRbpOffset);
        
        Add((byte)1);
        AddInt(variable.inscopeRbpOffset);

        AddSize(variable);
    }

   

    public override Variable Allocate(TypeInfo type, string name)
    {
        if (type == null)
        {
            throw new Exception("Failed to allocated variable with null type.");
        }


        Variable variable = currentScope.RegisterLocalVariable(type, name);
        

        Add(OpCode.Allocate_Stack);
        Add((byte)Allocate_Stack_Mode.WithDefaultValue);
        
        byte sizeInBytes = Utils.GetSizeInBytes(type);
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
        AddInt(storageOfPointerToHeap.inscopeRbpOffset);
        AddInt(bytesToAllocate);
    }

    public override void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable)
    {
        Add(OpCode.Allocate_Heap);
        Add((byte)1);
        AddInt(storageOfPointerToHeap.inscopeRbpOffset);
        AddInt(bytesToAllocateVariable.inscopeRbpOffset);
        AddSize(bytesToAllocateVariable);
    }

    public override void PushToStack(Variable variable, string comment = null)
    {
        Add(OpCode.Allocate_Stack);
        Add((byte)Allocate_Stack_Mode.PushAlreadyAllocatedVariable);
        AddInt(variable.inscopeRbpOffset);
        AddSize(variable);
    }

    public override void PushToStack(string str, TypeInfo type, string comment = null)
    {
        Add(OpCode.Allocate_Stack);
        Add((byte)0);

        byte[] value = Utils.ParseNumber(str, Utils.GetSizeInBytes(type));
        Add((byte)value.Length);
        AddRange(value);
    }

    public override void Deallocate(Variable variable)
    {
        currentScope.UnregisterLocalVariable(variable);
        
        Add(OpCode.Deallocate_Stack);
        AddInt(variable.type.sizeInBytes);
    }


    public override void SetValue(Variable variable, string value)
    {
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(variable.inscopeRbpOffset);

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
        AddInt(destination.inscopeRbpOffset);

        Add((byte)1);
        AddInt(value.inscopeRbpOffset);
        
        AddSize(destination);
    }

    public override void SetValue(Variable destination, int address)
    {
        Add(OpCode.Mov);
        
        Add((byte)1);
        AddInt(destination.inscopeRbpOffset);
        
        Add((byte)4);
        AddInt(address);
        
        AddSize(destination);
    }

    public override void SetValueBehindPointer(Variable destination, string value)
    {
        Add(OpCode.Mov);
        
        Add((byte)2);
        AddInt(destination.inscopeRbpOffset);
        
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
        AddInt(destination.inscopeRbpOffset);
        
        Add((byte)1);
        AddInt(value.inscopeRbpOffset);
        
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
        AddInt(result.inscopeRbpOffset);
    }


    public override void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        Utils.AssertSameOrLessSize(result, a, b);
        
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
        
        AddInt(a.inscopeRbpOffset);
        AddInt(b.inscopeRbpOffset);
        AddInt(result.inscopeRbpOffset);

        byte sizeInBytes = Utils.GetSizeInBytes(a.type);
        Add(sizeInBytes);
    }

    public override void Negate(Variable a, Variable result)
    {
        Utils.AssertSameOrLessSize(result, a);
        
        Add(OpCode.Negate);
        AddInt(a.inscopeRbpOffset);
        AddInt(result.inscopeRbpOffset);

        byte size = Utils.GetSizeInBytes(result.type);
        Add(size);
    }

    
    public override void ToPtr_Primitive(Variable askedVariable, Variable result)
    {
        Add(OpCode.ToPtr_ValueType);
        AddInt(askedVariable.inscopeRbpOffset);
        AddInt(result.inscopeRbpOffset);
    }
    
    public override void ToPtr_Heap(Variable askedVariable, Variable result)
    {
        Add(OpCode.ToPtr_RefType);
        AddInt(askedVariable.inscopeRbpOffset);
        AddInt(result.inscopeRbpOffset);
    }

    public override void PtrAddress(Variable pointer, Variable result, bool isGetter)
    {
        if (isGetter) ToPtr_Heap(pointer, result);
        else ToPtr_Primitive(pointer, result);
    }

    public override void PtrGet(Variable pointerVariable, Variable result)
    {
        Add(OpCode.PtrGet);
        AddInt(pointerVariable.inscopeRbpOffset);
        AddInt(result.inscopeRbpOffset);
        
        AddSize(result);
    }

    public override void PtrSet(Variable pointerVariable, Variable targetVariable)
    {
        Add(OpCode.PtrSet);
        AddInt(pointerVariable.inscopeRbpOffset);
        AddInt(targetVariable.inscopeRbpOffset);
        
        AddSize(targetVariable);
    }

    public override void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0)
    {
        Add(OpCode.PtrShift);
        Add((byte)1);
        AddInt(pointerVariable.inscopeRbpOffset);
        AddInt(shiftVariable.inscopeRbpOffset);
        AddInt(additionalShift);

        AddSize(shiftVariable);
    }

    public override void PtrShift(Variable pointerVariable, int shift)
    {
        Add(OpCode.PtrShift);
        Add((byte)0);
        AddInt(pointerVariable.inscopeRbpOffset);
        AddInt(shift);
    }

    public override void Compare(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        Utils.AssertSameOrLessSize(result, a, b);
        if (result.type != PrimitiveTypes.BOOL) throw new Exception($"Failed to compare variables due to result variable is not bool, but '{result.type.name}'");
        
        Add(OpCode.Compare);
        AddInt(a.inscopeRbpOffset);
        AddInt(b.inscopeRbpOffset);
        AddSize(a);
        AddInt(result.inscopeRbpOffset);

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
        AddInt(condition.inscopeRbpOffset);
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

    public override void Cast(Variable variable, Variable result)
    {
        Add(OpCode.Cast);
        AddInt(variable.inscopeRbpOffset);
        AddSize(variable);
        AddInt(result.inscopeRbpOffset);
        AddSize(result);
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
    
    public override void PushExceptionHandler(string catchLabel)
    {
        throw new NotImplementedException();
    }

    public override void ThrowException(Variable exception)
    {
        throw new NotImplementedException();
    }
    
    public override void LogicalNOT(Variable a, Variable result)
    {
        throw new NotImplementedException();
    }
    
    public override void Print(Variable variable)
    {
        throw new NotImplementedException();
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