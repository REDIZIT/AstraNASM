using System.Collections.Generic;

namespace Astra.Compilation
{
    public class RawModule
    {
        public Dictionary<string, RawTypeInfo> typeInfoByName = new Dictionary<string, RawTypeInfo>();
        public Dictionary<string, RawClassTypeInfo> classInfoByName = new Dictionary<string, RawClassTypeInfo>();
        public Dictionary<string, RawFunctionInfo> functionInfoByName = new Dictionary<string, RawFunctionInfo>();

        public void RegisterClass(RawClassTypeInfo classInfo)
        {
            typeInfoByName.Add(classInfo.name, classInfo);
            classInfoByName.Add(classInfo.name, classInfo);
        }
        public void RegisterFunction(RawFunctionInfo functionInfo)
        {
            functionInfoByName.Add(functionInfo.name, functionInfo);
        }
    }
    public class RawFunctionInfo
    {
        public string name;
        public RawClassTypeInfo owner;

        public List<RawFieldInfo> arguments = new List<RawFieldInfo>();
        public List<RawTypeInfo> returns = new List<RawTypeInfo>();
    }
    public class RawClassTypeInfo : RawTypeInfo
    {
        public List<RawFieldInfo> fields = new List<RawFieldInfo>();
        public List<RawFunctionInfo> functions = new List<RawFunctionInfo>();
    }
    public class RawFieldInfo
    {
        public string name;
        public string typeName;
    }
    public class RawTypeInfo
    {
        public string name;
    }
    public class RawPrimitiveTypeInfo : RawTypeInfo
    {
        public string asmName;
    }
}