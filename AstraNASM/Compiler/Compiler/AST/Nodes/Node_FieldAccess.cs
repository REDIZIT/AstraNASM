namespace Astra.Compilation;

public class Node_FieldAccess : Node
{
    public Node target;
    public string targetFieldName;

    public FieldInfo field;


    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);

        if (field is EmbeddedFieldInfo)
        {
            if (field is PtrAddress_EmbeddedFieldInfo ptrAddress)
            {
                result = ctx.gen.Allocate(PrimitiveTypes.PTR);
                ctx.gen.PtrAddress(target.result, this.result);
            }
            else
            {
                throw new Exception($"Failed to generate unknown EmbeddedFieldInfo '{field}'");
            }
        }
        else
        {
            int fieldOffsetInBytes = 0;

            result = ctx.gen.Allocate(PrimitiveTypes.PTR);

            int totalOffset = target.result.rbpOffset + fieldOffsetInBytes;


            ctx.gen.Space();
            ctx.gen.Comment($"{target.result.name}.{targetFieldName}");

            if (target is Node_FieldAccess)
            {
                // ctx.b.Line($"mov rbx, [rbp{totalOffset}]");
                // ctx.b.Line($"mov {result.GetRBP()}, rbx");
                
                ctx.gen.RBP_Shift_And_LoadFromRAM(totalOffset, result);
            }
            else
            {
                // ctx.b.Line($"mov rbx, rbp");
                // ctx.b.Line($"add rbx, {totalOffset}");
                // ctx.b.Line($"mov {result.GetRBP()}, rbx");

                ctx.gen.CalculateAddress_RBP_Shift(totalOffset, result);
            }

            // ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }
    }
}