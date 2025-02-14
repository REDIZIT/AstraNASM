using System.Text;

namespace Astra.Compilation;

public abstract class CodeGeneratorBase
{
    public CodeGeneratorBase parent;
    public List<CodeGeneratorBase> children = new();
    public CodeStringBuilder b;
    protected int rbpOffset;
    protected Dictionary<string, Variable> variableByName = new();
    protected Dictionary<int, Variable> variableByRBPOffset = new();
    protected Stack<Variable> variableStack = new();
    private int anonVariableNameIndex;
    private int addressOfHeapSize = 0x100; // On this address located a long, that describes the heap size
    private int heapBaseAddress = 0x110; // The start (zero address) of heap
    private int thrownExceptionAddress = 0x104000;
    public virtual int ExceptionPointerAddress => thrownExceptionAddress;
    private int exceptionsHandlersStackAddress => thrownExceptionAddress + 8;

    public virtual void Label(string labelName)
    {
        if (b.labels.Contains(labelName) == false)
        {
            throw new Exception("Failed to generate label code because label '' is not registered yet. You must register label before it's generation.");
        }

        b.Line(labelName + ":");
    }

    public virtual string RegisterLabel(string labelName)
    {
        if (b.labels.Contains(labelName))
        {
            labelName = NextLabel(labelName, 2);
        }

        b.labels.Add(labelName);
        return labelName;
    }

    private string NextLabel(string originalLabelName, int i)
    {
        string uniqueLabelName = originalLabelName + "_" + i;
        if (b.labels.Contains(uniqueLabelName))
        {
            return NextLabel(originalLabelName, i + 1);
        }
        return uniqueLabelName;
    }

    public virtual void Prologue()
    {
        b.Line("push rbp");
        b.Line("mov rbp, rsp");
    }

    public virtual void Epilogue()
    {
        b.Line("mov rsp, rbp");
        b.Line("pop rbp");
    }

    public virtual void PrologueForSimulation(CompileTarget target)
    {
        if (target == CompileTarget.Simulator)
        {
            b.Line("mov rbx, 0");
            b.Line("push rbx ; return int");

            b.Line("call program.main");

            b.Line("add rsp, 8");
            b.Line("pop rax");

            b.Line("mov 0x00, rax");
            b.Line("exit");
        }
        else
        {
            b.Line("global main");
            b.Space();
            b.Line("main:");
            b.Line("call program.main");
            b.Line("halt:");
            b.Line("hlt");
            b.Line("jmp halt");
            b.Space(3);
        }
        
        b.Space(2);
    }

    public virtual void Return_Void()
    {
        b.Line("ret");
    }

    public virtual void Return_Variable(FunctionInfo function, Variable variable)
    {
        int rbpOffset = 16 + function.arguments.Count * 8;
        if (function.isStatic == false) rbpOffset += 8;
        
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"mov [rbp+{rbpOffset}], rbx");
    }

    public virtual void Return_Field(FunctionInfo function, Variable variable)
    {
        int rbpOffset = 16 + function.arguments.Count * 8;
        if (function.owner != null) rbpOffset += 8;

        b.Line($"mov rbx, {variable.RBP} ; rbx - address of address to primitive");
        // b.Line($"mov rbx, [rbx] ; rbx - address to primitive");
        b.Line($"mov [rbp+{rbpOffset}], [rbx] ; [rbx] - value of primitive");
    }

    public virtual Variable Allocate(TypeInfo type)
    {
        anonVariableNameIndex++;
        return Allocate(type, $"anon_{anonVariableNameIndex}");
    }

    public virtual Variable Allocate(TypeInfo type, string name)
    {
        if (type == null)
        {
            throw new Exception("Failed to allocated variable with null type.");
        }
        
        int sizeInBytes = 8;

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
        
        b.Line($"sub rsp, {sizeInBytes} ; allocate {variable.type} '{variable.name}' at {variable.RBP}");

        return variable;
    }

    public virtual int AllocateRSPSaver()
    {
        rbpOffset -= 8;
        b.Line($"push rsp ; allocate saver at [rbp{rbpOffset}]");
        return rbpOffset;
    }

    public virtual void RestoreRSPSaver(int saverRBPOffset)
    {
        b.Line($"mov rsp, [rbp{saverRBPOffset}] ; restore saver");
    }

    public virtual void DeallocateRSPSaver()
    {
        rbpOffset -= 8;
        b.Line($"pop rsp ; deallocate saver");
    }

    public virtual void Deallocate(Variable variable)
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
        variableByRBPOffset.Remove(variable.rbpOffset);
        variableStack.Pop();

        int sizeInBytes = 8;
        b.CommentLine($"return allocation");
        b.Line($"add rsp, {sizeInBytes}");
    }

    public virtual void Deallocate(int sizeInBytes)
    {
        b.Line($"add rsp, {sizeInBytes}");
    }

    public virtual void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable)
    {
        AllocateHeap(storageOfPointerToHeap);
        
        b.Line($"mov rbx, [{addressOfHeapSize}]"); 
        b.Line($"mov rdx, {bytesToAllocateVariable.RBP}");
        b.Line($"add rbx, [rdx]"); // TODO: Why double depoint required? Think about that in daytime
        b.Line($"mov [{addressOfHeapSize}], rbx");
    }

    public virtual void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate)
    {
        AllocateHeap(storageOfPointerToHeap);
        
        b.Line($"mov rbx, [{addressOfHeapSize}]");
        b.Line($"add rbx, {bytesToAllocate}");
        b.Line($"mov [{addressOfHeapSize}], rbx");
    }

    private void AllocateHeap(Variable storageOfPointerToHeap)
    {
        b.CommentLine($"heap alloc");
        b.Line($"mov rbx, [{addressOfHeapSize}]"); // rbx - next heap byte address
        
        b.Line($"mov rdx, {heapBaseAddress}");
        b.Line($"add rdx, rbx");
        
        b.Line($"mov qword {storageOfPointerToHeap.RBP}, rdx");
    }

    public virtual Variable Register_FunctionArgumentVariable(FieldInfo info, int index)
    {
        Variable variable = new Variable()
        {
            name = info.name,
            type = info.type,
            rbpOffset = 16 + index * 8
        };
        
        variableByName.Add(variable.name, variable);
        variableByRBPOffset.Add(variable.rbpOffset, variable);

        return variable;
    }

    public virtual void Unregister_FunctionArgumentVariable(Variable variable)
    {
        variableByName.Remove(variable.name);
        variableByRBPOffset.Remove(variable.rbpOffset);
    }

    public virtual void SetValue(Variable variable, string value)
    {
        b.Line($"mov qword {variable.RBP}, {value}");
    }

    public virtual void SetValue(Variable destination, Variable value)
    {
        b.Line($"mov qword rbx, {value.RBP}");
        b.Line($"mov qword {destination.RBP}, rbx");
    }

    public virtual void SetValue(Variable destination, int address)
    {
        b.Line($"mov qword rbx, [{address}]");
        b.Line($"mov qword {destination.RBP}, rbx");
    }

    public virtual void SetValueBehindPointer(Variable destination, Variable value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov rdx, {value.RBP}");
        b.Line($"mov qword [rbx], rdx");
    }

    public virtual void SetValueBehindPointer(Variable destination, string value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov qword [rbx], {value}");
    }

    public virtual void SetValueFromReg(Variable destination, string sourceReg)
    {
        b.Line($"mov {destination.RBP}, {sourceReg}");
    }

    public virtual void SetValueToReg(string destReg, Variable source)
    {
        b.Line($"mov {destReg}, {source.RBP}");
    }

    public virtual void FieldAccess(int baseOffset, TypeInfo fieldType, int fieldOffset, Variable result, bool isGetter)
    {
        if (fieldOffset < 0) throw new Exception("Negative fieldOffset is not allowed.");
        
        // Load from ram address to ref-type inside heap
        string rbp = baseOffset > 0 ? "+" + baseOffset : baseOffset.ToString();
        b.Line($"mov rbx, [rbp{rbp}]");
        
        // If we accessing not first field
        if (fieldOffset != 0)
        {
            // Add offset to rbx to go to field inside ref-type
            b.Line($"add rbx, {fieldOffset}");
        }
        
        // Now rbx is pointing to valid address of ref-type.field
        
        // If we don't need a pointer (like setter), but want to get a value (like getter)
        if (isGetter)
        {
            // Depoint rbx to get actual field value
            
            string nasmType = Utils.GetNASMType(fieldType);
            b.Line($"mov {nasmType} {Utils.ClampRegister(nasmType, "rbx")}, [rbx] ; depoint one more time due to getter");
            
            // b.Line($"mov rbx, [rbx] ; depoint one more time due to getter");
        }
        
        // Put result (ref for setter and value for getter) inside result variable
        b.Line($"mov {result.RBP}, rbx");
        
        
    }

    public virtual void JumpIfFalse(Variable condition, string label)
    {
        b.Line($"mov rbx, {condition.RBP}");
        JumpIfFalse("rbx", label);
    }

    public virtual void JumpIfFalse(string reg, string label)
    {
        b.Line($"cmp {reg}, 0");
        b.Line($"jle {label}");
    }

    public virtual void JumpToLabel(string label)
    {
        b.Line($"jmp {label}");
    }

    public virtual void Compare(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        this.b.Line($"mov rbx, {a.RBP}"); 
        this.b.Line($"mov rdx, {b.RBP}"); 
        
        this.b.Line($"cmp rbx, rdx");
        this.b.Line($"mov rbx, 0");
        this.b.Line($"set{@operator.asmOperatorName} bl");
        
        this.b.Line($"mov {result.RBP}, rbx");
    }

    public virtual void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        if (@operator is Token_Factor)
        {
            if (@operator.asmOperatorName == "mul")
            {
                this.b.Line($"mov rdi, {a.RBP}");
                this.b.Line($"mov rax, {b.RBP}");
                this.b.Line($"mul rdi");
                this.b.Line($"mov {result.RBP}, rax");
            }
            else if (@operator.asmOperatorName == "div" || @operator.asmOperatorName == "%")
            {
                // div operator:
                // rax / N
                // quotient inside rax
                // remainder inside N
                this.b.Line($"mov rax, {a.RBP}");
                this.b.Line($"mov rcx, {b.RBP}");
                this.b.Line($"mov rdx, 0"); // clear rdx, because it used as extension for rdx:rax

                this.b.Line($"div rcx");

                if (@operator.asmOperatorName == "div")
                {
                    this.b.Line($"mov {result.RBP}, rax"); // take quotient
                }
                else
                {
                    this.b.Line($"mov {result.RBP}, rdx"); // take remainder
                }
            }
            else
            {
                throw new Exception($"Unknown Token_Factor operator '{@operator.asmOperatorName}'");
            }
        }
        else if (@operator is Token_BitOperator)
        {
            this.b.Line($"mov rdx, {a.RBP}");
            
            if (@operator.asmOperatorName == ">>" || @operator.asmOperatorName == "<<")
            {
                this.b.Line($"mov rcx, {b.RBP}");
                if (@operator.asmOperatorName == ">>")
                {
                    this.b.Line($"shr rdx, cl");
                }
                else
                {
                    this.b.Line($"shl rdx, cl"); 
                }
            }
            else
            {
                this.b.Line($"mov rbx, {b.RBP}");
                if (@operator.asmOperatorName == "&")
                {
                    this.b.Line($"and rdx, rbx");
                }
                else
                {
                    this.b.Line($"or rdx, rbx"); 
                }
            }
            this.b.Line($"mov {result.RBP}, rdx");
        }
        else
        {
            this.b.Line($"mov rbx, {a.RBP}");
            this.b.Line($"mov rdx, {b.RBP}");

            this.b.Line($"{@operator.asmOperatorName} rbx, rdx");

            this.b.Line($"mov {result.RBP}, rbx");
        }
    }

    public virtual void LogicalNOT(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"test rbx, rbx");
        b.Line($"xor rbx, rbx"); // reset rbx to zero
        b.Line($"sete bl"); // set last byte of reg to 1 or 0
        b.Line($"mov {result.RBP}, rbx");
    }

    public virtual void Negate(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"neg rbx");
        b.Line($"mov {result.RBP}, rbx");
    }

    public virtual void ToPtr_Primitive(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name}");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov {result.RBP}, rbx");
    }

    public virtual void ToPtr_Heap(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name} (heap data)");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov rbx, [rbx]");
        b.Line($"mov {result.RBP}, rbx");
    }

    public virtual void PtrAddress(Variable pointer, Variable result, bool isGetter)
    {
        b.Space();
        b.CommentLine($"{pointer.name}.address");
        
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {pointer.rbpOffset} ; offset to target ptr data cell");
        if (isGetter) b.Line($"mov rbx, [rbx]");
        b.Line($"mov {result.RBP}, rbx ; now {result.RBP} is pointer to {pointer.name} (.address)");
    }

    public virtual void PtrGet(Variable pointerVariable, Variable result)
    {
        string nasmType = Utils.GetNASMType(result.type);
        
        Comment($"Ptr get");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, [rbx]");
        b.Line($"mov {nasmType} {result.RBP}, {Utils.ClampRegister(nasmType)}");
    }

    public virtual void PtrSet(Variable pointerVariable, Variable targetVariable)
    {
        string nasmType = Utils.GetNASMType(targetVariable.type);
        
        Comment("Ptr set");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {targetVariable.RBP}");
        b.Line($"mov {nasmType} [rbx], {Utils.ClampRegister(nasmType)}");
    }

    public virtual void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0)
    {
        Comment("Ptr shift");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {shiftVariable.RBP}");
        b.Line($"add rbx, rdx");
        if (additionalShift != 0) b.Line($"add rbx, {additionalShift} ; additionalShift");
        b.Line($"mov {pointerVariable.RBP}, rbx");
    }

    public virtual void PtrShift(Variable pointerVariable, int shift)
    {
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"add rbx, {shift}");
        b.Line($"mov {pointerVariable.RBP}, rbx");
    }

    public virtual void Print(Variable variable)
    {
        b.Space();
        b.CommentLine($"print {variable.name}");
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"print [rbx]");
    }

    public virtual void CalculateAddress_RBP_Shift(Variable shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes.RBP}");
        SetValueFromReg(result, "rbx");
    }

    public virtual void CalculateAddress_RBP_Shift(int shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes}");
        SetValueFromReg(result, "rbx");
    }

    public virtual void PushToStack(Variable variable, string comment = null)
    {
        PushToStack(variable.RBP, comment);
    }

    public virtual void PushToStack(string value, string comment = null)
    {
        b.Line($"mov rbx, {value}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
        b.Line($"push rbx");
    }

    public virtual void PushRegToStack(string regName, string comment = null)
    {
        b.Line($"push {regName}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
    }

    public virtual void PopRegFromStack(string regName, string comment = null)
    {
        b.Line($"pop {regName}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
    }

    public virtual void Call(string functionName)
    {
        b.Line($"call {functionName}");
    }

    public virtual void PushExceptionHandler(string catchLabel)
    {
        string addressOfStackSize = $"0x{exceptionsHandlersStackAddress.ToString("x")}";
        string addressOfStackBegin = $"0x{(exceptionsHandlersStackAddress + 8).ToString("x")}";
        
        b.Space();
        b.CommentLine($"Push exception handler: catch '{catchLabel}'");
        b.Line($"mov rbx, [{addressOfStackSize}]");
        
        b.Line($"mov rdx, rbx");
        b.Line($"add rdx, {addressOfStackBegin}");
        b.Line($"mov qword [rdx], {catchLabel}");
        
        b.Line($"add rbx, 8");
        b.Line($"mov [{addressOfStackSize}], rbx");
        b.Space();
    }

    public virtual void ThrowException(Variable exception)
    {
        b.Space();
        b.CommentLine($"Throw exception");
        
        string addressOfStackSize = $"0x{exceptionsHandlersStackAddress.ToString("x")}";
        string addressOfStackBegin = $"0x{(exceptionsHandlersStackAddress + 8).ToString("x")}";
        string addressOfException = $"0x{(thrownExceptionAddress).ToString("x")}";
        
        // Place exception into memory
        b.Line($"mov rdx, {addressOfException}");
        b.Line($"mov rbx, {exception.RBP}");
        b.Line($"mov [rdx], rbx");
        
        // Pop stack
        b.Line($"mov rbx, [{addressOfStackSize}]");
        
        // b.Line($"mov rbx, rdx");
        b.Line($"sub rbx, 8");
        b.Line($"mov [{addressOfStackSize}], rbx");
        
        b.Line($"add rbx, {addressOfStackBegin}");
        b.Line($"mov rbx, [rbx]");
        
        b.Line($"jmp rbx");
        b.Space();
    }

    public virtual void SectionData()
    {
        b.Line("section .data");
    }

    public virtual void SectionText()
    {
        b.Line("section .text");
    }

    public virtual void BufferString(string id, string value)
    {
        b.Line($"{id} db {value.Length}, \"{value}\", 0");
    }

    public virtual void Extern(string variableName)
    {
        b.Line($"extern {variableName}");
    }

    public virtual void Space(int lines = 1)
    {
        b.Space(lines);
    }

    public virtual void Comment(string comment)
    {
        b.CommentLine(comment);
    }

    public virtual void Comment(string comment, int bookmarkDistance)
    {
        List<char> chars = new();

        for (int i = 0; i < bookmarkDistance; i++)
        {
            for (int d = 0; d < 4; d++)
            {
                chars.Add('-');
            }
        }

        chars.Add(' ');
        chars.AddRange(comment);

        b.CommentLine(string.Concat(chars));
    }

    public virtual Variable GetVariable(string name)
    {
        if (variableByName.TryGetValue(name, out Variable var)) return var;

        if (parent != null) return parent.GetVariable(name);
        
        throw new Exception($"Variable '{name}' not found in scope");
    }

    public abstract byte[] Build();
}