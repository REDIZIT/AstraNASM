
namespace Astra.Compilation;

public class Node_Return : Node
{
    public Node expr;
    public FunctionInfo function;

    public override IEnumerable<Node> EnumerateChildren()
    {
        if (expr != null) yield return expr;
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
                    ctx.b.Line($"mov rbx, {expr.result.GetRBP()}");
                    ctx.b.Line($"mov [rbp+{rbpOffset}], [rbx]");
                }
                else
                {
                    ctx.b.Line($"mov rbx, {expr.result.GetRBP()}");
                    ctx.b.Line($"mov [rbp+{rbpOffset}], rbx");
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