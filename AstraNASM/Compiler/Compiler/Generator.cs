﻿
public class Variable
{
    public string name;
    public TypeInfo type;
    public int rbpOffset;

    public string RBP
    {
        get
        {
            if (rbpOffset > 0) return $"[rbp+{rbpOffset}]";
            else return $"[rbp{rbpOffset}]";
        }
    }

    public string GetRBP()
    {
        return RBP;
    }
}

public static class Generator
{
    public class Context
    {
        public CodeStringBuilder b = new();
        public HashSet<string> stackVariables = new();
        public HashSet<string> tempVariables = new();
        public int tempVariablesCount = 0;
        public int localVariablesCount = 0;

        public Dictionary<string, TypeInfo> typeByVariableName = new();
        public Dictionary<string, TypeInfo> pointedTypeByVariableName = new();

        public ResolvedModule module;



        private List<Variable> localVariables = new();

        public int lastLocalVariableIndex = 0;
        public int lastAnonVariableIndex = 0;

        public Context parent;


        public Variable Register_FunctionArgumentVariable(FieldInfo info, int index)
        {
            Variable variable = new Variable()
            {
                name = info.name,
                type = info.type,
                rbpOffset = index * 8 + 16, // index * 8 (arguments sizeInBytes) + 16 (pushed rbp (8 bytes) + pushed rsi (8 bytes)
            };
            localVariables.Add(variable);

            return variable;
        }
        public Variable AllocateStackVariable(TypeInfo type, string name = null)
        {
            if (name == null)
            {
                name = NextStackAnonVariableName();
            }

            lastLocalVariableIndex -= 8;

            Variable variable = new Variable()
            {
                name = name,
                type = type,
                rbpOffset = lastLocalVariableIndex,
            };
            localVariables.Add(variable);

            return variable;
        }
        public string NextStackAnonVariableName()
        {
            lastAnonVariableIndex++;
            return "anon_" + lastAnonVariableIndex;
        }
        public Variable GetVariable(string name)
        {
            Variable var = localVariables.FirstOrDefault(v => v.name == name);
            if (var != null) return var;

            if (parent != null) return parent.GetVariable(name);

            throw new Exception($"Variable '{name}' not found in context");
        }


        public Context CreateSubContext()
        {
            Context ctx = new();
            ctx.parent = this;
            ctx.b = b;

            return ctx;
        }


        public string NextTempVariableName(TypeInfo type)
        {
            throw new Exception("Depercated");

            string varName = $"%tmp_{tempVariablesCount}_{type.name}";
            tempVariablesCount++;
            tempVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }

        public bool IsPointer(string generatedName)
        {
            return typeByVariableName[generatedName] == PrimitiveTypeInfo.PTR;
        }

        public TypeInfo GetVariableType(string variableName)
        {
            return typeByVariableName[variableName];
        }
        public TypeInfo GetPointedType(string pointerVariableName)
        {
            if (pointedTypeByVariableName.ContainsKey(pointerVariableName) == false)
            {
                throw new Exception($"Failed to get type behind the pointer variable named '{pointerVariableName}'. This variable is not a pointer or not defined at all.");
            }
            return pointedTypeByVariableName[pointerVariableName];
        }
    }

    public static string Generate(List<Node> statements, ResolvedModule module)
    {
        Context ctx = new()
        {
            module = module
        };

        //ctx.b.Line($";");
        //ctx.b.Line($"; Structs");
        //ctx.b.Line($";");
        //foreach (ClassTypeInfo info in module.classInfoByName.Values)
        //{
        //    string typesStr = string.Join(", ", info.fields.Select(f => f.type.ToString()));
        //    ctx.b.Line($"%{info.name} = type {{ {typesStr} }}");
        //}

        //ctx.b.Space(2);

        //ctx.b.Line(";");
        //ctx.b.Line("; Methods");
        //ctx.b.Line(";");

        ctx.b.Line("call main");
        ctx.b.Line("mov 0x00, rax");
        ctx.b.Line("exit");

        ctx.b.Space(2);

        foreach (Node statement in statements)
        {
            statement.Generate(ctx);
        }

        return FormatNASM(ctx.b.BuildString());
    }


    private static string FormatLLVM(string llvm)
    {
        int depth = 0;

        string[] lines = llvm.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.Contains("}"))
            {
                depth--;
            }
            
            if (depth > 0 && line.Contains(":") == false)
            {
                lines[i] = '\t' + line;
            }

            if (line.Contains("{"))
            {
                depth++;
            }
        }

        return string.Join('\n', lines);
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
