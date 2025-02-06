namespace Astra.Compilation;
public class Node_While : Node
{
    public Node condition, body;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return condition;
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);


        throw new Exception("Not fixed");

        body.Generate(ctx);



        //////string conditionLabel = ctx.gen.RegisterLabel("while_condition");
        //////string endLabel = ctx.gen.RegisterLabel("while_end");

        ////////ctx.gen.Comment("Allocate rsp saver");
        ////////Variable rspSaver = ctx.gen.Allocate(TypeIn)
        //////int rspRBPOffset = ctx.gen.AllocateRSPSaver();

        //////ctx.gen.Label(conditionLabel);

        ////////ctx.gen.Prologue();
        ////////ctx.gen.PushRegToStack("rsp");

        ////////ctx.gen.Comment("isolated condition body");
        //////condition.Generate(ctx);
        ////////ctx.gen.SetValueToReg("rbx", condition.result);

        ////////ctx.gen.Epilogue();
        ////////ctx.gen.PopRegFromStack("rsp");


        //////ctx.gen.JumpIfFalse(condition.result, endLabel);
        ////////ctx.gen.JumpIfFalse("rbx", endLabel);

        //////body.Generate(ctx);


        //////ctx.gen.RestoreRSPSaver(rspRBPOffset);
        //////ctx.gen.JumpToLabel(conditionLabel);

        //////ctx.gen.Label(endLabel);
        //////ctx.gen.DeallocateRSPSaver();
    }
}