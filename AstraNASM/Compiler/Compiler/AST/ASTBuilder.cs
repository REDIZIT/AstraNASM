namespace Astra.Compilation;

public class ASTBuilder
{
    protected List<Token> tokens;
    protected int current;
    protected List<Node> statements;

    private Stack<List<Token>> frames = new();

    protected bool Match<T>() where T : Token
    {
        ConsumeSpace(false);

        if (Check<T>())
        {
            Advance();
            return true;
        }
        return false;
    }
    protected bool Match<T>(out T token) where T : Token
    {
        ConsumeSpace(false);

        if (Check(typeof(T)))
        {
            token = (T)Advance();
            return true;
        }
        token = null;
        return false;
    }
    protected bool Match(Type tokenType)
    {
        ConsumeSpace(false);

        if (Check(tokenType))
        {
            Advance();
            return true;
        }
        return false;
    }

    protected bool Check<T>() where T : Token
    {
        ConsumeSpace(false);

        return Check(typeof(T));
    }
    protected bool Check(Type tokenType)
    {
        ConsumeSpace(false);

        if (IsAtEnd()) return false;

        return Peek().GetType() == tokenType;
    }
    protected Token Advance()
    {
        ConsumeSpace(false);

        if (IsAtEnd() == false)
        {
            frames.Peek().Add(tokens[current]);
            current++;
        }
        return Previous();
    }
    protected bool IsAtEnd()
    {
        return current >= tokens.Count;
    }
    protected Token Peek()
    {
        return tokens[current];
    }
    protected T Previous<T>() where T : Token
    {
        return (T)Previous();
    }
    protected Token Previous()
    {
        return tokens[current - 1];
    }
    protected Token Next()
    {
        return tokens[current + 1];
    }
    protected T Consume<T>(string errorMessage) where T : Token
    {
        return (T)Consume(typeof(T), errorMessage);
    }
    protected Token Consume(Type awaitingTokenType, string errorMessage)
    {
        ConsumeSpace(false);

        if (Check(awaitingTokenType)) return Advance();

        Token gotToken = Peek();
        throw new UnexpectedTokenException(awaitingTokenType, gotToken, errorMessage);
    }

    protected void ConsumeSpace(bool consumeNewLines)
    {
        if (IsAtEnd()) return;

        while (IsAtEnd() == false)
        {
            Token token = Peek();

            if (token is Token_Space)
            {
                current++;
            }
            else if (consumeNewLines && token is Token_Terminator)
            {
                current++;
            }
            else
            {
                return;
            }
        }
    }

    protected void StartNewFrame()
    {
        frames.Push(new List<Token>());
    }

    protected List<Token> PopFrame()
    {
        return frames.Pop();
    }
}