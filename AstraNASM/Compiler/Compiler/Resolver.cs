namespace Astra.Compilation;

public class Scope
{
    public List<FieldInfo> variables = new();
    public ClassTypeInfo typeInfo;
    public FunctionInfo functionInfo;

    public Scope parent;
    public List<Scope> children = new();

    public Scope CreateSubScope()
    {
        Scope child = new();
        child.parent = this;
        children.Add(child);

        return child;
    }

    public Scope Find(Func<Scope, bool> predicate)
    {
        if (predicate(this)) return this;
        if (parent == null) throw new Exception("Failed to find scope");
        return parent.Find(predicate);
    }
}

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
        Dictionary<string, ClassTypeInfo> classInfoByName = new();


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
                        ClassTypeInfo type = classInfoByName[arg.rawType];
                        funcInfo.arguments.Add(new FieldInfo()
                        {
                            name = arg.name,
                            type = type
                        });
                    }

                    foreach (VariableRawData ret in funcDec.returnValues)
                    {
                        ClassTypeInfo type = classInfoByName[ret.rawType];
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
                nodeNew.classInfo = classInfoByName[nodeNew.className];
            }
            else if (node is Node_FunctionCall call)
            {
                ClassTypeInfo targetType = (ClassTypeInfo)CalculateType(call.caller);

                FunctionInfo targetTypeFunction = targetType.functions.FirstOrDefault(i => i.name == call.functionName);

                if (targetTypeFunction == null)
                {
                    targetTypeFunction = GetExtensionFunction(targetType, call.functionName, module);
                }

                if (targetTypeFunction == null) throw new Exception($"Failed to find function '{call.functionName}' inside type '{targetType}'");

                call.function = targetTypeFunction;
            }
            else if (node is Node_Return ret)
            {
                Scope scope = ret.scope.Find(s => s.functionInfo != null);
                ret.function = scope.functionInfo;
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

    private static void RegisterVirtualMembers(Dictionary<string, ClassTypeInfo> classInfoByName)
    {
        RegisterInt(classInfoByName);
        RegisterBool(classInfoByName);
        RegisterPtr(classInfoByName);
    }
    private static void RegisterPtr(Dictionary<string, ClassTypeInfo> classInfoByName)
    {
        ClassTypeInfo ptrInfo = new ClassTypeInfo()
        {
            name = "ptr",
            isStruct = true
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
    private static void RegisterInt(Dictionary<string, ClassTypeInfo> classInfoByName)
    {
        ClassTypeInfo info = new ClassTypeInfo()
        {
            name = "int",
            isStruct = true
        };
        classInfoByName.Add(info.name, info);
    }
    private static void RegisterBool(Dictionary<string, ClassTypeInfo> classInfoByName)
    {
        ClassTypeInfo info = new ClassTypeInfo()
        {
            name = "bool",
            isStruct = true
        };
        classInfoByName.Add(info.name, info);
    }

    private static FunctionInfo GetExtensionFunction(ClassTypeInfo type, string functionName, ResolvedModule module)
    {
        if (functionName == "to_ptr")
        {
            return module.GetType("ptr").functions.First(f => f.name == "to_ptr");
        }

        return null;
    }
}
