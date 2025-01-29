using System;
using System.Collections.Generic;
using System.Linq;

namespace Astra.Compilation
{
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
            public Context parent;

            public CodeStringBuilder b = new CodeStringBuilder();
            public ResolvedModule module;


            private List<Variable> localVariables = new List<Variable>();

            private int lastLocalVariableIndex = 0;
            private int lastAnonVariableIndex = 0;


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
                Context ctx = new Context()
                {
                    parent = this,
                    b = b,
                    module = module,
                };

                return ctx;
            }

            public void Release(Variable variable)
            {
                if (localVariables.Contains(variable) == false)
                {
                    throw new Exception("Failed to release variable due to variable is not presented in current context");
                }

                lastLocalVariableIndex += 8; // sizeInBytes
            }
        }

        public static string Generate(List<Node> statements, ResolvedModule module)
        {
            Context ctx = new Context()
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
}
