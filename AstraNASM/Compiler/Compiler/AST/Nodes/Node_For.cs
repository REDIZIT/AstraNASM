namespace Astra.Compilation;

public class Node_For : Node
{
    public Node declaration, condition, advance, body;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return declaration;
        yield return condition;
        yield return advance;
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string endLabel = ctx.gen.RegisterLabel("for_end");
        string conditionLabel = ctx.gen.RegisterLabel("for_condition");


        ctx.gen.BeginSubScope();
        declaration.Generate(ctx);
        
        
        ctx.gen.Label(conditionLabel);
        ctx.gen.BeginSubScope();
        condition.Generate(ctx);
        // ctx.gen.DropSubScope();
        
        ctx.gen.JumpIfFalse(condition.result, endLabel);
        
        
        // ctx.gen.BeginSubScope();
        body.Generate(ctx);
        advance.Generate(ctx);
        ctx.gen.DropSubScope();
        
        ctx.gen.JumpToLabel(conditionLabel);


        ctx.gen.Label(endLabel);
        ctx.gen.DropSubScope(); // due to compiletime Begin/Drop same amount requirement
        ctx.gen.Epilogue(); // due to runtime JumpIfFalse keeps SubScope openned
    }
}