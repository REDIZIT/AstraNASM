﻿namespace Astra.Compilation;

public class CodeGenerator
{
    private CodeStringBuilder b = new();
    private int rbpOffset;

    private Dictionary<string, Variable> variableByName = new();
    private Stack<Variable> variableStack = new();
    private int anonVariableNameIndex;

    public CodeGenerator parent;
    public List<CodeGenerator> children = new();

    public void Label(string labelName)
    {
        b.Line(labelName + ":");
    }
    
    public void Prologue()
    {
        b.Line("push rbp");
        b.Line("mov rbp, rsp");
    }
    public void Epilogue()
    {
        b.Line("mov rsp, rbp");
        b.Line("pop rbp");
    }
    public void PrologueForSimulation()
    {
        b.Line("mov rbx, 0");
        b.Line("push rbx ; return int");

        b.Line("call main");

        b.Line("add rsp, 8");
        b.Line("pop rax");

        b.Line("mov 0x00, rax");
        b.Line("exit");
        
        b.Space(2);
    }

    public void Return_Void()
    {
        b.Line("ret");
    }
    public void Return_Variable(FunctionInfo function, Variable variable)
    {
        int rbpOffset = 16 + function.arguments.Count * 8;
        if (function.owner != null) rbpOffset += 8;
        
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"mov [rbp+{rbpOffset}], rbx");
    }


    public Variable Allocate(TypeInfo type)
    {
        anonVariableNameIndex++;
        return Allocate(type, $"anon_{anonVariableNameIndex}");
    }

    public Variable Allocate(TypeInfo type, string name)
    {
        int sizeInBytes = 8;

        rbpOffset -= sizeInBytes;
        
        Variable variable = new Variable()
        {
            name = name,
            type = type,
            rbpOffset = rbpOffset,
        };
        variableByName.Add(variable.name, variable);
        variableStack.Push(variable);
        
        b.Line($"sub rsp, {sizeInBytes} ; allocate '{variable.name}' at {variable.RBP}");

        return variable;
    }
    
    public void Deallocate(Variable variable)
    {
        if (variable == null)
        {
            throw new Exception($"Failed to deallocate null variable.");
        }
        if (variableByName.ContainsKey(variable.name) == false)
        {
            throw new Exception($"Failed to deallocate '{variable.name}' because it is not even allocated (or already deallocated) on stack.");
        }
        if (variableStack.Peek() != variable)
        {
            throw new Exception($"Failed to deallocate variable '{variable.name}' because it is not the last variable on stack. Only last variable can be deallocated on stack.'");
        }
        
        variableByName.Remove(variable.name);
        variableStack.Pop();

        int sizeInBytes = 8;
        b.CommentLine($"return allocation");
        b.Line($"add rsp, {sizeInBytes}");
    }

    public void Deallocate(int sizeInBytes)
    {
        b.Line($"add rsp, {sizeInBytes}");
    }

    public void AllocateHeap(Variable storageOfPointerToHeap)
    {
        b.CommentLine($"heap alloc");
        b.Line($"mov {storageOfPointerToHeap.RBP}, 0x110"); // result.RBP - pointer to object table, 0x110 - pointer to real data
        b.Line($"mov rbx, [0x100]");
        b.Line($"add rbx, 1");
        b.Line($"mov [0x100], rbx");
    }

    public void Register_FunctionArgumentVariable(FieldInfo info, int index)
    {
        Variable variable = new Variable()
        {
            name = info.name,
            type = info.type,
            rbpOffset = 16 + index * 8
        };
        
        variableByName.Add(variable.name, variable);
    }

    public void Unregister_FunctionArgumentVariable(Variable variable)
    {
        variableByName.Remove(variable.name);
    }

    public void SetValue(Variable variable, string value)
    {
        b.Line($"mov qword {variable.RBP}, {value}");
    }

    public void SetValue(Variable destination, Variable value)
    {
        b.Line($"mov qword rbx, {value.RBP}");
        b.Line($"mov qword {destination.RBP}, rbx");
    }

    public void SetValueToField(Variable destination, Variable value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov rdx, {value.RBP}");
        b.Line($"mov qword [rbx], rdx");
    }

    public void SetValueFromReg(Variable destination, string sourceReg)
    {
        b.Line($"mov {destination.RBP}, {sourceReg}");
    }



    public void JumpIfFalse(Variable condition)
    {
        b.Line($"mov rbx, {condition.RBP}");
        b.Line($"cmp rbx, 0");
        b.Line($"jle if_false");
    }

    public void JumpToLabel(string label)
    {
        b.Line($"jmp {label}");
    }
    

    public void Compare(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        this.b.Line($"mov rbx, {a.RBP}"); 
        this.b.Line($"mov rdx, {b.RBP}"); 
        
        this.b.Line($"cmp rbx, rdx");
        this.b.Line($"mov rbx, 0");
        this.b.Line($"set{@operator.asmOperatorName} bl");
        
        this.b.Line($"mov {result.RBP}, rbx");
    }

    public void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        this.b.Line($"mov rbx, {a.RBP}"); 
        this.b.Line($"mov rdx, {b.RBP}"); 
        
        this.b.Line($"{@operator.asmOperatorName} rbx, rdx");
        
        this.b.Line($"mov {result.RBP}, rbx");
    }


    public void LogicalNOT(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"test rbx, rbx");
        b.Line($"xor rbx, rbx"); // reset rbx to zero
        b.Line($"sete bl"); // set last byte of reg to 1 or 0
        b.Line($"mov {result.RBP}, rbx");
    }

    public void Negate(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"neg rbx");
        b.Line($"mov {result.RBP}, rbx");
    }


    public void ToPtr_Primitive(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name}");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov {result.RBP}, rbx");
    }

    public void ToPtr_Heap(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name} (heap data)");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov {result.RBP}, [rbx]");
    }

    public void PtrAddress(Variable pointer, Variable result)
    {
        b.Space();
        b.CommentLine($"{pointer.name}.address");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {pointer.rbpOffset} ; offset to target ptr data cell");
        b.Line($"mov {result.RBP}, rbx ; now {result.RBP} is pointer to {pointer.name} (.address)");
    }

    public void PtrGet(Variable pointerVariable, Variable result)
    {
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, [rbx]");
        b.Line($"mov {result.RBP}, rdx");
    }

    public void PtrSet(Variable pointerVariable, Variable targetVariable)
    {
        string nasmType = Utils.GetNASMType(targetVariable.type);
        
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {targetVariable.RBP}");
        b.Line($"mov {nasmType} [rbx], {Utils.ClampRegister(nasmType)}");
    }

    public void PtrShift(Variable pointerVariable, Variable shiftVariable)
    {
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {shiftVariable.RBP}");
        b.Line($"add rbx, rdx");
        b.Line($"mov {pointerVariable.RBP}, rbx");
    }

    public void Print(Variable variable)
    {
        b.Space();
        b.CommentLine($"print {variable.name}");
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"print [rbx]");
    }
    
    

    public void CalculateAddress_RBP_Shift(Variable shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes.RBP}");
        SetValueFromReg(result, "rbx");
    }
    public void CalculateAddress_RBP_Shift(int shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes}");
        SetValueFromReg(result, "rbx");
    }

    public void RBP_Shift_And_LoadFromRAM(int shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, [rbp{shiftInBytes}]");
        b.Line($"mov {result.RBP}, rbx");
    }


    public void PushToStack(Variable variable, string comment = null)
    {
        PushToStack(variable.RBP, comment);
    }
    public void PushToStack(string value, string comment = null)
    {
        b.Line($"mov rbx, {value}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
        b.Line($"push rbx");
    }


    public void Call(string functionName)
    {
        b.Line($"call {functionName}");
    }
    
    public void Space(int lines = 1)
    {
        b.Space(lines);
    }
    public void Comment(string comment)
    {
        b.CommentLine(comment);
    }

    public Variable GetVariable(string name)
    {
        if (variableByName.TryGetValue(name, out Variable var)) return var;

        if (parent != null) return parent.GetVariable(name);
        
        throw new Exception($"Variable '{name}' not found in scope");
    }

    public string BuildString()
    {
        List<string> childrenStrings = new();

        childrenStrings.Add(b.BuildString());
        
        foreach (CodeGenerator child in children)
        {
            childrenStrings.Add(child.BuildString());
        }
        

        string nasm = string.Join("\n", childrenStrings);
        return nasm;
    }
}