
namespace Astra.Compilation;

public class Node_VariableDeclaration : Node
{
    public VariableRawData variable;
    public Node initValue;

    public ClassTypeInfo ownerInfo;
    public FieldInfo fieldInfo;

    public override IEnumerable<Node> EnumerateChildren()
    {
        if (initValue != null) yield return initValue;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

       

        if (initValue == null)
        {
            Generate_WithDefaultValue(ctx);
        }
        else if (initValue is Node_Literal literal)
        {
            Generate_WithInit_Literal(ctx, literal);
        }
        else if (initValue is Node_New tokenNew)
        {
            Generate_WithInit_New(ctx, tokenNew);
        }
        else
        {
            Generate_WithInit_AnyExpression(ctx);
        }

        ctx.gen.Space();
    }

    private void Generate_WithDefaultValue(Generator.Context ctx)
    {
        TypeInfo type = ctx.module.GetType(variable.rawType);
        
        result = ctx.gen.Allocate(type, variable.name);
        ctx.gen.SetValue(result, "0");
    }
    private void Generate_WithInit_Literal(Generator.Context ctx, Node_Literal literal)
    {
        TypeInfo type = ctx.module.GetType(variable.rawType);
        
        result = ctx.gen.Allocate(type, variable.name);
        ctx.gen.SetValue(result, literal.constant.value);
    }
    private void Generate_WithInit_AnyExpression(Generator.Context ctx)
    {
        initValue.Generate(ctx);

        result = ctx.gen.Allocate(initValue.result.type, variable.name);
        ctx.gen.SetValue(result, initValue.result);
    }
    private void Generate_WithInit_New(Generator.Context ctx, Node_New tokenNew)
    {
        tokenNew.Generate(ctx);
        
        result = ctx.gen.Allocate(tokenNew.result.type, variable.name);
        ctx.gen.SetValue(result, tokenNew.result);
    }
}