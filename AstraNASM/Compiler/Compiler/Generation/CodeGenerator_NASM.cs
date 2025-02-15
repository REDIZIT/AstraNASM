using System.Text;

namespace Astra.Compilation;

public class CodeGenerator_NASM : CodeGeneratorBase
{
    public override void PrologueForSimulation(CompileTarget target)
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
    
    public override void Prologue()
    {
        b.Line("push rbp");
        b.Line("mov rbp, rsp");
    }

    public override void Epilogue()
    {
        b.Line("mov rsp, rbp");
        b.Line("pop rbp");
    }
    
    public override void Return_Void()
    {
        b.Line("ret");
    }
    public override void Return_Variable(FunctionInfo function, Variable variable)
    {
        int rbpOffset = 16 + function.arguments.Count * 8;
        if (function.isStatic == false) rbpOffset += 8;
        
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"mov [rbp+{rbpOffset}], rbx");
    }

    public override Variable Allocate(TypeInfo type, string name)
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
    
    public override void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable)
    {
        AllocateHeap(storageOfPointerToHeap);
        
        b.Line($"mov rbx, [{addressOfHeapSize}]"); 
        b.Line($"mov rdx, {bytesToAllocateVariable.RBP}");
        b.Line($"add rbx, [rdx]"); // TODO: Why double depoint required? Think about that in daytime
        b.Line($"mov [{addressOfHeapSize}], rbx");
    }

    public override void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate)
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
    
    public override void PushToStack(Variable variable, string comment = null)
    {
        PushToStack(variable.RBP, comment);
    }

    public override void PushToStack(string value, string comment = null)
    {
        b.Line($"mov rbx, {value}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
        b.Line($"push rbx");
    }
    
    public override void Deallocate(int sizeInBytes)
    {
        b.Line($"add rsp, {sizeInBytes}");
    }
    
    public override void SetValue(Variable variable, string value)
    {
        b.Line($"mov qword {variable.RBP}, {value}");
    }
    
    public override void SetValue(Variable destination, Variable value)
    {
        b.Line($"mov qword rbx, {value.RBP}");
        b.Line($"mov qword {destination.RBP}, rbx");
    }
    
    public override void SetValue(Variable destination, int address)
    {
        b.Line($"mov qword rbx, [{address}]");
        b.Line($"mov qword {destination.RBP}, rbx");
    }
    
    public override void SetValueBehindPointer(Variable destination, Variable value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov rdx, {value.RBP}");
        b.Line($"mov qword [rbx], rdx");
    }

    public override void SetValueBehindPointer(Variable destination, string value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov qword [rbx], {value}");
    }
    
    public override void FieldAccess(int baseOffset, TypeInfo fieldType, int fieldOffset, Variable result, bool isGetter)
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
    
    public override void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result)
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
    public override void Negate(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"neg rbx");
        b.Line($"mov {result.RBP}, rbx");
    }
    
    public override void Call(string functionName)
    {
        b.Line($"call {functionName}");
    }
    
    public override void ToPtr_Primitive(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name}");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov {result.RBP}, rbx");
    }
    public override void ToPtr_Heap(Variable askedVariable, Variable result)
    {
        b.CommentLine($"ToPtr {askedVariable.name} (heap data)");
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {askedVariable.rbpOffset}");
        b.Line($"mov rbx, [rbx]");
        b.Line($"mov {result.RBP}, rbx");
    }
    
    public override void PtrAddress(Variable pointer, Variable result, bool isGetter)
    {
        b.Space();
        b.CommentLine($"{pointer.name}.address");
        
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {pointer.rbpOffset} ; offset to target ptr data cell");
        if (isGetter) b.Line($"mov rbx, [rbx]");
        b.Line($"mov {result.RBP}, rbx ; now {result.RBP} is pointer to {pointer.name} (.address)");
    }

    public override void PtrGet(Variable pointerVariable, Variable result)
    {
        string nasmType = Utils.GetNASMType(result.type);
        
        Comment($"Ptr get");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, [rbx]");
        b.Line($"mov {nasmType} {result.RBP}, {Utils.ClampRegister(nasmType)}");
    }

    public override void PtrSet(Variable pointerVariable, Variable targetVariable)
    {
        string nasmType = Utils.GetNASMType(targetVariable.type);
        
        Comment("Ptr set");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {targetVariable.RBP}");
        b.Line($"mov {nasmType} [rbx], {Utils.ClampRegister(nasmType)}");
    }

    public override void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0)
    {
        Comment("Ptr shift");
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"mov rdx, {shiftVariable.RBP}");
        b.Line($"add rbx, rdx");
        if (additionalShift != 0) b.Line($"add rbx, {additionalShift} ; additionalShift");
        b.Line($"mov {pointerVariable.RBP}, rbx");
    }

    public override void PtrShift(Variable pointerVariable, int shift)
    {
        b.Line($"mov rbx, {pointerVariable.RBP}");
        b.Line($"add rbx, {shift}");
        b.Line($"mov {pointerVariable.RBP}, rbx");
    }


    public override void Space(int lines = 1)
    {
        b.Space(lines);
    }
    public override void Comment(string comment)
    {
        b.CommentLine(comment);
    }
    public override void Comment(string comment, int bookmarkDistance)
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
    
    public override void Compare(Variable a, Variable b, Token_Operator @operator, Variable result)
    {
        this.b.Line($"mov rbx, {a.RBP}"); 
        this.b.Line($"mov rdx, {b.RBP}"); 
        
        this.b.Line($"cmp rbx, rdx");
        this.b.Line($"mov rbx, 0");
        this.b.Line($"set{@operator.asmOperatorName} bl");
        
        this.b.Line($"mov {result.RBP}, rbx");
    }
    
    public override void JumpIfFalse(Variable condition, string label)
    {
        b.Line($"mov rbx, {condition.RBP}");
        JumpIfFalse("rbx", label);
    }

    public override void JumpIfFalse(string reg, string label)
    {
        b.Line($"cmp {reg}, 0");
        b.Line($"jle {label}");
    }

    public override void JumpToLabel(string label)
    {
        b.Line($"jmp {label}");
    }


    public override byte[] Build()
    {
        string nasm = FormatNASM(string.Join("\n", b.BuildString()));
        return Encoding.UTF8.GetBytes(nasm);
    }
    
    private static string FormatNASM(string nasm)
    {
        string[] lines = nasm.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.Contains(":") == false && line.StartsWith(';') == false)
            {
                lines[i] = '\t' + line;
            }
        }

        return string.Join('\n', lines);
    }
}