namespace Astra.Compilation;
public class Node_While : Node
{
    public Node condition, body;

    public Scope_StaticAnalysis staticScope;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return condition;
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);


        string endLabel = ctx.gen.RegisterLabel("while_end");
        string conditionLabel = ctx.gen.RegisterLabel("while_condition");


        ctx.gen.Label(conditionLabel);
        
        ctx.gen.BeginSubScope();
        condition.Generate(ctx);
        
        ctx.gen.JumpIfFalse(condition.result, endLabel);
        
        body.Generate(ctx);
        ctx.gen.DropSubScope();
        
        ctx.gen.JumpToLabel(conditionLabel);


        ctx.gen.Space();
        ctx.gen.Label(endLabel);
        ctx.gen.Epilogue(); // due to runtime JumpIfFalse keeps SubScope openned
    }
}