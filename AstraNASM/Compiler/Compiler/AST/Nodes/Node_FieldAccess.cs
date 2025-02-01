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

        GenerateInternal(ctx, false);
    }

    public void GenerateAsSetter(Generator.Context ctx)
    {
        GenerateInternal(ctx, true);
    }

    private void GenerateInternal(Generator.Context ctx, bool isSetter)
    {
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
            int totalOffset = target.result.rbpOffset + fieldOffsetInBytes;

            ctx.gen.Space();
            ctx.gen.Comment($"{target.result.name}.{targetFieldName}");
            result = ctx.gen.Allocate(PrimitiveTypes.PTR);

            ctx.gen.FieldAccess(totalOffset, result, target, !isSetter);
        }
    }
}