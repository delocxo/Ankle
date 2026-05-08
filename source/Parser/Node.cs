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

class IfNode : Node
{
    public Expression Expression;
    public List<Node> Nodes;
    public bool HasElse;
    public List<Node> ElseNodes;

    public IfNode(Expression expression, List<Node> nodes, bool hasElse, List<Node> elseNodes, Position position) : base(position)
    {
        Expression = expression;
        Nodes = nodes;
        HasElse = hasElse;
        ElseNodes = elseNodes;
    }
}