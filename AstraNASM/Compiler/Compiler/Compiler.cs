namespace Astra.Compilation;

public static class Compiler
{
    private static Lexer lexer = new();
    private static AstraAST parser = new();

    public static string Compile_Astra_to_NASM(string astraCode)
    {
        List<Token> tokens = lexer.Tokenize(astraCode, false);

        List<Node> ast = parser.Parse(tokens);

        ResolvedModule module = Resolver.DiscoverModule(ast);

        string llvm = Generator.Generate(ast, module);

        return llvm;
    }
}