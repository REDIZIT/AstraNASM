using System;
using System.Collections.Generic;

namespace Astra.Compilation
{
    public class Node_Function : Node
    {
        public string name;
        public Node body;
        public List<VariableRawData> parameters = new List<VariableRawData>();
        public List<VariableRawData> returnValues = new List<VariableRawData>();

        private FunctionInfo functionInfo;

        public override void RegisterRefs(RawModule raw)
        {
            RawFunctionInfo rawInfo = new RawFunctionInfo()
            {
                name = name
            };

            foreach (VariableRawData data in parameters)
            {
                rawInfo.arguments.Add(new RawFieldInfo()
                {
                    name = data.name,
                    typeName = data.rawType,
                });
            }

            foreach (VariableRawData data in returnValues)
            {
                rawInfo.returns.Add(new RawTypeInfo()
                {
                    name = data.rawType
                });
            }

            raw.RegisterFunction(rawInfo);

            body.RegisterRefs(raw);
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            body.ResolveRefs(module);
            foreach (VariableRawData rawData in parameters)
            {
                rawData.Resolve(module);
            }
            foreach (VariableRawData rawData in returnValues)
            {
                rawData.Resolve(module);
            }

            functionInfo = module.functionInfoByName[name];

            ResolveReturnNodesRecursive((Node_Block)body);
        }

        private void ResolveReturnNodesRecursive(Node_Block block)
        {
            foreach (Node childNode in block.children)
            {
                if (childNode is Node_Return returnNode)
                {
                    returnNode.function = functionInfo;
                }
                else if (childNode is Node_Block anotherBlock)
                {
                    ResolveReturnNodesRecursive(anotherBlock);
                }
                else if (childNode is Node_If ifNode)
                {
                    if (ifNode.thenBranch is Node_Block thenBlock) ResolveReturnNodesRecursive(thenBlock);
                    if (ifNode.elseBranch is Node_Block elseBlock) ResolveReturnNodesRecursive(elseBlock);
                }
            }
        }

        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            if (returnValues.Count > 1)
            {
                throw new NotImplementedException("Function has 1+ return values. Generation for this is not supported yet");
            }

            //List<string> paramsDeclars = new();
            //foreach (VariableRawData param in parameters)
            //{
            //    string generatedName = ctx.NextPointerVariableName(param.type, param.name);
            //    string generatedType = (param.type is PrimitiveTypeInfo) ? param.type.ToString() : "ptr";

            //    paramsDeclars.Add($"{generatedType} {generatedName}");
            //}
            //string paramsStr = string.Join(", ", paramsDeclars);


            //ctx.b.Space(3);

            //if (returnValues.Count == 0)
            //{
            //    ctx.b.Line($"define void @{name}({paramsStr})");
            //}
            //else
            //{
            //    ctx.b.Line($"define {returnValues[0].type} @{name}({paramsStr})");
            //}
        
            //ctx.b.Line("{");

       

            ctx = ctx.CreateSubContext();

            ctx.b.Space(3);

        

            ctx.b.Line($"{name}:");
            ctx.b.Line("push rbp");
            ctx.b.Line("mov rbp, rsp");

            ctx.b.Space(1);

            int index = 0;

            for (int i = functionInfo.arguments.Count - 1; i >= 0; i--)
            {
                FieldInfo argInfo = functionInfo.arguments[i];
                ctx.Register_FunctionArgumentVariable(argInfo, index);
                index++;
            }

        
            if (functionInfo.owner != null)
            {
                ctx.Register_FunctionArgumentVariable(new FieldInfo()
                {
                    name = "self",
                    type = functionInfo.owner
                }, index);
                index++;
            }

            body.Generate(ctx);

            ctx.b.Space(1);
        }
    }
}