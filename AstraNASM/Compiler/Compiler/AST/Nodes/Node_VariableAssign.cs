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
            // We're assigning value to field.
            // That means, that we should generate pointer (setter), but don't depoint it (getter)
            targetField.GenerateAsSetter(ctx);
        }
        else
        {
            target.Generate(ctx);
        }
        
        
        ctx.gen.Comment("generating value for assign");

        if (value is Node_Literal)
        {
            // Do not generate literal for shortcut
        }
        else
        {
            value.Generate(ctx);    
        }
        
        ctx.gen.Space();
        

        if (target is Node_FieldAccess)
        {
            if (value is Node_Literal lit)
            {
                // Shortcut
                ctx.gen.Comment($"Assign {target.result.name} = {lit.constant.value}");
                ctx.gen.SetValueBehindPointer(target.result, lit.constant.value);
            }
            else
            {
                ctx.gen.Comment($"Assign {target.result.name} = {(value.result.name)}");
                ctx.gen.SetValueBehindPointer(target.result, value.result);
            }
        }
        else
        {
            if (value is Node_Literal lit)
            {
                // Shortcut
                ctx.gen.Comment($"Assign {target.result.name} = {lit.constant.value}");
                ctx.gen.SetValue(target.result, lit.constant.value);
            }
            else
            {
                ctx.gen.Comment($"Assign {target.result.name} = {(value.result.name)}");
                ctx.gen.SetValue(target.result, value.result);
            }
        }
    }
}