namespace Astra.Compilation;

public static class Resolver
{
    public static ResolvedModule DiscoverModule(List<Node> ast)
    {
        ResolvedModule module = new ResolvedModule();


        List<Node> flatTree = new();
        foreach (Node node in ast)
        {
            foreach (Node child in EnumerateAllNodes(node))
            {
                flatTree.Add(child);
            }
        }



        Dictionary<Node_Class, ClassTypeInfo> classInfos = new();
        Dictionary<string, TypeInfo> classInfoByName = new();


        //
        // Pass 4: Register virtual members
        //
        RegisterVirtualMembers(classInfoByName);


        //
        // Pass 1: Register types
        //
        foreach (Node node in flatTree)
        {
            if (node is Node_Class cls)
            {
                ClassTypeInfo info = new()
                {
                    name = cls.name,
                    fields = new(),
                    functions = new(),
                };

                classInfos.Add(cls, info);
                classInfoByName.Add(info.name, info);

                cls.classInfo = info;
            }
        }


        //
        // Pass 2: Register type fields
        //
        foreach (KeyValuePair<Node_Class, ClassTypeInfo> kv in classInfos)
        {
            Node_Class node = kv.Key;
            ClassTypeInfo clsInfo = kv.Value;

            foreach (Node child in node.body.EnumerateChildren())
            {
                if (child is Node_VariableDeclaration varDec)
                {
                    FieldInfo fieldInfo = new()
                    {
                        name = varDec.variable.name,
                        type = classInfoByName[varDec.variable.rawType],
                    };
                    clsInfo.fields.Add(fieldInfo);

                    varDec.ownerInfo = clsInfo;
                    varDec.fieldInfo = fieldInfo;
                }
            }
        }

        //
        // Pass 3: Register type functions
        //
        foreach (KeyValuePair<Node_Class, ClassTypeInfo> kv in classInfos)
        {
            Node_Class node = kv.Key;
            ClassTypeInfo clsInfo = kv.Value;

            foreach (Node child in node.body.EnumerateChildren())
            {
                if (child is Node_Function funcDec)
                {
                    FunctionInfo funcInfo = new()
                    {
                        name = funcDec.name,
                        owner = clsInfo,
                        arguments = new(),
                        returns = new()
                    };

                    foreach (VariableRawData arg in funcDec.parameters)
                    {
                        TypeInfo type = classInfoByName[arg.rawType];
                        funcInfo.arguments.Add(new FieldInfo()
                        {
                            name = arg.name,
                            type = type
                        });
                    }

                    foreach (VariableRawData ret in funcDec.returnValues)
                    {
                        TypeInfo type = classInfoByName[ret.rawType];
                        funcInfo.returns.Add(type);
                    }

                    clsInfo.functions.Add(funcInfo);
                    funcDec.functionInfo = funcInfo;
                }
            }
        }

        module.classInfoByName = classInfoByName;

        //
        // Pass 5: Generate Scopes
        //
        Scope globalScope = new();
        GenerateScope(globalScope, new Node_Block() { children = ast}, module);

        //
        // Pass 6: Resolve Nodes
        //
        foreach (Node node in flatTree)
        {
            if (node is Node_New nodeNew)
            {
                nodeNew.classInfo = (ClassTypeInfo)classInfoByName[nodeNew.className];
            }
            else if (node is Node_FunctionCall call)
            {
                TypeInfo targetType = CalculateType(call.caller);

                FunctionInfo targetTypeFunction = TryFindFunction(targetType, call.functionName, module);
                if (targetTypeFunction == null) throw new Exception($"Failed to find function '{call.functionName}' inside type '{targetType}'");

                call.function = targetTypeFunction;
            }
            else if (node is Node_Return ret)
            {
                Scope scope = ret.scope.Find(s => s.functionInfo != null);
                ret.function = scope.functionInfo;
            }
            else if (node is Node_FieldAccess access)
            {
                TypeInfo targetType = CalculateType(access.target);

                if (TryFindFunction(targetType, access.targetFieldName, module) == null && targetType is ClassTypeInfo classInfo)
                {
                    FieldInfo targetTypeField = TryFindField(classInfo, access.targetFieldName, module);
                    if (targetTypeField == null) throw new Exception($"Failed to find field '{access.targetFieldName}' inside type '{targetType}'");

                    access.field = targetTypeField;
                }                
            }
        }

        return module;
    }

    private static IEnumerable<Node> EnumerateAllNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            foreach (Node child in EnumerateAllNodes(node))
            {
                yield return child;
            }
        }
    }
    private static IEnumerable<Node> EnumerateAllNodes(Node node)
    {
        yield return node;

        foreach (Node child in node.EnumerateChildren())
        {
            foreach (Node item in EnumerateAllNodes(child))
            {
                yield return item;
            }
        }
    }

    private static void GenerateScope(Scope parentScope, Node node, ResolvedModule module)
    {
        Scope scope = parentScope.CreateSubScope();
        node.scope = scope;

        if (node is Node_Class cls)
        {
            scope.typeInfo = cls.classInfo;
        }
        else if (node is Node_Function func)
        {
            scope.functionInfo = func.functionInfo;
            
            if (func.functionInfo.owner != null)
            {
                scope.variables.Add(new FieldInfo()
                {
                    name = "self",
                    type = func.functionInfo.owner
                });
            }
            else
            {
                throw new Exception("Static functions are not supported");
            }
        }

        foreach (Node childNode in node.EnumerateChildren())
        {
            if (childNode is Node_VariableDeclaration varDec)
            {
                ClassTypeInfo clsInfo = scope.Find(s => s.typeInfo != null).typeInfo;

                FieldInfo fieldInfo = new()
                {
                    name = varDec.variable.name,
                    type = module.classInfoByName[varDec.variable.rawType],
                };
                clsInfo.fields.Add(fieldInfo);

                varDec.ownerInfo = clsInfo;
                varDec.fieldInfo = fieldInfo;

                scope.variables.Add(varDec.fieldInfo);
            }

            GenerateScope(scope, childNode, module);
        }
    }

    private static TypeInfo CalculateType(Node targetNode)
    {
        if (targetNode is Node_FieldAccess acces)
        {
            return CalculateType(acces.target);
        }
        if (targetNode is Node_VariableUse use)
        {
            string variableName = use.variableName;
            Scope scope = targetNode.scope.Find(s => s.variables.Any(f => f.name == variableName));

            FieldInfo variable = scope.variables.First(f => f.name == variableName);
            return variable.type;
        }

        return null;
    }

    private static FunctionInfo TryFindFunction(TypeInfo targetType, string functionName, ResolvedModule module)
    {
        FunctionInfo targetTypeFunction = null;

        if (targetType is ClassTypeInfo classType)
        {
            targetTypeFunction = classType.functions.FirstOrDefault(i => i.name == functionName);
        }

        if (targetTypeFunction == null)
        {
            targetTypeFunction = GetExtensionFunction(targetType, functionName, module);
        }

        return targetTypeFunction;
    }

    private static FieldInfo TryFindField(ClassTypeInfo targetType, string fieldName, ResolvedModule module)
    {
        FieldInfo targetTypeField = targetType.fields.FirstOrDefault(i => i.name == fieldName);

        return targetTypeField;
    }


    private static void RegisterVirtualMembers(Dictionary<string, TypeInfo> classInfoByName)
    {
        RegisterLong(classInfoByName);
        RegisterInt(classInfoByName);
        RegisterShort(classInfoByName);
        RegisterByte(classInfoByName);

        RegisterBool(classInfoByName);

        RegisterPtr(classInfoByName);
    }
    private static void RegisterPtr(Dictionary<string, TypeInfo> classInfoByName)
    {
        ClassTypeInfo ptrInfo = new ClassTypeInfo()
        {
            name = "ptr",
            isStruct = true
        };



        FieldInfo address = new PtrAddress_EmbeddedFieldInfo()
        {
            name = "address",
            type = classInfoByName["int"]
        };

        ptrInfo.fields = new List<FieldInfo>()
        {
            address
        };



        FunctionInfo toPtr = new ToPtr_EmbeddedFunctionInfo()
        {
            name = "to_ptr",
            arguments = new(),
            returns = new() { ptrInfo },
            owner = ptrInfo
        };
        FunctionInfo shift = new PtrShift_EmbeddedFunctionInfo()
        {
            name = "shift",
            arguments = new() { new FieldInfo(classInfoByName["int"], "shiftInBytes") },
            returns = new(),
            owner = ptrInfo
        };
        FunctionInfo set = new PtrSet_EmbeddedFunctionInfo()
        {
            name = "set",
            arguments = new() { new FieldInfo(classInfoByName["int"], "value") },
            returns = new(),
            owner = ptrInfo
        };
        FunctionInfo get = new PtrGet_EmbeddedFunctionInfo()
        {
            name = "get",
            arguments = new(),
            returns = new() { classInfoByName["int"] },
            owner = ptrInfo
        };

        ptrInfo.functions = new() 
        {
            toPtr,
            shift,
            set,
            get
        };



        classInfoByName.Add(ptrInfo.name, ptrInfo);
    }

    private static void RegisterLong(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "long"
        };
        classInfoByName.Add(info.name, info);

        PrimitiveTypes.LONG = info;
    }
    private static void RegisterInt(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "int"
        };
        classInfoByName.Add(info.name, info);

        PrimitiveTypes.INT = info;
    }
    private static void RegisterShort(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "short"
        };
        classInfoByName.Add(info.name, info);

        PrimitiveTypes.SHORT = info;
    }
    private static void RegisterByte(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "byte"
        };
        classInfoByName.Add(info.name, info);

        PrimitiveTypes.BYTE = info;
    }
    private static void RegisterBool(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "bool"
        };
        classInfoByName.Add(info.name, info);

        PrimitiveTypes.BOOL = info;
    }

    private static FunctionInfo GetExtensionFunction(TypeInfo type, string functionName, ResolvedModule module)
    {
        if (functionName == "to_ptr")
        {
            return ((ClassTypeInfo)module.GetType("ptr")).functions.First(f => f.name == "to_ptr");
        }

        return null;
    }
}
