namespace Astra.Compilation;

public static class Compiler
{
    private static Lexer lexer = new();
    private static AstraAST parser = new();

    public static string Compile_Astra_to_NASM(string astraCode, CompileTarget target)
    {
        List<Token> tokens = lexer.Tokenize(astraCode, false);

        ErrorLogger logger = new();

        List<Node> ast = parser.Parse(tokens, logger);

        if (logger.entries.Count > 0)
        {
            Console.WriteLine();
            foreach (LogEntry entry in logger.entries)
            {
                Console.WriteLine(entry.message);
                Console.WriteLine();
            }
            throw new Exception($"Compilation failed with {logger.entries.Count} errors. Read the console output to get exceptions details.");
        }

        ResolvedModule module = Resolver.DiscoverModule(ast);

        string llvm = Generator.Generate(ast, module, target);

        return llvm;
    }
}