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

        Consume(TokenType.Equals, "Expected '='");

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

    Expression ParseExpression()
    {
        return ParseTerm();
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
        if (AtEnd()) return false;
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