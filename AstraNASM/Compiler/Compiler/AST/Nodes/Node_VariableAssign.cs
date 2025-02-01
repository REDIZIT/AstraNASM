namespace Astra.Compilation;

public class Node_VariableAssign : Node
{
    public Node target; // variable or field get
    public Node value;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
        yield return value;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        
        ctx.gen.Comment("generating target for assign");
        if (target is Node_FieldAccess targetField)
        {
            targetField.GenerateAsSetter(ctx);
        }
        else
        {
            target.Generate(ctx);
        }
        // target.Generate(ctx);
        
        ctx.gen.Comment("generating value for assign");
        value.Generate(ctx);
        
        
        ctx.gen.Space();
        ctx.gen.Comment($"Assign {target.result.name} = {(value.result.name)}");
        // ctx.gen.SetValue(target.result, value.result);

        if (target is Node_FieldAccess)
        {
            ctx.gen.SetValueToField(target.result, value.result);
        }
        else
        {
            ctx.gen.SetValue(target.result, value.result);
        }
    }
}