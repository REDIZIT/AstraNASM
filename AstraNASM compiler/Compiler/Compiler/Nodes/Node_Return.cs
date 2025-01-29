using System;

namespace Astra.Compilation
{
    public class Node_Return : Node
    {
        public Node expr;
        public FunctionInfo function;

        public override void RegisterRefs(RawModule module)
        {
            expr?.RegisterRefs(module);
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            expr?.ResolveRefs(module);
        }
        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            ctx.b.Space(2);

            if (function.returns.Count > 0)
            {
                if (function.returns.Count > 1) throw new Exception("Not supported yet");

                int rbpOffset = 16 + function.arguments.Count * 8;
                if (function.owner != null) rbpOffset += 8;

                if (expr != null)
                {
                    expr.Generate(ctx);

                    ctx.b.Space(1);

                    if (expr is Node_FieldAccess)
                    {
                        ctx.b.Line($"mov rax, {expr.result.GetRBP()}");
                        ctx.b.Line($"mov [rbp+{rbpOffset}], [rax]");
                    }
                    else
                    {
                        ctx.b.Line($"mov rax, {expr.result.GetRBP()}");
                        ctx.b.Line($"mov [rbp+{rbpOffset}], rax");
                    }
                }
                else
                {
                    throw new Exception("Syntax error: Function has return type, but return node does not have any expression to return.");
                }
            }
        

            ctx.b.Line("mov rsp, rbp");
            ctx.b.Line("pop rbp");
            ctx.b.Line("ret");
        }
    }
}