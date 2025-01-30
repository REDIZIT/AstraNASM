namespace Astra.Compilation;

public static class Compiler
{
    private static Lexer lexer = new();

    public static string Compile_Astra_to_NASM(string astraCode)
    {
        List<Token> tokens = lexer.Tokenize(astraCode, false);

        List<Node> ast = AbstractSyntaxTreeBuilder.Parse(tokens);

        ResolvedModule module = Resolver.DiscoverModule(ast);

        string llvm = Generator.Generate(ast, module);

        return llvm;
    }
}