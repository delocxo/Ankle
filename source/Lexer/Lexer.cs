using System.Text;

class Lexer
{
    string code;
    int i;
    Position currentPosition;

    public Lexer(string code)
    {
        this.code = code;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new List<Token>();

        while (i < code.Length)
        {
            if (code[i] == '"')
            {
                tokens.Add(TokenizeString());
                continue;
            }

            if (char.IsDigit(code[i]))
            {
                tokens.Add(TokenizeNumber());
                continue;
            }

            if (char.IsLetter(code[i]) || code[i] == '_')
            {
                tokens.Add(TokenizeLetter());
                continue;
            }

            if (i + 1 < code.Length)
            {
                char peak = code[i + 1];
                string value = $"{code[i]}{peak}";

                if (TokenTypeDicts.doubleSymbols.TryGetValue(value, out TokenType doubleSymbolType))
                {
                    tokens.Add(new Token(new Value(value), doubleSymbolType, currentPosition));
                    Next();
                    Next();
                    continue;
                }
            }

            if (TokenTypeDicts.symbols.TryGetValue(code[i], out TokenType symboleType))
            {
                tokens.Add(new Token(new Value(ValueKind.Void), symboleType, currentPosition));
                Next();
                continue;
            }

            Next();
        }

        tokens.Add(new Token(new Value(ValueKind.Void), TokenType.EOF, currentPosition));
        return tokens;
    }

    Token TokenizeString()
    {
        Position position = currentPosition;

        Next();

        StringBuilder builder = new StringBuilder();

        while (i < code.Length && code[i] != '"')
        {
            if (code[i] == '\\')
            {
                Next();

                switch (code[i])
                {
                    case 'n': builder.Append("\n"); break;
                    case 't': builder.Append("\t"); break;
                    case 'b': builder.Append("\b"); break;
                    case '"': builder.Append("\""); break;
                    case '\\': builder.Append("\\"); break;
                    case 'r': builder.Append("\r"); break;
                    case 'v': builder.Append("\v"); break;
                    case 'f': builder.Append("\f"); break;
                    case '0': builder.Append("\0"); break;
                    default:
                        throw new Errno($"Invalid escape character '\\{code[i]}'", currentPosition, ErrorLocation.Lexer);
                }

                Next();
                continue;
            }

            if (code[i] == '\n')
            {
                throw new Errno("Unterminated string", currentPosition, ErrorLocation.Lexer);
            }

            builder.Append(code[i]);
            Next();
        }

        if (i >= code.Length)
        {
            throw new Errno("Unterminated string", currentPosition, ErrorLocation.Lexer);
        }

        Next();

        return new Token(new Value(builder.ToString()), TokenType.String, position);
    }

    Token TokenizeNumber()
    {
        Position position = currentPosition;

        int start = i;
        bool hasDecimal = false;
        bool declaredFloatSuffix = false;
        Position decimalPosition = new Position();

        while (i < code.Length && (char.IsDigit(code[i]) || code[i] == '.' || code[i] == 'f'))
        {
            if (code[i] == '.')
            {
                if (hasDecimal)
                {
                    throw new Errno("More than one decimal point was found", currentPosition, ErrorLocation.Lexer);
                }

                if (i + 1 >= code.Length || !char.IsDigit(code[i + 1]))
                {
                    throw new Errno("Expected number after decimal point", currentPosition, ErrorLocation.Lexer);
                }

                hasDecimal = true;
                decimalPosition = currentPosition;

                Next();
                continue;
            }

            if (code[i] == 'f')
            {
                declaredFloatSuffix = true;
                Next();
                break;
            }

            Next();
        }

        if (hasDecimal && !declaredFloatSuffix)
        {
            throw new Errno("Cannot declare a float without a float suffix", decimalPosition, ErrorLocation.Lexer);
        }

        string value = code.Substring(start, i - start - (declaredFloatSuffix ? 1 : 0));

        if (hasDecimal || declaredFloatSuffix)
        {
            return new Token(new Value(double.Parse(value)), TokenType.Float, position);
        }

        return new Token(new Value(long.Parse(value)), TokenType.Int, position);
    }

    Token TokenizeLetter()
    {
        Position position = currentPosition;

        int start = i;

        while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
        {
            Next();
        }

        string value = code.Substring(start, i - start);

        if (value == "true")
        {
            return new Token(new Value(true), TokenType.Bool, position);
        }

        if (value == "false")
        {
            return new Token(new Value(false), TokenType.Bool, position);
        }

        if (TokenTypeDicts.keywords.TryGetValue(value, out TokenType tokenType))
        {
            return new Token(new Value(value), tokenType, position);
        }

        return new Token(new Value(value), TokenType.Identifier, position);
    }

    void Next()
    {
        if (code[i] == '\n')
        {
            currentPosition.Row++;
            currentPosition.Column = 1;
        }
        else
        {
            currentPosition.Column++;
        }

        i++;
    }
}
