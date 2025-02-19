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



        Dictionary<Node_Class, TypeInfo> classInfos = new();
        Dictionary<string, TypeInfo> classInfoByName = new();


        //
        // Pass 1: Register virtual members
        //
        RegisterVirtualMembers(classInfoByName);


        //
        // Pass 2: Register types
        //
        foreach (Node node in flatTree)
        {
            if (node is Node_Class cls)
            {
                TypeInfo info = new()
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
        // Pass 3: Register type fields
        //
        foreach (KeyValuePair<Node_Class, TypeInfo> kv in classInfos)
        {
            Node_Class node = kv.Key;
            TypeInfo clsInfo = kv.Value;

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


        foreach (KeyValuePair<string, TypeInfo> kv in classInfoByName)
        {
            kv.Value.CalculateSizeInBytes();
        }

        
        //
        // Pass 4: Register type functions
        //
        foreach (KeyValuePair<Node_Class, TypeInfo> kv in classInfos)
        {
            Node_Class node = kv.Key;
            TypeInfo clsInfo = kv.Value;

            foreach (Node child in node.body.EnumerateChildren())
            {
                if (child is Node_FunctionBody funcDec)
                {
                    FunctionInfo funcInfo = new()
                    {
                        name = funcDec.name,
                        owner = clsInfo,
                        isStatic = funcDec.isStatic,
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
        // Pass 5: Register string constants
        //
        foreach (Node node in EnumerateAllNodes(ast))
        {
            if (node is Node_Literal lit)
            {
                if (lit.constant is Token_String)
                {
                    lit.constant.value = module.RegisterString(lit.constant.value);
                }
            }
        }

        //
        // Pass 6: Generate Scopes
        //
        Scope_StaticAnalysis globalScope = new();
        ParseScope(globalScope, new Node_Block() { children = ast}, module);

        //
        // Pass 7: Resolve Nodes
        //
        foreach (Node node in flatTree)
        {
            if (node is Node_New nodeNew)
            {
                nodeNew.classInfo = (TypeInfo)classInfoByName[nodeNew.className];
            }
            else if (node is Node_FunctionCall call)
            {
                TypeInfo targetType = CalculateType(call.caller, module);

                FunctionInfo targetTypeFunction = TryFindFunction(targetType, call.functionName, module);
                if (targetTypeFunction == null) throw new Exception($"Failed to find function '{call.functionName}' inside type '{targetType}'");

                call.function = targetTypeFunction;
            }
            else if (node is Node_Return ret)
            {
                Scope_StaticAnalysis scope = ret.scope.Find(s => s.functionInfo != null);
                ret.function = scope.functionInfo;
            }
            else if (node is Node_FieldAccess access)
            {
                TypeInfo targetType = CalculateType(access.target, module);
                access.targetType = targetType;

                if (TryFindFunction(targetType, access.targetFieldName, module) == null && targetType is TypeInfo classInfo)
                {
                    FieldInfo targetTypeField = TryFindField(classInfo, access.targetFieldName, module);
                    if (targetTypeField == null) throw new Exception($"Failed to find field '{access.targetFieldName}' inside type '{targetType}'");
                    
                    access.field = targetTypeField;
                }                
            }
            else if (node is Node_VariableUse varUse)
            {
                TypeInfo targetType = CalculateType(varUse, module);

                varUse.type = targetType;
            }
            else if (node is Node_TryCatch tryCatch)
            {
                if (tryCatch.exceptionRawVariable != null)
                {
                    tryCatch.exceptionVariableType = module.classInfoByName[tryCatch.exceptionRawVariable.rawType];
                }
            }
            else if (node is Node_As nodeAs)
            {
                nodeAs.typeInfo = module.GetType(nodeAs.typeToken.name);
            }
            else if (node is Node_Binary bin)
            {
                bin.resultType = module.GetType(bin.@operator.ResultType);
            }
            else if (node is Node_VariableDeclaration varDec)
            {
                varDec.variableType = module.GetType(varDec.variable.rawType);
            }
        }

        return module;
    }

    public static IEnumerable<Node> EnumerateAllNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            foreach (Node child in EnumerateAllNodes(node))
            {
                yield return child;
            }
        }
    }
    public static IEnumerable<Node> EnumerateAllNodes(Node node)
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

    private static void ParseScope(Scope_StaticAnalysis parentScope, Node node, ResolvedModule module)
    {
        Scope_StaticAnalysis scope = parentScope.CreateSubScope();
        node.scope = scope;

        if (node is Node_Class cls)
        {
            scope.typeInfo = cls.classInfo;
        }
        else if (node is Node_FunctionBody func)
        {
            scope.functionInfo = func.functionInfo;
            
            if (func.functionInfo.isStatic == false)
            {
                scope.namedVariables.Add(new FieldInfo()
                {
                    name = "self",
                    type = func.functionInfo.owner
                });
            }

            foreach (FieldInfo argument in func.functionInfo.arguments)
            {
                scope.namedVariables.Add(argument);
            }
        }
        else if (node is Node_TryCatch tryCatch)
        {
            if (tryCatch.exceptionRawVariable != null)
            {
                scope.namedVariables.Add(new FieldInfo(tryCatch.exceptionVariableType, tryCatch.exceptionRawVariable.name));
                Console.WriteLine("Register scope for " + tryCatch.exceptionRawVariable.name);
            }
        }

        foreach (Node childNode in node.EnumerateChildren())
        {
            if (childNode is Node_VariableDeclaration varDec)
            {
                TypeInfo clsInfo = scope.Find(s => s.typeInfo != null).typeInfo;

                FieldInfo fieldInfo = new()
                {
                    name = varDec.variable.name,
                    type = module.classInfoByName[varDec.variable.rawType],
                };
                clsInfo.fields.Add(fieldInfo);

                varDec.ownerInfo = clsInfo;
                varDec.fieldInfo = fieldInfo;

                scope.namedVariables.Add(varDec.fieldInfo);
            }

            ParseScope(scope, childNode, module);
        }
    }

    private static TypeInfo CalculateType(Node targetNode, ResolvedModule module)
    {
        if (targetNode is Node_FieldAccess acces)
        {
            return CalculateType(acces.target, module);
        }
        if (targetNode is Node_VariableUse use)
        {
            string name = use.variableName;

            if (module.classInfoByName.TryGetValue(name, out TypeInfo type))
            {
                // Access to static type name
                // Example: Console.
                return type;
            }
            else if (name == "alloc")
            {
                return PrimitiveTypes.PTR;
            }
            else
            {
                // Access to variable
                // Example: listOfItems.
                FieldInfo variable = targetNode.scope.FindVariable(name);
                return variable.type;   
            }
        }

        return null;
    }

    private static FunctionInfo TryFindFunction(TypeInfo targetType, string functionName, ResolvedModule module)
    {
        FunctionInfo targetTypeFunction = null;

        if (targetType is TypeInfo classType)
        {
            targetTypeFunction = classType.functions.FirstOrDefault(i => i.name == functionName);
        }

        if (targetTypeFunction == null)
        {
            targetTypeFunction = GetExtensionFunction(targetType, functionName, module);
        }

        return targetTypeFunction;
    }

    private static FieldInfo TryFindField(TypeInfo targetType, string fieldName, ResolvedModule module)
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
        RegisterString(classInfoByName);
    }
    private static void RegisterPtr(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo ptrInfo = new TypeInfo()
        {
            name = "ptr",
            isStruct = true
        };



        FieldInfo address = new PtrAddress_EmbeddedFieldInfo()
        {
            name = "address",
            type = PrimitiveTypes.INT
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
        FunctionInfo get_byte = new PtrGet_Byte()
        {
            name = "get_byte",
            returns = new() { PrimitiveTypes.BYTE },
            owner = ptrInfo
        };
        FunctionInfo get_short = new PtrGet_Short()
        {
            name = "get_short",
            returns = new() { PrimitiveTypes.SHORT },
            owner = ptrInfo
        };
        FunctionInfo get_int = new PtrGet_Int()
        {
            name = "get_int",
            returns = new() { PrimitiveTypes.INT },
            owner = ptrInfo
        };
        FunctionInfo get_long = new PtrGet_Long()
        {
            name = "get_long",
            returns = new() { PrimitiveTypes.LONG },
            owner = ptrInfo
        };
        

        ptrInfo.functions = new() 
        {
            toPtr,
            shift,
            set,
            get_byte,
            get_short,
            get_int,
            get_long,
        };

        
        classInfoByName.Add(ptrInfo.name, ptrInfo);
        PrimitiveTypes.PTR = ptrInfo;
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
    private static void RegisterString(Dictionary<string, TypeInfo> classInfoByName)
    {
        TypeInfo info = new()
        {
            name = "string"
        };
        classInfoByName.Add(info.name, info);

        FunctionInfo get = new StringGet_EmbeddedFunctionInfo()
        {
            arguments = new()
            {
                new FieldInfo(PrimitiveTypes.INT, "index"),
            },
            returns = new()
            {
                PrimitiveTypes.BYTE
            },
            name = "get",
            owner = info
        };
        info.functions.Add(get);
        
        FunctionInfo length = new StringLength_EmbeddedFunctionInfo()
        {
            returns = new()
            {
                PrimitiveTypes.BYTE
            },
            name = "length",
            owner = info
        };
        info.functions.Add(length);

        PrimitiveTypes.STRING = info;
    }

    private static FunctionInfo GetExtensionFunction(TypeInfo type, string functionName, ResolvedModule module)
    {
        if (functionName == "to_ptr")
        {
            return (module.GetType("ptr")).functions.First(f => f.name == "to_ptr");
        }

        if (functionName == "alloc")
        {
            return new Alloc_EmbeddedFunctionInfo()
            {
                name = "alloc",
                owner = null,
                isStatic = true,
                arguments = new List<FieldInfo>() { new FieldInfo(PrimitiveTypes.INT, "bytesCount" )},
                returns = new List<TypeInfo>() { PrimitiveTypes.PTR }
            };
        }

        return null;
    }
}