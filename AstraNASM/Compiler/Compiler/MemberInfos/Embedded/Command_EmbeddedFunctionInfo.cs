using AVM;

namespace Astra.Compilation;

public class Command_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, VMCommand_Cmd cmd, Variable[] variables)
    {
        ctx.gen.VMCmd(cmd, variables);
        return null;
    }
}