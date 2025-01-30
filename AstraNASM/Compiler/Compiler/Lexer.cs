using System.Reflection.Metadata;

namespace Astra.Compilation;

public class Lexer
{
    public List<char> chars = new();

    public int currentPos;
    public int startRead;
    public int endRead;
    public int markedPos;
    public int lexicalState;

    private static Dictionary<string, Type> tokenTypeBySingleWord = new Dictionary<string, Type>()
    {
        { "if", typeof(Token_If) },
        { "else", typeof(Token_Else) },
        { "while", typeof(Token_While) },
        { "for", typeof(Token_For) },
        { "fn", typeof(Token_Fn) },
        { "return", typeof(Token_Return) },
        { "class", typeof(Token_Class) },
        { "new", typeof(Token_New) },
    };
    private static Dictionary<string, Type> tokenTypeBySingleChar = new Dictionary<string, Type>()
    {
        { "(", typeof(Token_BracketOpen) },
        { ")", typeof(Token_BracketClose) },
        { "{", typeof(Token_BlockOpen) },
        { "}", typeof(Token_BlockClose) },
        { ";", typeof(Token_Terminator) },
        { ":", typeof(Token_Colon) },
        { ",", typeof(Token_Comma) },
        { ".", typeof(Token_Dot) },
        { "[", typeof(Token_SquareBracketOpen) },
        { "]", typeof(Token_SquareBracketClose) },
    };

    public List<Token> Tokenize(string astraCode, bool includeSpacesAndEOF)
    {
        Reset(astraCode.ToList(), 0, astraCode.Length, 0);

        List<Token> tokens = new();

        while (true)
        {
            Token token = Advance();

            if (includeSpacesAndEOF == false && (token is Token_Space || token is Token_EOF))
            {
                // Don't add to tokens
            }
            else
            {
                tokens.Add(token);
            }

            if (token is Token_EOF) break;
        }

        return tokens;
    }

    public void Reset(List<char> chars, int start, int end, int initialState)
    {
        this.chars = chars;
        currentPos = markedPos = startRead = start;
        endRead = end;

        lexicalState = initialState;

        Console.WriteLine($"Lexer got {chars.Count} chars, {start}:{end} with state {initialState}");
    }

    public Token Advance()
    {
        if (currentPos >= endRead)
        {
            return new Token_EOF();
        }

        // Save word start pos
        startRead = currentPos;


        Token token = AdvanceInternal();
        markedPos = currentPos; // Save word end pos

        if (token == null)
        {
            return new Token_Bad();
        }
        else
        {
            return token;
        }
    }

    private Token AdvanceInternal()
    {
        char startChar = chars[currentPos];
        currentPos++;

        if (startChar == ' ' || startChar == '\t')
        {
            return new Token_Space();
        }


        if (startChar == '\r' || startChar == '\n' || startChar == ';')
        {
            while (currentPos < endRead && (chars[currentPos] == '\r' || chars[currentPos] == '\n' || chars[currentPos] == ';'))
            {
                currentPos++;
            }
            return new Token_Terminator();
        }


        if (tokenTypeBySingleChar.TryGetValue(startChar.ToString(), out Type singleCharTokenType))
        {
            return (Token)Activator.CreateInstance(singleCharTokenType);
        }


        if (char.IsDigit(startChar))
        {
            //
            // Iterate digits for numbers
            //
            while (currentPos < endRead)
            {
                if (char.IsDigit(chars[currentPos]) == false)
                {
                    string word = string.Concat(chars[startRead..currentPos]);

                    if (int.TryParse(word, out int value))
                    {
                        return new Token_Constant()
                        {
                            value = word
                        };
                    }

                    break;
                }

                currentPos++;
            }
        }
        else
        {
            //
            // Iterate chars for tokens
            //
            string word = "";

            while (currentPos < endRead)
            {
                word = string.Concat(chars[startRead..currentPos]);



                if (startChar == '=' || startChar == '!' || startChar == '>' || startChar == '<')
                {
                    char opChar = chars[currentPos];

                    while (currentPos < endRead && (opChar == '=' || opChar == '!' || opChar == '>' || opChar == '<'))
                    {
                        opChar = chars[opChar];
                        currentPos++;
                    }

                    string operatorWord = string.Concat(chars[startRead..currentPos]);

                    if (Token_Equality.TryMatch(operatorWord, out var eq)) return eq;
                    if (Token_Comprassion.TryMatch(operatorWord, out var cmp)) return cmp;
                    if (Token_Assign.TryMatch(operatorWord, out var ass)) return ass;
                }


                if (tokenTypeBySingleWord.TryGetValue(word, out Type tokenType))
                {
                    return (Token)Activator.CreateInstance(tokenType);
                }


                if (Token_AddSub.TryMatch(word, out var term)) return term;
                if (Token_Factor.TryMatch(word, out var fact)) return fact;
                if (Token_Unary.TryMatch(word, out var un)) return un;


                if (currentPos < endRead && (char.IsLetterOrDigit(chars[currentPos]) == false && chars[currentPos] != '_'))
                {
                    break;
                }

                currentPos++;
            }

            if (Token_Visibility.TryMatch(word, out Token_Visibility tokenVisibility))
            {
                return tokenVisibility;
            }
            return new Token_Identifier()
            {
                name = word
            };
        }

        return null;
    }
}
