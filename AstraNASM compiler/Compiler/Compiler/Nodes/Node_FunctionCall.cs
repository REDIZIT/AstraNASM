using System;
using System.Collections.Generic;

namespace Astra.Compilation
{
    public class Node_FunctionCall : Node
    {
        public Node caller;
        public List<Node> arguments;
        public string functionName;

        public FunctionInfo function;

        public override void RegisterRefs(RawModule module)
        {
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            function = module.functionInfoByName[functionName];
        }

        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            if (function is EmbeddedFunctionInfo embeddedFunctionInfo)
            {
                GenerateEmbedded(ctx, embeddedFunctionInfo);
            }
            else
            {
                GenerateRegular(ctx);
            }
        }

        private void GenerateRegular(Generator.Context ctx)
        {
            ctx.b.Space();
            ctx.b.CommentLine($"{caller}.{function.name}()");


            ctx.b.CommentLine($"arguments generation");

            bool isStatic = function.owner == null;

            Variable[] argumentsResults = new Variable[isStatic ? arguments.Count : arguments.Count + 1];

            for (int i = 0; i < arguments.Count; i++)
            {
                Node node = arguments[i];
                if (node is Node_Literal) continue;

                node.Generate(ctx);
                argumentsResults[isStatic ? 0 : 1] = node.result;
            }


            if (function.returns.Count > 0)
            {
                if (function.returns.Count > 1) throw new Exception("Not supported yet");

                result = ctx.AllocateStackVariable(function.returns[0]);

                ctx.b.CommentLine($"return allocation");
                ctx.b.Line($"sub rsp, 8");
            }


            ctx.b.CommentLine($"arguments pushing");

            if (isStatic == false)
            {
                Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;

                Variable selfVar = ctx.GetVariable(variableNode.variableName);
                ctx.AllocateStackVariable(selfVar.type);

                ctx.b.Line($"mov rax, {selfVar.RBP} ; self");
                ctx.b.Line($"push rax");

                argumentsResults[0] = selfVar;
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                Variable result = argumentsResults[isStatic ? 0 : 1];
                FieldInfo argInfo = function.arguments[i];

                if (arguments[i] is Node_Literal literal)
                {
                    ctx.b.Line($"mov rax, {literal.constant.value} ; arg[{i}] = {argInfo.name}");
                }
                else
                {
                    ctx.AllocateStackVariable(result.type, argInfo.name);
                    ctx.b.Line($"mov rax, {result.GetRBP()} ; arg[{i}] = {argInfo.name}");
                }

                ctx.b.Line($"push rax");
            }



            ctx.b.Line($"call {function.name}");



            int argumentsSizeInBytes = 0;
            for (int i = 0; i < argumentsResults.Length; i++)
            {
                Variable arg = argumentsResults[i];

                if (arg != null) ctx.Release(arg);

                argumentsSizeInBytes += 8;
            }

            ctx.b.Line($"add rsp, {argumentsSizeInBytes}");
        }



        private void GenerateEmbedded(Generator.Context ctx, EmbeddedFunctionInfo embeddedFunctionInfo)
        {
            if (embeddedFunctionInfo is ToPtr_EmbeddedFunctionInfo toPtr)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string valueVariableName = variable.variableName;
                result = toPtr.Generate(ctx, valueVariableName);
            }
            else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                result = ptrSet.Generate(ctx, pointerVariableName, arguments[0].result);
            }
            else if (embeddedFunctionInfo is PtrGet_EmbeddedFunctionInfo ptrGet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                result = ptrGet.Generate(ctx, pointerVariableName);
            }
            else if (embeddedFunctionInfo is PtrShift_EmbeddedFunctionInfo ptrShift)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                result = ptrShift.Generate(ctx, pointerVariableName, arguments[0].result);
            }
            else if (embeddedFunctionInfo is Print_EmbeddedFunctionInfo print)
            {
                Node variable = arguments[0];
                variable.Generate(ctx);

                print.Generate(ctx, variable.result);
            }
            else
            {
                throw new Exception($"Unknown EmbeddedFunctionInfo '{embeddedFunctionInfo}'");
            }
        }
    }
}