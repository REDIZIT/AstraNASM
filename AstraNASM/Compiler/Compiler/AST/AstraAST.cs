namespace Astra.Compilation;

using System.Collections.Generic;

public class AstraAST : ASTBuilder
{
    private ErrorLogger logger;
    private bool isBlockFailed;

    public List<Node> Parse(List<Token> tokens, ErrorLogger logger = null)
    {
        this.tokens = tokens.Where(t => t is Token_EOF == false && t is Token_Comment == false).ToList();
        this.logger = logger;
        this.current = 0;

        statements = new();

        while (IsAtEnd() == false)
        {
            try
            {
                ConsumeSpace(true);
                statements.Add(Declaration());
            }
            catch (Exception err)
            {
                LogAndSync(err);
            }
        }


        return statements;
    }

    private void LogAndSync(Exception err)
    {
        Log(err);
        Sync();
    }

    private void Log(Exception err, Token errorToken = null)
    {
        string messageText = err.Message;

        if (err is UnexpectedTokenException tokenErr)
        {
            messageText = tokenErr.message + "\n" + messageText;
        }

        LogEntry entry;

        if (errorToken == null)
        {
            entry = new LogEntry()
            {
                //tokenBeginIndex = current,
                //tokenEndIndex = current,
                token = tokens[current],
                message = messageText
            };
        }
        else
        {
            int tokenIndex = tokens.IndexOf(errorToken);

            entry = new LogEntry()
            {
                //tokenBeginIndex = tokenIndex,
                //tokenEndIndex = tokenIndex,
                token = tokens[tokenIndex],
                message = messageText
            };
        }

        logger.Error(entry);
    }

    private void Log(string message)
    {
        //Console.WriteLine(message);
    }

    private void Sync()
    {
        Token failedToken = Previous();
        Log("Syncing. Failed: " + failedToken);

        if (IsAtEnd())
        {
            return;
        }

        if (failedToken is Token_Terminator)
        {
            Advance();
            Log("Skip failed (previous token) terminator");
        }

        while (IsAtEnd() == false && Peek() is Token_Terminator == false)
        {
            Log("Skipping " + Peek());
            Advance();
        }

        if (IsAtEnd())
        {
            Log("Synced at end");
        }
        else
        {
            Log("Synced at " + Peek());
        }
    }


    #region Layers

    private Node Declaration()
    {
        if (Match(typeof(Token_Class))) return ClassDeclaration();

        return FunctionsAndFieldsDeclaration();
    }
    public Node FunctionsAndFieldsDeclaration()
    {
        if (Check<Token_Identifier>()) return Variable();
        if (Match(typeof(Token_Visibility))) return FunctionDeclaration();

        return Statement();
    }
    private Node Statement()
    {
        if (Match(typeof(Token_BlockOpen))) return Block();
        if (Match(typeof(Token_If))) return If();
        if (Match(typeof(Token_While))) return While();
        if (Match(typeof(Token_For))) return For();
        if (Match(typeof(Token_Return))) return Return();

        return Expression();
    }
    private Node Expression()
    {
        return Assignment();
    }
    private Node Assignment()
    {
        // Target
        Node left = Equality();

        // '='
        if (Match(typeof(Token_Assign)))
        {
            // Value
            Node right = Assignment();

            if (left is Node_VariableUse || left is Node_FieldAccess)
            {
                return new Node_VariableAssign()
                {
                    target = left,
                    value = right
                };
            }
            else
            {
                throw new Exception("Expected variable name or field access to assign, but no such token found after '='");
            }
        }

        return left;
    }
    private Node Equality()
    {
        Node left = Comprassion();

        while (Match(out Token_Equality operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = Comprassion()
            };
        }

        return left;
    }
    private Node Comprassion()
    {
        Node left = AddSub();

        while (Match(out Token_Comprassion operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = AddSub()
            };
        }

        return left;
    }
    private Node AddSub()
    {
        Node left = MulDiv();

        while (Match(out Token_AddSub operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = MulDiv()
            };
        }

        return left;
    }
    private Node MulDiv()
    {
        Node left = BitShift();

        while (Match(out Token_Factor operatorToken))
        {
            Node right = NotNeg();
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = right
            };
        }

        return left;
    }
    private Node BitShift()
    {
        Node left = NotNeg();

        while (Match(out Token_BitShift operatorToken))
        {
            Node right = NotNeg();
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = right
            };
        }

        return left;
    }
    private Node NotNeg()
    {
        if (Match(out Token_Unary operatorToken))
        {
            return new Node_Unary()
            {
                @operator = operatorToken,
                right = NotNeg()
            };
        }
        else if (Peek() is Token_AddSub tokenAddSub && tokenAddSub.asmOperatorName == "sub")
        {
            // Handle unary '-' token
            // @code_example - 3_ret_-1.ac
            try
            {
                return Call();
            }
            catch (TotallyUnexpectedTokenException err)
            {
                if (err.unexpectedToken == tokenAddSub)
                {
                    Consume<Token_AddSub>("Expected '+' or '-'");

                    return new Node_Unary()
                    {
                        @operator = tokenAddSub,
                        right = NotNeg()
                    };
                }
                else
                {
                    throw;
                }
            }
        }

        return Call();
    }
    private Node Call()
    {
        if (Match(typeof(Token_New))) return New();

        Node expr = Primary();

        while (true)
        {
            Token_Identifier token = Previous() as Token_Identifier;
            if (Match(typeof(Token_BracketOpen)))
            {
                expr = FinishCall(expr, token);
            }
            else if (Match(typeof(Token_Dot)))
            {
                expr = Property(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    #endregion


    private Node ClassDeclaration()
    {
        Token_Identifier ident = Consume<Token_Identifier>("Expected class name");



        ConsumeSpace(true);
        try
        {
            Consume<Token_BlockOpen>("Expected '{' after class declaration");
        }
        catch (Exception err)
        {
            isBlockFailed = true;
            Log(err, ident);
        }

        var body = (Node_Block)Block();

        return new Node_Class()
        {
            name = ident.name,
            body = body
        };
    }
    private Node FunctionDeclaration()
    {
        //Consume(typeof(Token_Fn), "Expected 'fn' before function declaration.");
        return Function();
    }
    private Node Function()
    {
        Token_Identifier functionName = Consume<Token_Identifier>("Expected function name");
        Consume<Token_BracketOpen>("Expected '(' after function name");



        List<VariableRawData> parameters = new();
        if (Check(typeof(Token_BracketClose)) == false)
        {
            do
            {
                var paramType = Consume<Token_Identifier>("Expected parameter type");
                var paramName = Consume<Token_Identifier>("Expected parameter name");
                parameters.Add(new VariableRawData()
                {
                    name = paramName.name,
                    rawType = paramType.name,
                });

            } while (Match(typeof(Token_Comma)));
        }

        Consume<Token_BracketClose>("Expected ')' after parameters");



        // Has return type
        List<VariableRawData> returnValues = new();
        if (Match(typeof(Token_Colon)))
        {
            do
            {
                VariableRawData variable = ReturnValueDeclaration();
                returnValues.Add(variable);
            } while (Match(typeof(Token_Comma)));
        }

        ConsumeSpace(true);

        try
        {
            Consume<Token_BlockOpen>("Expected '{' before function body");
        }
        catch (Exception err)
        {
            isBlockFailed = true;
            Log(err, functionName);
        }

        Node body = Block();
        return new Node_Function()
        {
            name = functionName.name,
            body = body,
            parameters = parameters,
            returnValues = returnValues,
        };

    }
    private VariableRawData ReturnValueDeclaration()
    {
        if (Match(typeof(Token_Identifier)))
        {
            var type = Previous<Token_Identifier>();

            VariableRawData data = new()
            {
                rawType = type.name
            };

            if (Check(typeof(Token_Identifier)))
            {
                Token_Identifier name = Consume<Token_Identifier>("Expected argument name");
                data.name = name.name;
            }

            return data;
        }
        else
        {
            throw new Exception("Expected type inside argument declaration");
        }
    }
    private Node Variable()
    {
        //var firstName = Consume<Token_Identifier>();

        // SomeType myVar ...
        //if (Check<Token_Identifier>() && ) return VariableDeclaration(firstName);
        if (Next() is Token_Identifier || Next() is Token_SquareBracketOpen) return VariableDeclaration();

        // myVar ...
        return Expression();
    }
    private Node VariableDeclaration()
    {
        var type = Consume<Token_Identifier>("Expected variable type");

        bool isArray = false;
        if (Check(typeof(Token_SquareBracketOpen)))
        {
            isArray = true;
            Consume<Token_SquareBracketOpen>("Expected '[' for array declaration");
            Consume<Token_SquareBracketClose>("Expected ']' for array declaration");
        }

        var varNameToken = Consume<Token_Identifier>("Expect variable name");

        Node initValue = null;
        if (Match(typeof(Token_Assign)))
        {
            initValue = Expression();
        }

        return new Node_VariableDeclaration()
        {
            variable = new VariableRawData()
            {
                rawType = isArray ? "array" : type.name,
                name = varNameToken.name
            },
            initValue = initValue
        };
    }

    private Node New()
    {
        Token_Identifier ident = Consume<Token_Identifier>("Expected ref type name");

        Consume<Token_BracketOpen>("Expected '(' after type name");
        Consume<Token_BracketClose>("Expected ')' after type name");

        return new Node_New()
        {
            className = ident.name,
        };
    }
    private Node Return()
    {
        if (Match(typeof(Token_Terminator)))
        {
            return new Node_Return();
        }
        else
        {
            Node expr = Expression();
            //Consume<Token_Terminator>("Expected terminator after return", skipTerminators: false);
            return new Node_Return()
            {
                expr = expr
            };
        }
    }
    private Node For()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' after 'for'");
        Node declaration = Declaration();
        Consume(typeof(Token_Terminator), "Expected ';' after declaration");
        Node condition = Expression();
        Consume(typeof(Token_Terminator), "Expected ';' after condition");
        Node action = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after action");

        ConsumeSpace(true);
        Consume<Token_BlockOpen>("Expected '{' after for declaration");
        Node body = Block();

        //return new Node_Block()
        //{
        //    children = new List<Node>()
        //    {
        //        declaration,
        //        new Node_While()
        //        {
        //            condition = condition,
        //            body = new Node_Block()
        //            {
        //                children = new List<Node>()
        //                {
        //                    body,
        //                    action
        //                }
        //            }
        //        }
        //    }
        //};
        return new Node_For()
        {
            declaration = declaration,
            condition = condition,
            advance = action,
            body = body
        };
    }
    private Node While()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' before condition.");
        Node condition = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after condition.");

        ConsumeSpace(true);
        Consume<Token_BlockOpen>("Expected '{' after while declaration");
        Node body = Block();

        return new Node_While()
        {
            condition = condition,
            body = body
        };
    }
    private Node If()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' before condition.");
        Node condition = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after condition.");

        ConsumeSpace(true);

        Node thenBranch = Statement();
        Node elseBranch = null;

        ConsumeSpace(true);

        if (Match(typeof(Token_Else)))
        {
            elseBranch = Statement();
        }

        return new Node_If()
        {
            condition = condition,
            thenBranch = thenBranch,
            elseBranch = elseBranch
        };
    }

    /// <summary>
    /// Before entering Block method you make sure that <see cref="Token_BlockOpen"></see> is already consumed.<br/>
    /// After exiting Block method will be guaranteed that <see cref="Token_BlockClose"></see> is already consumed. (You should not consume it manually)
    /// </summary>
    private Node Block()
    {
        List<Node> nodes = new();

        while (IsAtEnd() == false)
        {
            ConsumeSpace(true);

            if (Check<Token_BlockClose>() == false)
            {
                try
                {
                    nodes.Add(Declaration());
                }
                catch (Exception e)
                {
                    LogAndSync(e);
                }
            }
            else
            {
                if (isBlockFailed)
                {
                    isBlockFailed = false;
                    throw new Exception("This block has '}' but not '{'");
                }

                Consume<Token_BlockClose>("Expected '}' after block");
                return new Node_Block()
                {
                    children = nodes
                };
            }
        }

        throw new Exception("Block is not closed by '}'");
    }


    private Node Property(Node target)
    {
        Token_Identifier ident = Consume<Token_Identifier>("Expected field name");

        return new Node_FieldAccess()
        {
            target = target,
            targetFieldName = ident.name,
        };
    }
    private Node FinishCall(Node caller, Token_Identifier ident)
    {
        List<Node> arguments = new();

        if (Check(typeof(Token_BracketClose)) == false)
        {
            do
            {
                arguments.Add(Expression());
            }
            while (Match(typeof(Token_Comma)));
        }

        Consume(typeof(Token_BracketClose), "Expected ')' after arguments for function call");
        return new Node_FunctionCall()
        {
            functionName = ident == null ? "<anon>" : ident.name,
            caller = caller,
            arguments = arguments,
        };
    }
    private Node Primary()
    {
        if (Check<Token_Constant>() || Check<Token_Char>() || Check<Token_String>())
        {
            if (Check<Token_Char>())
            {
                return new Node_Literal()
                {
                    constant = new("'" + Consume<Token_Char>("Expected char").value + "'")
                };
            }
            else if (Check<Token_String>())
            {
                return new Node_Literal()
                {
                    constant = Consume<Token_String>("Expected string")
                };
            }
            else
            {
                return new Node_Literal()
                {
                    constant = Consume<Token_Constant>("Expected constant")
                };
            }
        }

        if (Match(typeof(Token_Identifier)))
        {
            return new Node_VariableUse()
            {
                variableName = ((Token_Identifier)Previous()).name
            };
        }


        if (Match(typeof(Token_BracketOpen)))
        {
            Node expr = Expression();
            Consume(typeof(Token_BracketClose), "Expect ')' after expression.");
            return new Node_Grouping()
            {
                expression = expr
            };
        }


        if (Check(typeof(Token_Terminator)))
        {
            //bool anyTerminatorSkipped = SkipTerminators();

            //if (IsAtEnd()) return null;
            //else if (anyTerminatorSkipped) return Declaration();

            return null;
        }

        //throw new Exception($"Totally unexpected token '{Peek()}'");
        throw new TotallyUnexpectedTokenException(Peek());
    }


    
}