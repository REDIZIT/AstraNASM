namespace Astra.Compilation
{
    public class Node_VariableDeclaration : Node
    {
        public VariableRawData variable;
        public Node initValue;

        public override void RegisterRefs(RawModule module)
        {
            initValue?.RegisterRefs(module);
        }

        public override void ResolveRefs(ResolvedModule module)
        {
            initValue?.ResolveRefs(module);
            variable.Resolve(module);
        }

        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

       

            if (initValue == null)
            {
                Generate_WithDefaultValue(ctx);
            }
            else if (initValue is Node_Literal literal)
            {
                Generate_WithInit_Literal(ctx, literal);
            }
            else if (initValue is Node_New tokenNew)
            {
                Generate_WithInit_New(ctx, tokenNew);
            }
            else
            {
                Generate_WithInit_AnyExpression(ctx);
            }

            ctx.b.Space();
        }

        private void Generate_WithDefaultValue(Generator.Context ctx)
        {
            result = ctx.AllocateStackVariable(variable.type, variable.name);
            ctx.b.Line($"mov {result.GetRBP()}, 0");
        }
        private void Generate_WithInit_Literal(Generator.Context ctx, Node_Literal literal)
        {
            result = ctx.AllocateStackVariable(variable.type, variable.name);
            ctx.b.Line($"mov {result.GetRBP()}, {literal.constant.value}");
        }
        private void Generate_WithInit_AnyExpression(Generator.Context ctx)
        {
            initValue.Generate(ctx);

            result = initValue.result;
            result.name = variable.name;
        }
        private void Generate_WithInit_New(Generator.Context ctx, Node_New tokenNew)
        {
            tokenNew.Generate(ctx);

            result = tokenNew.result;
            result.name = variable.name;
        }
    }

    public class Node_VariableUse : Node
    {
        public string variableName;

        public override void RegisterRefs(RawModule module)
        {
        }
        public override void ResolveRefs(ResolvedModule module)
        {
        }

        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            result = ctx.GetVariable(variableName);
            //generatedVariableName = "%" + variableName;
        }
    }
}
