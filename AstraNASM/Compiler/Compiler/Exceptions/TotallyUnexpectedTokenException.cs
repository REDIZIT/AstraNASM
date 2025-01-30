namespace Astra.Compilation;

public class TotallyUnexpectedTokenException : Exception
{
    public Token unexpectedToken;

    public override string Message => $"Totally unexpected token '{unexpectedToken.GetType()}'";

    public TotallyUnexpectedTokenException(Token unexpectedToken)
    {
        this.unexpectedToken = unexpectedToken;
    }
}