using AVM;

namespace Astra.Compilation;

public class Command_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, VMCommand_Cmd cmd, List<Variable> variables)
    {
        Variable ret = null;
        
        if (cmd == VMCommand_Cmd.CreateWindow)
        {
            ret = ctx.gen.Allocate(PrimitiveTypes.PTR);
            variables.Insert(0, ret);
        }
        
        ctx.gen.VMCmd(cmd, variables);
        return ret;
    }
}