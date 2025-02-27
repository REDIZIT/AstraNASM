﻿namespace Astra.Compilation;

public class PtrShift_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable pointerVariable, Variable shiftVariable)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"Shift pointer {pointerVariable.name} by {shiftVariable.name}");

        if (shiftVariable.type != PrimitiveTypes.INT)
        {
            throw new Exception($"Pointer.shift method received only int variables, but got {shiftVariable.type.name}");
        }

        ctx.gen.PtrShift(pointerVariable, shiftVariable);

        ctx.gen.Space();

        return null;
    }
}