class Parser
{
    List<Token> tokens;
    int i;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Node> Parse()
    {
        List<Node> nodes = new List<Node>();

        while (!AtEnd())
        {
            if (Check(TokenType.Let))
            {
                nodes.Add(ParseVariable(false));
                continue;
            }
            else if (Check(TokenType.Const))
            {
                nodes.Add(ParseVariable(true));
                continue;
            }
            else if (Check(TokenType.Print))
            {
                nodes.Add(ParsePrint());
                continue;
            }
            else if (Check(TokenType.If))
            {
                nodes.Add(ParseIf());
                continue;
            }
            else if (Check(TokenType.Set))
            {
                nodes.Add(ParseAssign());
                continue;
            }
            else if (Check(TokenType.While))
            {
                nodes.Add(ParseWhile());
                continue;
            }

            Next();
        }

        return nodes;
    }

    VariableNode ParseVariable(bool isConst)
    {
        Position position = tokens[i].Position;

        Consume(isConst ? TokenType.Const : TokenType.Let);

        string identifier = tokens[i].Value.StringValue;
        Consume(TokenType.Identifier, "Expected identifier");

        Consume(TokenType.Equal, "Expected '='");

        Expression expression = ParseExpression();

        Consume(TokenType.Semicolon, "Expected ';'");

        return new VariableNode(identifier, expression, isConst, position);
    }

    PrintNode ParsePrint()
    {
        Position position = tokens[i].Position;

        Consume(TokenType.Print);

        Consume(TokenType.LeftParen, "Expected '('");

        Expression expression = ParseExpression();

        Consume(TokenType.RightParen, "Expected ')'");

        Consume(TokenType.Semicolon, "Expected ';'");

        return new PrintNode(expression, position);
    }

    IfNode ParseIf()
    {
        Position position = tokens[i].Position;

        Consume(TokenType.If);

        Consume(TokenType.LeftParen, "Expected '('");

        Expression expression = ParseExpression();

        Consume(TokenType.RightParen, "Expected ')'");

        List<Node> block = ParseBlock();

        if (Match(TokenType.Else))
        {
            List<Node> elseBlock = ParseBlock();
            return new IfNode(expression, block, true, elseBlock, position);
        }

        return new IfNode(expression, block, false, new List<Node>(), position);
    }

    AssignNode ParseAssign()
    {
        Position position = tokens[i].Position;

        Consume(TokenType.Set);

        string identifier = tokens[i].Value.StringValue;
        Consume(TokenType.Identifier, "Expected identifier");

        if (Check(TokenType.PlusEqual) ||
        Check(TokenType.MinusEqual) ||
        Check(TokenType.TimesEqual) ||
        Check(TokenType.DivideEqual) ||
        Check(TokenType.Equal))
        {
            Token op = tokens[i];

            Next();

            Expression expression = ParseExpression();

            Consume(TokenType.Semicolon, "Expected ';'");

            return new AssignNode(identifier, expression, op.Type, true, op.Position, position);
        }

        if (Check(TokenType.Increment) ||
        Check(TokenType.Decrement))
        {
            Token op = tokens[i];

            Next();

            Consume(TokenType.Semicolon, "Expected ';'");

            return new AssignNode(identifier, Expression.Empty(position), op.Type, false, op.Position, position);
        }

        throw new Errno("Invalid assign operator", position, ErrorLocation.Parser);
    }

    WhileNode ParseWhile()
    {
        Position position = tokens[i].Position;

        Consume(TokenType.While);

        Consume(TokenType.LeftParen, "Expected '('");

        Expression expression = ParseExpression();

        Consume(TokenType.RightParen, "Expected ')'");

        List<Node> block = ParseBlock();

        return new WhileNode(expression, block, position);
    }

    List<Node> ParseBlock()
    {
        Consume(TokenType.LeftCurly, "Expected '{'");

        int start = i;
        int depth = 1;

        while (depth > 0)
        {
            if (Check(TokenType.EOF))
            {
                throw new Errno("Expected '}'", tokens[i].Position, ErrorLocation.Parser);
            }

            if (Check(TokenType.LeftCurly))
            {
                depth++;
            }

            if (Check(TokenType.RightCurly))
            {
                depth--;
                if (depth == 0) break;
            }

            Next();
        }

        List<Token> blockTokens = tokens.GetRange(start, i - start);
        blockTokens.Add(new Token(new Value(ValueKind.Void), TokenType.EOF, tokens[i].Position));

        Consume(TokenType.RightCurly);

        return new Parser(blockTokens).Parse();
    }

    Expression ParsePrimary()
    {
        Token token = tokens[i];

        if (Match(TokenType.Identifier))
        {
            return new NameExpression(token.Value.StringValue, token.Position);
        }
        else if (Match(TokenType.Int) ||
        Match(TokenType.Float) ||
        Match(TokenType.String) ||
        Match(TokenType.Bool))
        {
            return new LiteralExpression(token.Value, token.Position);
        }

        throw new Errno("Invalid expression", token.Position, ErrorLocation.Parser);
    }

    Expression ParseUnary()
    {
        if (Check(TokenType.Sub) || Check(TokenType.Exclamation))
        {
            Token op = tokens[i];

            Next();

            Expression right = ParseUnary();

            return new UnaryExpression(right, op.Type, op.Position);
        }

        return ParsePrimary();
    }

    Expression ParseFactor()
    {
        Expression left = ParseUnary();

        while (tokens[i].Type is TokenType.Mul or TokenType.Div or TokenType.Mod)
        {
            Token op = tokens[i];

            Next();

            Expression right = ParseUnary();

            left = new BinaryExpression(left, right, op.Type, op.Position);
        }

        return left;
    }

    Expression ParseTerm()
    {
        Expression left = ParseFactor();

        while (tokens[i].Type is TokenType.Add or TokenType.Sub)
        {
            Token op = tokens[i];

            Next();

            Expression right = ParseFactor();

            left = new BinaryExpression(left, right, op.Type, op.Position);
        }

        return left;
    }

    Expression ParseComparison()
    {
        Expression left = ParseTerm();

        while (tokens[i].Type is TokenType.LeftAngle or TokenType.RightAngle or TokenType.LeftAngleEquals or TokenType.RightAngleEquals)
        {
            Token op = tokens[i];

            Next();

            Expression right = ParseTerm();

            left = new BinaryExpression(left, right, op.Type, op.Position);
        }

        return left;
    }

    Expression ParseEquality()
    {
        Expression left = ParseComparison();

        while (tokens[i].Type is TokenType.Equals or TokenType.NotEquals)
        {
            Token op = tokens[i];

            Next();

            Expression right = ParseComparison();

            left = new BinaryExpression(left, right, op.Type, op.Position);
        }

        return left;
    }

    Expression ParseExpression()
    {
        return ParseEquality();
    }

    void Consume(TokenType tokenType, string message = "N\\A")
    {
        if (tokens[i].Type == tokenType)
        {
            Next();
            return;
        }
        throw new Errno(message, tokens[i].Position, ErrorLocation.Parser);
    }

    void Next()
    {
        if (AtEnd()) return;
        i++;
    }

    bool Check(TokenType tokenType)
    {
        if (i >= tokens.Count) return false;
        return tokens[i].Type == tokenType;
    }

    bool Match(TokenType tokenType)
    {
        if (Check(tokenType))
        {
            Consume(tokenType);
            return true;
        }
        return false;
    }

    bool AtEnd() => tokens[i].Type == TokenType.EOF;
}