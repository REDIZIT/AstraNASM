
namespace Astra.Compilation;

public class Node_Function : Node
{
    public string name;
    public Node body;
    public List<VariableRawData> parameters = new();
    public List<VariableRawData> returnValues = new();

    public FunctionInfo functionInfo;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (returnValues.Count > 1)
        {
            throw new NotImplementedException("Function has 1+ return values. Generation for this is not supported yet");
        }

        //List<string> paramsDeclars = new();
        //foreach (VariableRawData param in parameters)
        //{
        //    string generatedName = ctx.NextPointerVariableName(param.type, param.name);
        //    string generatedType = (param.type is PrimitiveTypeInfo) ? param.type.ToString() : "ptr";

        //    paramsDeclars.Add($"{generatedType} {generatedName}");
        //}
        //string paramsStr = string.Join(", ", paramsDeclars);


        //ctx.b.Space(3);

        //if (returnValues.Count == 0)
        //{
        //    ctx.b.Line($"define void @{name}({paramsStr})");
        //}
        //else
        //{
        //    ctx.b.Line($"define {returnValues[0].type} @{name}({paramsStr})");
        //}
        
        //ctx.b.Line("{");

       

        ctx = ctx.CreateSubContext();

        ctx.gen.Space(3);

        

        // ctx.b.Line($"{name}:");
        // ctx.b.Line("push rbp");
        // ctx.b.Line("mov rbp, rsp");
        
        ctx.gen.Label(name);
        ctx.gen.Prologue();
        
        ctx.gen.Space();

        int index = 0;

        for (int i = functionInfo.arguments.Count - 1; i >= 0; i--)
        {
            FieldInfo argInfo = functionInfo.arguments[i];
            ctx.gen.Register_FunctionArgumentVariable(argInfo, index);
            index++;
        }

        
        if (functionInfo.owner != null)
        {
            // ctx.gen.RegisterSelf(functionInfo);
            ctx.gen.Register_FunctionArgumentVariable(new FieldInfo()
            {
                name = "self",
                type = functionInfo.owner
            }, index);
            index++;
        }

        body.Generate(ctx);

        // ctx.b.Space(1);
        ctx.gen.Space();
    }
}