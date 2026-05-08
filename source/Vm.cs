class Vm
{
    Chunk chunk = new Chunk(Array.Empty<Value>(), Array.Empty<Instruction>(), 0);

    public Vm(string compiledFile)
    {
        if (!File.Exists(compiledFile))
        {
            Console.WriteLine($"'{compiledFile}' does not exist");
            Environment.Exit(1);
        }

        using var file = File.OpenRead(compiledFile);
        using var read = new BinaryReader(file);

        int constantCount = read.ReadInt32();

        chunk.Constants = new Value[constantCount];

        for (int i = 0; i < constantCount; i++)
        {
            ValueKind kind = (ValueKind)read.ReadByte();
            Value value = ReadKind(kind, read);
            chunk.Constants[i] = value;
        }

        int instructionCount = read.ReadInt32();

        chunk.Instructions = new Instruction[instructionCount];

        for (int i = 0; i < instructionCount; i++)
        {
            Opcode opcode = (Opcode)read.ReadByte();

            ValueKind kind = (ValueKind)read.ReadByte();

            Value value = ReadKind(kind, read);

            int row = read.ReadInt32();
            int column = read.ReadInt32();

            chunk.Instructions[i] = new Instruction(opcode, value, new Position(row, column));
        }

        chunk.LocalsCount = read.ReadInt32();
    }

    Value ReadKind(ValueKind kind, BinaryReader read)
    {
        switch (kind)
        {
            case ValueKind.Int:
                {
                    return new Value(read.ReadInt64());
                }

            case ValueKind.Float:
                {
                    return new Value(read.ReadDouble());
                }

            case ValueKind.String:
                {
                    return new Value(read.ReadString());
                }

            case ValueKind.Bool:
                {
                    return new Value(read.ReadBoolean());
                }

            case ValueKind.Void:
                return new Value(ValueKind.Void);

            default:
                throw new Exception("This should never throw");
        }
    }

    public void Run()
    {
        Value[] locals = new Value[chunk.LocalsCount];
        Value[] stack = new Value[8192];
        int sp = 0;
        int ip = 0;

        void Push(Value value) => stack[sp++] = value;
        Value Pop() => stack[--sp];

        for (; ; )
        {
            Instruction instruction = chunk.Instructions[ip++];

            switch (instruction.Opcode)
            {
                case Opcode.Add:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue + right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue + right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue + right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue + right.FloatValue));
                                    break;
                                }

                            case (ValueKind.String, ValueKind.String):
                                {
                                    Push(new Value(left.StringValue + right.StringValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} + {left.ValueKind}' are not able to add", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Sub:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue - right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue - right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue - right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue - right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} - {left.ValueKind}' are not able to subtract", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Mul:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue * right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue * right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue * right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue * right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} * {left.ValueKind}' are not able to multiply", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Div:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue / right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue / right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue / right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue / right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} / {left.ValueKind}' are not able to divide", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Mod:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue % right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue % right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue % right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue % right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} % {left.ValueKind}' is invalid", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Equals:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue == right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue == right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue == right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue == right.FloatValue));
                                    break;
                                }

                            case (ValueKind.String, ValueKind.String):
                                {
                                    Push(new Value(left.StringValue == right.StringValue));
                                    break;
                                }

                            case (ValueKind.Bool, ValueKind.Bool):
                                {
                                    Push(new Value(left.BoolValue == right.BoolValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} == {left.ValueKind}' are not able to be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.NotEquals:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue != right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue != right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue != right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue != right.FloatValue));
                                    break;
                                }

                            case (ValueKind.String, ValueKind.String):
                                {
                                    Push(new Value(left.StringValue != right.StringValue));
                                    break;
                                }

                            case (ValueKind.Bool, ValueKind.Bool):
                                {
                                    Push(new Value(left.BoolValue != right.BoolValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} != {left.ValueKind}' are not able to be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.LeftAngle:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue < right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue < right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue < right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue < right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} < {left.ValueKind}' can not be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.RightAngle:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue > right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue > right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue > right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue > right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} > {left.ValueKind}' can not be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.LeftAngleEquals:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue <= right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue <= right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue <= right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue <= right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} <= {left.ValueKind}' can not be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.RightAngleEquals:
                    {
                        Value right = Pop();
                        Value left = Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    Push(new Value(left.IntValue >= right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    Push(new Value(left.FloatValue >= right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    Push(new Value(left.IntValue >= right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    Push(new Value(left.FloatValue >= right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} >= {left.ValueKind}' can not be compared", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Constant:
                    {
                        Push(chunk.Constants[instruction.Value.IntValue]);
                        break;
                    }

                case Opcode.LoadVar:
                    {
                        Push(locals[instruction.Value.IntValue]);
                        break;
                    }

                case Opcode.StoreVar:
                    {
                        locals[instruction.Value.IntValue] = Pop();
                        break;
                    }

                case Opcode.Neg:
                    {
                        Value value = Pop();
                        switch (value.ValueKind)
                        {
                            case ValueKind.Int: Push(new Value(-value.IntValue)); break;
                            case ValueKind.Float: Push(new Value(-value.FloatValue)); break;
                            default: throw new Errno("Can only negate ints or floats", instruction.Position, ErrorLocation.VirtualMachine);
                        }
                        break;
                    }

                case Opcode.Not:
                    {
                        Value value = Pop();
                        switch (value.ValueKind)
                        {
                            case ValueKind.Bool: Push(new Value(!value.BoolValue)); break;
                            default: throw new Errno("Can only flip bools", instruction.Position, ErrorLocation.VirtualMachine);
                        }
                        break;
                    }

                case Opcode.JumpIfFalse:
                    {
                        Value condition = Pop();
                        if (condition.ValueKind != ValueKind.Bool)
                        {
                            throw new Errno("Can not use non bools values on conditions", instruction.Position, ErrorLocation.VirtualMachine);
                        }
                        if (!condition.BoolValue)
                        {
                            ip = (int)instruction.Value.IntValue;
                        }
                        break;
                    }

                case Opcode.Jump:
                    {
                        ip = (int)instruction.Value.IntValue;
                        break;
                    }

                case Opcode.Print:
                    Pop().Print();
                    break;

                case Opcode.Halt:
                    {
                        return;
                    }


            }
        }
    }

}