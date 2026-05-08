enum Opcode : byte
{
    Constant,
    LoadVar,
    StoreVar,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Neg,
    Not,
    Print,
    Halt = 255,
}