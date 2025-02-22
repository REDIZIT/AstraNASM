namespace Astra.Compilation;

public abstract class Token_Operator : Token
{
    public virtual string ResultType => "int";

    public string asmOperatorName;

    public override string ToString()
    {
        return base.ToString() + ": " + asmOperatorName;
    }
}
public class Token_Equality : Token_Operator
{
    public override string ResultType => "bool";
}
public class Token_Comprassion : Token_Operator
{
    public override string ResultType => "bool";
}
public class Token_AddSub : Token_Operator
{
}
public class Token_IncDec : Token_Unary
{
}
public class Token_Factor : Token_Operator
{
}
public class Token_BitOperator : Token_Operator
{
}
public class Token_Unary : Token_Operator
{
}