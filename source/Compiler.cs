class Variable
{
    public bool Const;
    public int Slot;

    public Variable(bool constant, int slot)
    {
        Const = constant;
        Slot = slot;
    }
}

class Compiler
{
    string target;
    List<Instruction> instructions = new List<Instruction>();
    List<Value> constants = new List<Value>();
    Dictionary<string, Variable>?[] scopeStack = new Dictionary<string, Variable>?[64];
    int scopeDepth;
    int nextSlot;

    public Compiler(string target)
    {
        this.target = target;
        scopeStack[0] = new Dictionary<string, Variable>();
    }

    public void Compile(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            CompileStatement(node);
        }
        Emit(Opcode.Halt, new Value(ValueKind.Void), new Position(0, 0));
    }

    void CompileBlock(List<Node> nodes)
    {
        scopeDepth++;
        scopeStack[scopeDepth] = new Dictionary<string, Variable>();
        foreach (Node node in nodes)
        {
            CompileStatement(node);
        }
        scopeStack[scopeDepth] = null;
        scopeDepth--;
    }

    Dictionary<string, Variable> CurrentScope()
    {
        Dictionary<string, Variable>? scope = scopeStack[scopeDepth];
        if (scope == null)
        {
            throw new Exception("Compiler scope not created");
        }
        return scope;
    }

    Variable? ResolveVariable(string target)
    {
        int currentDepth = scopeDepth;
        while (currentDepth >= 0)
        {
            var scope = scopeStack[currentDepth];
            if (scope != null && scope.TryGetValue(target, out Variable? variable))
            {
                return variable;
            }
            currentDepth--;
        }
        return null;
    }

    void CompileStatement(Node node)
    {
        switch (node)
        {
            case VariableNode variableNode:
                {
                    CompileExpression(variableNode.Expression);
                    var scope = CurrentScope();

                    if (scope.ContainsKey(variableNode.Name))
                    {
                        throw new Errno($"'{variableNode.Name}' already exist", node.Position, ErrorLocation.Compiler);
                    }

                    scope.Add(variableNode.Name, new Variable(variableNode.Const, nextSlot));
                    Emit(Opcode.StoreVar, new Value(nextSlot), node.Position);
                    nextSlot++;
                    break;
                }

            case PrintNode printNode:
                {
                    CompileExpression(printNode.Expression);
                    Emit(Opcode.Print, new Value(ValueKind.Void), node.Position);
                    break;
                }

            case IfNode ifNode:
                {
                    CompileExpression(ifNode.Expression);
                    int jumpToElse = EmitJumpIfFalse(0, ifNode.Position);

                    CompileBlock(ifNode.Nodes);

                    if (ifNode.HasElse)
                    {
                        int jumpToEnd = EmitJump(0, ifNode.Position);
                        int elseStart = instructions.Count;
                        PatchJump(jumpToElse, elseStart);
                        CompileBlock(ifNode.ElseNodes);
                        int afterIf = instructions.Count;
                        PatchJump(jumpToEnd, afterIf);
                    }
                    else
                    {
                        int afterIf = instructions.Count;
                        PatchJump(jumpToElse, afterIf);
                    }
                    break;
                }

            case AssignNode assignNode:
                {
                    Variable? variable = ResolveVariable(assignNode.Name);

                    if (variable == null)
                    {
                        throw new Errno($"'{assignNode.Name}' does not exist", assignNode.Position, ErrorLocation.Compiler);
                    }

                    if (variable.Const)
                    {
                        throw new Errno($"'{assignNode.Name}' is a const and cant be set", assignNode.Position, ErrorLocation.Compiler);
                    }

                    Emit(Opcode.LoadVar, new Value(variable.Slot), assignNode.Position);

                    if (assignNode.HasExpression)
                    {
                        CompileExpression(assignNode.Expression);
                    }

                    switch (assignNode.Operator)
                    {
                        case TokenType.PlusEqual:
                            {
                                Emit(Opcode.Add, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.MinusEqual:
                            {
                                Emit(Opcode.Sub, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.TimesEqual:
                            {
                                Emit(Opcode.Mul, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.DivideEqual:
                            {
                                Emit(Opcode.Div, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.Increment:
                            {
                                int constantIndex = AddConstant(new Value(1));
                                Emit(Opcode.Constant, new Value(constantIndex), assignNode.OperatorPosition);
                                Emit(Opcode.Add, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.Decrement:
                            {
                                int constantIndex = AddConstant(new Value(1));
                                Emit(Opcode.Constant, new Value(constantIndex), assignNode.OperatorPosition);
                                Emit(Opcode.Sub, new Value(ValueKind.Void), assignNode.OperatorPosition);
                                break;
                            }

                        case TokenType.Equal:
                            {
                                break;
                            }
                    }

                    Emit(Opcode.StoreVar, new Value(variable.Slot), assignNode.OperatorPosition);
                    break;
                }

            case WhileNode whileNode:
                {
                    int loopStart = instructions.Count;
                    CompileExpression(whileNode.Expression);
                    int jumpToEnd = EmitJumpIfFalse(0, whileNode.Position);
                    CompileBlock(whileNode.Nodes);
                    EmitJump(loopStart, whileNode.Position);
                    int afterLoop = instructions.Count;
                    PatchJump(jumpToEnd, afterLoop);
                    break;
                }
        }
    }

    int EmitJumpIfFalse(int target, Position position)
    {
        Instruction instruction = new Instruction(Opcode.JumpIfFalse, new Value(target), position);
        instructions.Add(instruction);
        return instructions.IndexOf(instruction);
    }

    int EmitJump(int target, Position position)
    {
        Instruction instruction = new Instruction(Opcode.Jump, new Value(target), position);
        instructions.Add(instruction);
        return instructions.IndexOf(instruction);
    }

    void PatchJump(int index, int target)
    {
        Instruction instruction = instructions[index];
        instruction.Value = new Value(target);
        instructions[index] = instruction;
    }

    void CompileExpression(Expression expression)
    {
        switch (expression)
        {
            case LiteralExpression literalExpression:
                {
                    int constantIndex = AddConstant(literalExpression.Value);
                    Emit(Opcode.Constant, new Value(constantIndex), expression.Position);
                    break;
                }

            case NameExpression nameExpression:
                {
                    Variable? variable = ResolveVariable(nameExpression.Name);
                    if (variable == null)
                    {
                        throw new Errno($"'{nameExpression.Name}' does not exist", expression.Position, ErrorLocation.Compiler);
                    }
                    Emit(Opcode.LoadVar, new Value(variable.Slot), nameExpression.Position);
                    break;
                }

            case UnaryExpression unaryExpression:
                {
                    CompileExpression(unaryExpression.Right);
                    switch (unaryExpression.Operator)
                    {
                        case TokenType.Sub:
                            {
                                Emit(Opcode.Neg, new Value(ValueKind.Void), unaryExpression.Position);
                                break;
                            }

                        case TokenType.Exclamation:
                            {
                                Emit(Opcode.Not, new Value(ValueKind.Void), unaryExpression.Position);
                                break;
                            }
                    }
                    break;
                }

            case BinaryExpression binaryExpression:
                {
                    CompileExpression(binaryExpression.Left);
                    CompileExpression(binaryExpression.Right);

                    switch (binaryExpression.Operator)
                    {
                        case TokenType.Add:
                            Emit(Opcode.Add, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.Sub:
                            Emit(Opcode.Sub, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.Mul:
                            Emit(Opcode.Mul, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.Div:
                            Emit(Opcode.Div, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.Mod:
                            Emit(Opcode.Mod, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.Equals:
                            Emit(Opcode.Equals, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.NotEquals:
                            Emit(Opcode.NotEquals, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.LeftAngle:
                            Emit(Opcode.LeftAngle, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.RightAngle:
                            Emit(Opcode.RightAngle, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.LeftAngleEquals:
                            Emit(Opcode.LeftAngleEquals, new Value(ValueKind.Void), expression.Position);
                            break;

                        case TokenType.RightAngleEquals:
                            Emit(Opcode.RightAngleEquals, new Value(ValueKind.Void), expression.Position);
                            break;
                    }

                    break;
                }
        }
    }

    int AddConstant(Value value)
    {
        if (constants.Contains(value))
        {
            return constants.IndexOf(value);
        }
        constants.Add(value);
        return constants.IndexOf(value);
    }

    void Emit(Opcode opecode, Value value, Position position)
    {
        instructions.Add(new Instruction(opecode, value, position));
    }

    public void Save()
    {
        using var file = File.Create(target);
        using var write = new BinaryWriter(file);

        write.Write(constants.Count);

        foreach (Value value in constants)
        {
            SaveValue(value, write);
        }

        write.Write(instructions.Count);

        foreach (Instruction instruction in instructions)
        {
            write.Write((byte)instruction.Opcode);

            Value value = instruction.Value;

            SaveValue(value, write);

            write.Write(instruction.Position.Row);
            write.Write(instruction.Position.Column);
        }

        write.Write(nextSlot);
    }

    void SaveValue(Value value, BinaryWriter write)
    {
        write.Write((byte)value.ValueKind);
        switch (value.ValueKind)
        {
            case ValueKind.Int: write.Write(value.IntValue); break;
            case ValueKind.Float: write.Write(value.FloatValue); break;
            case ValueKind.String: write.Write(value.StringValue); break;
            case ValueKind.Bool: write.Write(value.BoolValue); break;
        }
    }

    public void Print()
    {
        instructions.ForEach(i => i.Print());
    }
}