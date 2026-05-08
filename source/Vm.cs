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
        Stack<Value> stack = new Stack<Value>(2048);
        int ip = 0;

        for (; ; )
        {
            Instruction instruction = chunk.Instructions[ip++];

            switch (instruction.Opcode)
            {
                case Opcode.Constant:
                    {
                        stack.Push(chunk.Constants[instruction.Value.IntValue]);
                        break;
                    }

                case Opcode.LoadVar:
                    {
                        stack.Push(locals[instruction.Value.IntValue]);
                        break;
                    }

                case Opcode.StoreVar:
                    {
                        locals[instruction.Value.IntValue] = stack.Pop();
                        break;
                    }

                case Opcode.Add:
                    {
                        Value right = stack.Pop();
                        Value left = stack.Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.IntValue + right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.FloatValue + right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.IntValue + right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.FloatValue + right.FloatValue));
                                    break;
                                }

                            case (ValueKind.String, ValueKind.String):
                                {
                                    stack.Push(new Value(left.StringValue + right.StringValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} + {left.ValueKind}' are not able to add", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Sub:
                    {
                        Value right = stack.Pop();
                        Value left = stack.Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.IntValue - right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.FloatValue - right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.IntValue - right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.FloatValue - right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} - {left.ValueKind}' are not able to subtract", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Mul:
                    {
                        Value right = stack.Pop();
                        Value left = stack.Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.IntValue * right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.FloatValue * right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.IntValue * right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.FloatValue * right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} * {left.ValueKind}' are not able to multiply", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Div:
                    {
                        Value right = stack.Pop();
                        Value left = stack.Pop();

                        switch (right.ValueKind, left.ValueKind)
                        {
                            case (ValueKind.Int, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.IntValue / right.IntValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Int):
                                {
                                    stack.Push(new Value(left.FloatValue / right.IntValue));
                                    break;
                                }

                            case (ValueKind.Int, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.IntValue / right.FloatValue));
                                    break;
                                }

                            case (ValueKind.Float, ValueKind.Float):
                                {
                                    stack.Push(new Value(left.FloatValue / right.FloatValue));
                                    break;
                                }

                            default:
                                throw new Errno($"'{left.ValueKind} / {left.ValueKind}' are not able to divide", instruction.Position, ErrorLocation.VirtualMachine);
                        }

                        break;
                    }

                case Opcode.Print:
                    stack.Pop().Print();
                    break;

                case Opcode.Halt:
                    {
                        return;
                    }
            }
        }
    }
}