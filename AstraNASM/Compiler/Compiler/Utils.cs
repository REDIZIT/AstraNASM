public static class Utils
{
    public static string SureNotPointer(string varName, Generator.Context ctx)
    {
        throw new Exception("Deprecated");

        if (ctx.IsPointer(varName) == false) return varName;

        TypeInfo type = ctx.GetPointedType(varName);
        string tempName = ctx.NextTempVariableName(type);
        ctx.b.Line($"{tempName} = load {type}, ptr {varName}");
        return tempName;
    }
}
