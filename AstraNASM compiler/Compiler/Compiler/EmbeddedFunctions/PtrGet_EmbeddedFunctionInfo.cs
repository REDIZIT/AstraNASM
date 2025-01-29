namespace Astra.Compilation
{
    public class PtrGet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
    {
        public Variable Generate(Generator.Context ctx, string pointerVariableName)
        {
            ctx.b.Space();
            ctx.b.CommentLine($"Get value from {pointerVariableName}");


            var pointerVar = ctx.GetVariable(pointerVariableName);

            TypeInfo pointedType = PrimitiveTypeInfo.LONG; // TODO

            var result = ctx.AllocateStackVariable(pointedType);

            ctx.b.Line($"mov rax, {pointerVar.GetRBP()}");
            ctx.b.Line($"mov {result.GetRBP()}, [rax]");

            ctx.b.Space();

            return result;
        }
    }
}
