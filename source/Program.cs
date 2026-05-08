try
{
    string code = File.ReadAllText("main.ankle");

    Lexer lexer = new Lexer(code);

    List<Token> tokens = lexer.Tokenize();

    tokens.ForEach(Token.Print);

    Parser parser = new Parser(tokens);

    List<Node> nodes = parser.Parse();

    Compiler compiler = new Compiler("main.anklec");

    compiler.Compile(nodes);

    compiler.Save();

    Vm vm = new Vm("main.anklec");
}
catch (Errno err)
{
    err.Print();
}