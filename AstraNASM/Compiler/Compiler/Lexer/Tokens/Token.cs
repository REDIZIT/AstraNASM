﻿namespace Astra.Compilation;

using System.Text.RegularExpressions;

public abstract class Token
{
}
public class Token_Constant : Token
{
    public string value;

    public Token_Constant(string value)
    {
        this.value = value;
    }

    public override string ToString()
    {
        return base.ToString() + ": " + value;
    }
}
public class Token_BracketOpen : Token
{
}
public class Token_BracketClose : Token
{
}
public class Token_Identifier : Token
{
    public string name;

    public static bool IsMatch(string word)
    {
        return Regex.IsMatch(word, "[a-zA-Z0-9_]");
    }
}
public class Token_Assign : Token
{
    public static bool TryMatch(string word, out Token_Assign token)
    {
        if (word == "=")
        {
            token = new Token_Assign();
            return true;
        }
        token = null;
        return false;
    }
}
public class Token_BlockOpen : Token
{
}
public class Token_BlockClose : Token
{
}
public class Token_If : Token
{
}
public class Token_Else : Token
{
}
public class Token_While : Token
{
}
public class Token_For : Token
{
}
public class Token_Comma : Token
{
}
public class Token_Fn : Token
{
}
public class Token_Return : Token
{
}
public class Token_Terminator : Token
{
}
public class Token_Colon : Token
{
}
public class Token_Visibility : Token
{
    public bool isPublic;

    public static bool TryMatch(string word, out Token_Visibility token)
    {
        if (IsMatch(word))
        {
            token = new Token_Visibility()
            {
                isPublic = word == "public"
            };
            return true;
        }
        token = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "public" || word == "private";
    }
}
public class Token_Class : Token
{
}
public class Token_New : Token
{
}
public class Token_Dot : Token
{
}
public class Token_SquareBracketOpen : Token
{
}
public class Token_SquareBracketClose : Token
{
}
public class Token_Comment : Token
{
}
public class Token_EOF : Token
{
}
public class Token_Bad : Token
{
}
public class Token_Space : Token
{
}
public class Token_TypeName : Token
{
}

public class Token_Char(string value) : Token_Constant(value);
public class Token_String(string value) : Token_Constant(value);