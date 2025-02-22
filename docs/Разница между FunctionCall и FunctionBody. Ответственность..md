Node_FunctionCall.cs
```c#
int bytesAllocated = 0;

ctx.gen.PushToStack(argument.result)
bytesAllocated += argument.result.type.refSizeInBytes;

ctx.gen.Call(function.GetCombinedName())
ctx.gen.Deallocate(bytesAllocated)
```


Node_FunctionBody.cs
```c#
ctx.gen.currentScope.RegisterLocalVariable(arg.type, arg.name)

Variable callPushed = ctx.gen.currentScope.RegisterLocalVariable(PrimitiveTypes.PTR "call_pushed_instruction");



ctx.gen.BeginSubScope();
body.Generate(ctx);
ctx.gen.DropSubScope();


ctx.gen.currentScope.UnregisterLocalVariable(callPushed)

ctx.gen.currentScope.UnregisterLocalVariable(arg)
```

При генерации тела функции мы "даём обещание", что в момент выполнения кода внутри обрамления body.Generate нам будут доступны аргументы. Так как аргументы находятся не в текущем scope, а в предыдущем, то rbp offset будет отрицательным (согласно управлению памятью)