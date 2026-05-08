class Instruction
{
    public Opcode Opcode;
    public Value Value;
    public Position Position;

    public Instruction(Opcode opecode, Value value, Position position)
    {
        Opcode = opecode;
        Value = value;
        Position = position;
    }

    public void Print()
    {
        Console.WriteLine($"Opcode: {Opcode} | Value: {Value.GetValue()}");
    }
}