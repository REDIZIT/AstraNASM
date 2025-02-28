using Astra.Shared;

namespace Astra.Compilation;

public static class CompiledModuleExtensions
{
    public static CompiledModule Compile(ResolvedModule resolved, byte[] bytes)
    {
        CompiledModule module = new()
        {
            metaTable = CreateMetaTable(resolved),
            managedCode = CreateManagedCode(bytes)
        };

        return module;
    }

    private static MetaTable CreateMetaTable(ResolvedModule module)
    {
        MetaTable table = new();

        table.types = new TypeInfo_Blit[module.types.Count];
        for (int i = 0; i < module.types.Count; i++)
        {
            TypeInfo type = module.types[i];
            TypeInfo_Blit blit = new()
            {
                name = type.name,
                isValueType = type.isValueType,
                fields = new FieldInfo_Blit[type.fields.Count],
                functions = new InModuleIndex[type.functions.Count],
            };

            for (int j = 0; j < type.fields.Count; j++)
            {
                FieldInfo field = type.fields[j];
                blit.fields[j] = new FieldInfo_Blit()
                {
                    name = field.name,
                    type = field.type.inModuleIndex
                };
            }
            
            for (int j = 0; j < type.functions.Count; j++)
            {
                FunctionInfo func = type.functions[j];
                blit.functions[j] = func.inModuleIndex;
            }

            table.types[i] = blit;
        }


        table.functions = new FunctionInfo_Blit[module.functions.Count];
        for (int i = 0; i < module.functions.Count; i++)
        {
            FunctionInfo func = module.functions[i];
            FunctionInfo_Blit blit = new()
            {
                name = func.name,
                isStatic = func.isStatic,
                isAbstract = func.isAbstract,
                ownerType = func.owner.inModuleIndex,
                arguments = new FieldInfo_Blit[func.arguments.Count],
                returns = new InModuleIndex[func.returns.Count],
                pointedModule = 0,
                pointedOpCode = func.pointedOpCode
            };
            
            for (int j = 0; j < func.arguments.Count; j++)
            {
                FieldInfo arg = func.arguments[j];
                blit.arguments[j] = new FieldInfo_Blit()
                {
                    name = arg.name,
                    type = arg.type.inModuleIndex
                };
            }
            
            for (int j = 0; j < func.returns.Count; j++)
            {
                TypeInfo ret = func.returns[j];
                blit.returns[j] = ret.inModuleIndex;
            }
            
            table.functions[i] = blit;
        }

        return table;
    }

    private static ManagedCode CreateManagedCode(byte[] byteCode)
    {
        return new ManagedCode()
        {
            byteCode = byteCode
        };
    }
}