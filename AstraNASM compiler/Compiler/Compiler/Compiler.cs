using System.Collections.Generic;

namespace Astra.Compilation
{
    public static class Compiler
    {
        public static string Compile_Astra_to_NASM(string astraCode)
        {
            List<Token> tokens = Tokenizer.Tokenize(astraCode);

            List<Node> ast = AbstractSyntaxTreeBuilder.Parse(tokens);

            ResolvedModule module = Resolver.DiscoverModule(ast);

            string llvm = Generator.Generate(ast, module);

            return llvm;
        }
    }
}