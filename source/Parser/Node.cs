class Node
{
    public Position Position;

    public Node(Position position)
    {
        Position = position;
    }
}

class VariableNode : Node
{
    public string Name;
    public Expression Expression;
    public bool Const;

    public VariableNode(string name, Expression expression, bool constant, Position position) : base(position)
    {
        Name = name;
        Expression = expression;
        Const = constant;
    }
}

class PrintNode : Node
{
    public Expression Expression;

    public PrintNode(Expression expression, Position position) : base(position)
    {
        Expression = expression;
    }
}