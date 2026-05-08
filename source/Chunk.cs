struct Chunk
{
    public Value[] Constants;
    public Instruction[] Instructions;
    public int LocalsCount;

    public Chunk(Value[] constants, Instruction[] instructions, int localsCount)
    {
        Constants = constants;
        Instructions = instructions;
        LocalsCount = localsCount;
    }
}