﻿namespace Astra.Compilation;

public class Node_FieldAccess : Node
{
    public Node target;
    public string targetFieldName;

    public TypeInfo targetType;
    public FieldInfo field;
    public FunctionInfo function;

    private bool IsPointingToFunction => function != null;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        // By default, FieldAccess will be generated as getter (primitive value will be allocated)
        // This Generate function will be invoked every time if caller doesn't know (and he doesn't care about that)
        // Only 1 case when you want to use FieldAccess as setter - VariableAssign
        GenerateInternal(ctx, true);
    }

    public void GenerateAsSetter(Generator.Context ctx)
    {
        // If caller want, FieldAccess will be generated as setter (pointer to primitive value will be allocated)
        // This the only case, when you want to use FieldAccess as setter - VariableAssign
        GenerateInternal(ctx, false);
    }

    private void GenerateInternal(Generator.Context ctx, bool isGetter)
    {
        target.Generate(ctx);

        if (field is EmbeddedFieldInfo)
        {
            if (field is PtrAddress_EmbeddedFieldInfo ptrAddress)
            {
                if (isGetter) result = ctx.gen.Allocate(field.type);
                else result = ctx.gen.Allocate(PrimitiveTypes.PTR);
                
                ctx.gen.PtrAddress(target.result, this.result, isGetter);
            }
            else
            {
                throw new Exception($"Failed to generate unknown EmbeddedFieldInfo '{field}'");
            }
        }
        else
        {
            TypeInfo type = targetType;

            int fieldOffsetInBytes = 0;
            foreach (FieldInfo typeField in type.fields)
            {
                if (typeField == field) break;
                fieldOffsetInBytes += 8;
            }

            ctx.gen.Space();
            ctx.gen.Comment($"{target.result.name}.{targetFieldName}");
            

            // (setter) result = pointer to pointer to primivite
            // (getter) result = pointer to primitive
            
            // (setter) result - variable to variable
            // (getter) result - variable

            if (isGetter)
            {
                result = ctx.gen.Allocate(IsPointingToFunction ? PrimitiveTypes.INT : field.type);
            }
            else
            {
                if (IsPointingToFunction) throw new Exception("Can not generate setter for function access");
                result = ctx.gen.Allocate(PrimitiveTypes.PTR);
            }

            if (IsPointingToFunction)
            {
                ctx.gen.FunctionAccess(function, result);
            }
            else
            {
                ctx.gen.FieldAccess(target.result.inscopeRbpOffset, field.type, fieldOffsetInBytes, result, isGetter);
            }
        }
    }
}