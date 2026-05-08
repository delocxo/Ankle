enum ErrorLocation
{
    Lexer,
    Parser,
    Compiler,
    VirtualMachine
}

class Errno : Exception
{
    public Position Position;
    public ErrorLocation Location;

    public Errno(string message, Position position, ErrorLocation errorLocation) : base(message)
    {
        Position = position;
        Location = errorLocation;
    }

    public void Print()
    {
        Console.WriteLine($"{Location} Error at line {Position.Row}, column {Position.Column}");
        Console.WriteLine($"{Message}");
    }
}