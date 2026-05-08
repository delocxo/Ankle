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
    Print,
    Halt = 255,
}