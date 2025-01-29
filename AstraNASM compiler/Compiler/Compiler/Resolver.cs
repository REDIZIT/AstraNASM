using System.Collections.Generic;

namespace Astra.Compilation
{
    public static class Resolver
    {
        public static ResolvedModule DiscoverModule(List<Node> ast)
        {
            //Access.Set(Stage.DisoverModule_Begin);



            RawModule raw = new RawModule();

            AppendRawLLVMTypes(raw);
            //Access.Set(Stage.DisoverModule_Done_LLVMTypes);


            foreach (Node node in ast)
            {
                node.RegisterRefs(raw);
            }
            //Access.Set(Stage.DisoverModule_Done_RegisterRefs);





            ResolvedModule resolved = ResolveRawModule(raw);
            RegisterEmbeddedFunctions(resolved);

            foreach (Node node in ast)
            {
                node.ResolveRefs(resolved);
            }
            //Access.Set(Stage.DisoverModule_Done_ResolveRefs);




            return resolved;
        }


        private static ResolvedModule ResolveRawModule(RawModule raw)
        {
            ResolvedModule resolved = new ResolvedModule();
            AppendResolvedLLVMTypes(resolved);

            //
            // Resolve Types (include custom Classes/Structs)
            //
            foreach (RawTypeInfo rawInfo in raw.typeInfoByName.Values)
            {
                // Primities already resolved in AppendResolvedLLVMTypes
                if (rawInfo is RawPrimitiveTypeInfo) continue;


                TypeInfo typeInfo;
                if (rawInfo is RawClassTypeInfo)
                {
                    typeInfo = new ClassTypeInfo()
                    {
                        name = rawInfo.name
                    };
                }
                else
                {
                    typeInfo = new TypeInfo()
                    {
                        name = rawInfo.name
                    };
                }

                resolved.RegisterType(typeInfo);
            }


            //
            // Resolve Functions
            //
            foreach (RawFunctionInfo rawInfo in raw.functionInfoByName.Values)
            {
                FunctionInfo functionInfo = new FunctionInfo()
                {
                    name = rawInfo.name
                };

                foreach (RawFieldInfo rawArgInfo in rawInfo.arguments)
                {
                    functionInfo.arguments.Add(new FieldInfo()
                    {
                        name = rawArgInfo.name,
                        type = resolved.GetType(rawArgInfo.typeName),
                    });
                }
                foreach (RawTypeInfo rawTypeInfo in rawInfo.returns)
                {
                    functionInfo.returns.Add(resolved.GetType(rawTypeInfo));
                }
                resolved.RegisterFunction(functionInfo);
            }

            //
            // Resolve Classes
            //
            foreach (RawClassTypeInfo rawInfo in raw.classInfoByName.Values)
            {
                ClassTypeInfo classInfo = (ClassTypeInfo)resolved.GetType(rawInfo.name);

                foreach (RawFieldInfo rawField in rawInfo.fields)
                {
                    FieldInfo fieldInfo = new FieldInfo()
                    {
                        name = rawField.name,
                        type = resolved.GetType(rawField.typeName)
                    };

                    classInfo.fields.Add(fieldInfo);
                }

                foreach (RawFunctionInfo rawFunc in rawInfo.functions)
                {
                    FunctionInfo funcInfo = resolved.functionInfoByName[rawFunc.name];
                    funcInfo.owner = classInfo;

                    classInfo.functions.Add(funcInfo);
                }

                resolved.RegisterClass(classInfo);

            }

            return resolved;
        }

        private static void RegisterEmbeddedFunctions(ResolvedModule module)
        {
            ToPtr_EmbeddedFunctionInfo to_ptr = new ToPtr_EmbeddedFunctionInfo()
            {
                name = "to_ptr",
                returns = new List<TypeInfo>() { PrimitiveTypeInfo.PTR },
            };
            module.RegisterFunction(to_ptr);

            PtrSet_EmbeddedFunctionInfo set = new PtrSet_EmbeddedFunctionInfo()
            {
                name = "set",
                arguments = new List<FieldInfo>() { new FieldInfo(PrimitiveTypeInfo.INT, "newValue") }
            };
            module.RegisterFunction(set);

            PtrGet_EmbeddedFunctionInfo get = new PtrGet_EmbeddedFunctionInfo()
            {
                name = "get",
                returns = new List<TypeInfo>() { PrimitiveTypeInfo.INT },
            };
            module.RegisterFunction(get);

            PtrShift_EmbeddedFunctionInfo shift = new PtrShift_EmbeddedFunctionInfo()
            {
                name = "shift",
                arguments = new List<FieldInfo>() { new FieldInfo(PrimitiveTypeInfo.INT, "offseInBytes") }
            };
            module.RegisterFunction(shift);

            Print_EmbeddedFunctionInfo print = new Print_EmbeddedFunctionInfo()
            {
                name = "print",
                arguments = new List<FieldInfo>() { new FieldInfo(PrimitiveTypeInfo.PTR, "pointer") }
            };
            module.RegisterFunction(print);
        }

        private static void AppendRawLLVMTypes(RawModule module)
        {
            CreateType(module, "bool", "i1");
            CreateType(module, "byte", "i8");
            CreateType(module, "short", "i16");
            CreateType(module, "int", "i32");
            CreateType(module, "long", "i64");


            RawPrimitiveTypeInfo ptrType = new RawPrimitiveTypeInfo()
            {
                name = "ptr",
                asmName = "ptr"
            };
            module.typeInfoByName[ptrType.name] = ptrType;

            RawPrimitiveTypeInfo arrayType = new RawPrimitiveTypeInfo()
            {
                name = "array",
                asmName = "array"
            };
            module.typeInfoByName[arrayType.name] = arrayType;
        }
        private static void AppendResolvedLLVMTypes(ResolvedModule module)
        {
            PrimitiveTypeInfo.BOOL = CreateType(module, "bool", "i1");
            PrimitiveTypeInfo.BYTE = CreateType(module, "byte", "i8");
            PrimitiveTypeInfo.SHORT = CreateType(module, "short", "i16");
            PrimitiveTypeInfo.INT = CreateType(module, "int", "i32");
            PrimitiveTypeInfo.LONG = CreateType(module, "long", "i64");

            PrimitiveTypeInfo ptrType = new PrimitiveTypeInfo()
            {
                name = "ptr",
                asmName = "ptr",
            };
            module.typeInfoByName[ptrType.name] = ptrType;
            PrimitiveTypeInfo.PTR = ptrType;



            PrimitiveTypeInfo arrayType = new PrimitiveTypeInfo()
            {
                name = "array",
                asmName = "array",
            };
            module.typeInfoByName[arrayType.name] = arrayType;
            PrimitiveTypeInfo.ARRAY = arrayType;
        }

        private static PrimitiveTypeInfo CreateType(ResolvedModule module, string astraName, string llvmName)
        {
            PrimitiveTypeInfo type = new PrimitiveTypeInfo()
            {
                name = astraName,
                asmName = llvmName
            };
            module.typeInfoByName[type.name] = type;
            return type;
        }
        private static RawPrimitiveTypeInfo CreateType(RawModule module, string astraName, string llvmName)
        {
            RawPrimitiveTypeInfo type = new RawPrimitiveTypeInfo()
            {
                name = astraName,
                asmName = llvmName
            };
            module.typeInfoByName[type.name] = type;
            return type;
        }
    }
}
