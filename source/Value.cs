enum ValueKind : byte
{
    Int,
    Float,
    String,
    Bool,
    Void
}

struct Value
{
    public ValueKind ValueKind;
    public long IntValue;
    public double FloatValue;
    public string StringValue = string.Empty;
    public bool BoolValue;

    public Value(long value)
    {
        IntValue = value;
        ValueKind = ValueKind.Int;
    }

    public Value(double value)
    {
        FloatValue = value;
        ValueKind = ValueKind.Float;
    }

    public Value(string value)
    {
        StringValue = value;
        ValueKind = ValueKind.String;
    }

    public Value(bool value)
    {
        BoolValue = value;
        ValueKind = ValueKind.Bool;
    }

    public Value(ValueKind valueKind)
    {
        ValueKind = valueKind;
    }

    public void Print()
    {
        switch (ValueKind)
        {
            case ValueKind.Int: Console.Write(IntValue); break;
            case ValueKind.Float: Console.Write(FloatValue); break;
            case ValueKind.String: Console.Write(StringValue); break;
            case ValueKind.Bool: Console.Write(BoolValue); break;
            case ValueKind.Void: Console.Write("Void"); break;
        }
    }

    public object? GetValue()
    {
        switch (ValueKind)
        {
            case ValueKind.Int: return IntValue;
            case ValueKind.Float: return FloatValue;
            case ValueKind.String: return StringValue;
            case ValueKind.Bool: return BoolValue;
            case ValueKind.Void: return "Void";
        }
        return "N\\A";
    }
}