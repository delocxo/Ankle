class Expression
{
    public Position Position;

    public Expression(Position position)
    {
        Position = position;
    }
}

class LiteralExpression : Expression
{
    public Value Value;

    public LiteralExpression(Value value, Position position) : base(position)
    {
        Value = value;
    }
}

class NameExpression : Expression
{
    public string Name;

    public NameExpression(string name, Position position) : base(position)
    {
        Name = name;
    }
}

class UnaryExpression : Expression
{
    public Expression Right;
    public TokenType Operator;

    public UnaryExpression(Expression right, TokenType op, Position position) : base(position)
    {
        Right = right;
        Operator = op;
    }
}

class BinaryExpression : Expression
{
    public Expression Left;
    public Expression Right;
    public TokenType Operator;

    public BinaryExpression(Expression left, Expression right, TokenType op, Position position) : base(position)
    {
        Left = left;
        Right = right;
        Operator = op;
    }
}