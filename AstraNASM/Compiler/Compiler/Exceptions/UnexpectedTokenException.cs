namespace Astra.Compilation;

public class UnexpectedTokenException : Exception
{
    public Type expectedToken;
    public Token gotToken;
    public string message;

    public override string Message => $"Expected '{expectedToken}', but got '{gotToken}'";

    public UnexpectedTokenException(Type expectedToken, Token gotToken, string message)
    {
        this.expectedToken = expectedToken;
        this.gotToken = gotToken;
        this.message = message;
    }
}