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
                token = tokens[current],
                message = messageText
            };
        }
        else
        {
            int tokenIndex = tokens.IndexOf(errorToken);

            entry = new LogEntry()
            {
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
        if (Check<Token_Class>()) return ClassDeclaration();

        return FunctionsAndFieldsDeclaration();
    }
    public Node FunctionsAndFieldsDeclaration()
    {
        if (Check<Token_Identifier>()) return Variable();
        if (Check<Token_Visibility>()) return FunctionDeclaration();

        return Statement();
    }
    private Node Statement()
    {
        if (Match(typeof(Token_BlockOpen))) return Block();
        if (Check<Token_If>()) return If();
        if (Check<Token_While>()) return While();
        if (Check<Token_For>()) return For();
        if (Check<Token_Return>()) return Return();

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
        if (Check<Token_Assign>())
        {
            // Value
            StartNewFrame();

            Consume<Token_Assign>("Expected '=' for variable assignment");
            
            Node right = Assignment();

            if (left is Node_VariableUse || left is Node_FieldAccess)
            {
                return new Node_VariableAssign()
                {
                    target = left,
                    value = right,
                    consumedTokens = PopFrame()
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

        while (Check<Token_Equality>())
        {
            StartNewFrame();
            Token_Equality operatorToken = Consume<Token_Equality>("Expected '=' for equality");
            
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = Comprassion(),
                consumedTokens = PopFrame()
            };
        }

        return left;
    }
    private Node Comprassion()
    {
        Node left = AddSub();

        while (Check<Token_Comprassion>())
        {
            StartNewFrame();
            Token_Comprassion operatorToken = Consume<Token_Comprassion>("Expected token comprassion for comprassion");
            
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = AddSub(),
                consumedTokens = PopFrame()
            };
        }

        return left;
    }
    private Node AddSub()
    {
        Node left = MulDiv();

        while (Check<Token_AddSub>())
        {
            StartNewFrame();
            Token_AddSub operatorToken = Consume<Token_AddSub>("Expected token add or sub for add sub");
            
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = MulDiv(),
                consumedTokens = PopFrame()
            };
        }

        return left;
    }
    private Node MulDiv()
    {
        Node left = BitShift();

        while (Check<Token_Factor>())
        {
            StartNewFrame();
            Token_Factor operatorToken = Consume<Token_Factor>("Expected token factor for factor");

            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = BitShift(),
                consumedTokens = PopFrame()
            };
        }

        return left;
    }
    private Node BitShift()
    {
        Node left = NotNeg();

        while (Check<Token_BitOperator>())
        {
            StartNewFrame();
            Token_BitOperator operatorToken = Consume<Token_BitOperator>("Expected token bit operator for bit operation");
            
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = NotNeg()
            };
        }

        return left;
    }
    private Node NotNeg()
    {
        if (Check<Token_Unary>())
        {
            StartNewFrame();
            Token_Unary operatorToken = Consume<Token_Unary>("Expected token unary for unary operation");
            
            return new Node_Unary()
            {
                @operator = operatorToken,
                right = NotNeg(),
                consumedTokens = PopFrame()
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
        if (Check<Token_New>()) return New();

        Node expr = Primary();

        while (true)
        {
            Token_Identifier token = Previous() as Token_Identifier;
            if (Check<Token_BracketOpen>())
            {
                expr = FinishCall(expr, token);
            }
            else if (Check<Token_Dot>())
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
        StartNewFrame();

        Consume<Token_Class>("Expected class declaration");
        
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
            body = body,
            consumedTokens = PopFrame()
        };
    }
    private Node FunctionDeclaration()
    {
        //Consume(typeof(Token_Fn), "Expected 'fn' before function declaration.");
        return Function();
    }
    private Node Function()
    {
        StartNewFrame();

        Consume<Token_Visibility>("Expected visibility");
        
        bool isStatic = Match<Token_Static>();
        
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
            isStatic = isStatic,
            consumedTokens = PopFrame()
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
        StartNewFrame();
        
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
            initValue = initValue,
            consumedTokens = PopFrame()
        };
    }

    private Node New()
    {
        StartNewFrame();

        Consume<Token_New>("Expected 'new' keyword");
        
        Token_Identifier ident = Consume<Token_Identifier>("Expected ref type name");

        Consume<Token_BracketOpen>("Expected '(' after type name");
        Consume<Token_BracketClose>("Expected ')' after type name");

        return new Node_New()
        {
            className = ident.name,
            consumedTokens = PopFrame()
        };
    }
    private Node Return()
    {
        StartNewFrame();

        Consume<Token_Return>("Expected 'return'");
        
        if (Match(typeof(Token_Terminator)))
        {
            return new Node_Return()
            {
                consumedTokens = PopFrame()
            };
        }
        else
        {
            Node expr = Expression();
            return new Node_Return()
            {
                expr = expr,
                consumedTokens = PopFrame()
            };
        }
    }
    private Node For()
    {
        StartNewFrame();

        Consume<Token_For>("Expected 'for' keyword");
        
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
        
        return new Node_For()
        {
            declaration = declaration,
            condition = condition,
            advance = action,
            body = body,
            consumedTokens = PopFrame()
        };
    }
    private Node While()
    {
        StartNewFrame();
        
        Consume<Token_While>("Expected 'while' keyword");
        
        Consume(typeof(Token_BracketOpen), "Expected '(' before condition.");
        Node condition = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after condition.");

        ConsumeSpace(true);
        Consume<Token_BlockOpen>("Expected '{' after while declaration");
        Node body = Block();

        return new Node_While()
        {
            condition = condition,
            body = body,
            consumedTokens = PopFrame()
        };
    }
    private Node If()
    {
        StartNewFrame();

        Consume<Token_If>("Expected 'if' keyword");
        
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
            elseBranch = elseBranch,
            consumedTokens = PopFrame()
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

                StartNewFrame();
                Consume<Token_BlockClose>("Expected '}' after block");
                return new Node_Block()
                {
                    children = nodes,
                    consumedTokens = PopFrame()
                };
            }
        }

        throw new Exception("Block is not closed by '}'");
    }


    private Node Property(Node target)
    {
        StartNewFrame();

        Consume<Token_Dot>("Expected '.' for property access");
        
        Token_Identifier ident = Consume<Token_Identifier>("Expected field name");

        return new Node_FieldAccess()
        {
            target = target,
            targetFieldName = ident.name,
            consumedTokens = PopFrame()
        };
    }
    private Node FinishCall(Node caller, Token_Identifier ident)
    {
        StartNewFrame();

        Consume<Token_BracketOpen>("Expected '(' for function call");
        
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
            consumedTokens = PopFrame()
        };
    }
    private Node Primary()
    {
        StartNewFrame();
        
        if (Check<Token_Constant>() || Check<Token_Char>() || Check<Token_String>())
        {
            if (Check<Token_Char>())
            {
                return new Node_Literal()
                {
                    constant = new("'" + Consume<Token_Char>("Expected char").value + "'"),
                    consumedTokens = PopFrame()
                };
            }
            else if (Check<Token_String>())
            {
                return new Node_Literal()
                {
                    constant = Consume<Token_String>("Expected string"),
                    consumedTokens = PopFrame()
                };
            }
            else
            {
                return new Node_Literal()
                {
                    constant = Consume<Token_Constant>("Expected constant"),
                    consumedTokens = PopFrame()
                };
            }
        }

        if (Match(typeof(Token_Identifier)))
        {
            return new Node_VariableUse()
            {
                variableName = ((Token_Identifier)Previous()).name,
                consumedTokens = PopFrame()
            };
        }


        if (Match(typeof(Token_BracketOpen)))
        {
            Node expr = Expression();
            Consume(typeof(Token_BracketClose), "Expect ')' after expression.");
            return new Node_Grouping()
            {
                expression = expr,
                consumedTokens = PopFrame()
            };
        }


        if (Check(typeof(Token_Terminator)))
        {
            return null;
        }

        throw new TotallyUnexpectedTokenException(Peek());
    }
}