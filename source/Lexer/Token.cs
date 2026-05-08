using System.Text;

public enum TokenType
{
    Int,
    String,
    Float,
    Bool,
    Identifier,
    Let,
    Const,
    Print,
    If,
    Else,
    EOF,

    LeftParen,
    RightParen,
    Semicolon,
    Equal,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Exclamation,
    Equals,
    NotEquals,
    LeftAngle,
    RightAngle,
    LeftAngleEquals,
    RightAngleEquals,
    LeftCurly,
    RightCurly,

}

struct Token
{
    public Value Value;
    public TokenType Type;
    public Position Position;

    public Token(Value value, TokenType type, Position position)
    {
        Value = value;
        Type = type;
        Position = position;
    }

    public static void Print(Token token)
    {
        Value value = token.Value;
        StringBuilder builder = new StringBuilder($"Type: {token.Type} | Kind: {value.ValueKind} | Value: ");

        switch (value.ValueKind)
        {
            case ValueKind.Int: builder.Append(value.IntValue); break;
            case ValueKind.Float: builder.Append(value.FloatValue); break;
            case ValueKind.String: builder.Append(value.StringValue); break;
            case ValueKind.Bool: builder.Append(value.BoolValue); break;
            case ValueKind.Void: builder.Append("Void"); break;
        }

        Console.WriteLine(builder.ToString());
    }
}

public static class TokenTypeDicts
{
    public static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
    {
        { "let", TokenType.Let },
        { "const", TokenType.Const },
        { "print", TokenType.Print },
        {"if", TokenType.If},
        {"else", TokenType.Else}
    };

    public static Dictionary<char, TokenType> symbols = new Dictionary<char, TokenType>()
    {
        { '(', TokenType.LeftParen },
        { ')', TokenType.RightParen },
        { ';', TokenType.Semicolon },
        { '+', TokenType.Add },
        { '-', TokenType.Sub },
        { '*', TokenType.Mul },
        { '/', TokenType.Div },
        { '%', TokenType.Mod },
        { '=', TokenType.Equal },
        { '!', TokenType.Exclamation },
        {'<', TokenType.LeftAngle},
        {'>', TokenType.RightAngle},
        {'{', TokenType.LeftCurly},
        {'}', TokenType.RightCurly}
    };

    public static Dictionary<string, TokenType> doubleSymbols = new Dictionary<string, TokenType>()
    {
        {"==", TokenType.Equals},
        {"!=", TokenType.NotEquals},
        {"<=", TokenType.LeftAngleEquals},
        {">=", TokenType.RightAngleEquals}
    };
}
