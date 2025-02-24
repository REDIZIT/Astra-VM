using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Astra.Shared;

namespace AVM;

public class DynamicModule
{
    public DynamicMetaTable metaTable;
    public ManagedCode managedCode;

    public DynamicModule(CompiledModule compiled)
    {
        this.managedCode = compiled.managedCode;
        metaTable = new(compiled.metaTable);
    }
}

public class DynamicMetaTable
{
    public List<TypeInfo_Blit> types;
    public List<FunctionInfo_Dynamic> functions;
    
    public DynamicMetaTable(MetaTable compiled)
    {
        types = new(compiled.types);
        
        functions = new(compiled.functions.Length);
        for (int i = 0; i < compiled.functions.Length; i++)
        {
            FunctionInfo_Blit blit = compiled.functions[i];
            FunctionInfo_Dynamic dyn = new(blit);
            functions.Add(dyn);

            if (blit.isAbstract)
            {
                dyn.implementation = new VM_FunctionImplementation()
                {
                    implementation = GetImplementation(compiled, blit)
                };
            }
            else
            {
                dyn.implementation = new InModule_FunctionImplementation()
                {
                    pointedOpCode = blit.pointedOpCode
                };
            }
        }
    }

    private Action<VM> GetImplementation(MetaTable table, FunctionInfo_Blit blit)
    {
        TypeInfo_Blit type = table.types[blit.ownerType];

        if (type.name == "Window")
        {
            if (blit.name == "DrawRect")
            {
                return CreateFunction(Window_DrawRect);
            }
            else if (blit.name == "DrawText")
            {
                return CreateFunction(Window_DrawText);
            }

            if (blit.name == "Clear")
            {
                return (VM vm) =>
                {
                    vm.graphic.Clear(Color.Black);
                };
            }
        }

        throw new Exception($"Failed to find implementation for abstract function '{blit.name}'");
    }

    private Action<VM> CreateFunction(Delegate function)
    {
        ParameterInfo[] infos = function.Method.GetParameters();
        
        return (VM vm) =>
        {
            object[] arguments = new object[infos.Length];
            arguments[0] = vm;

            int offset = sizeof(int);

            for (int i = infos.Length - 1; i >= 1; i--)
            {
                ParameterInfo info = infos[i];

                offset += info.ParameterType == typeof(string) ? sizeof(int) : Marshal.SizeOf(info.ParameterType);
                int address = vm.memory.stackPointer - offset;

                object argument = null;
                if (info.ParameterType == typeof(int))
                {
                    argument = vm.memory.ReadInt(address);
                }
                else if (info.ParameterType == typeof(string))
                {
                    int strAddress = vm.memory.ReadInt(address);
                    
                    int length = vm.memory.ReadInt(strAddress);
                    byte[] bytes = vm.memory.Read(strAddress + sizeof(int), (byte)length);
                    argument = Encoding.ASCII.GetString(bytes);
                }
                else
                {
                    throw new Exception($"Failed to get abstract function implementation's argument for unknown type '{info.ParameterType.Name}'");
                }

                arguments[i] = argument;
            }

            function.DynamicInvoke(arguments);
        };
    }

    private void Window_DrawRect(VM vm, int x, int y)
    {
        vm.graphic.FillRectangle(new SolidBrush(Color.Red), x, y, 15, 15);
    }

    private void Window_DrawText(VM vm, int x, int y, string text)
    {
        vm.graphic.DrawString(text, vm.forms[0].Font, new SolidBrush(Color.White), x, y);
    }
}

public class FunctionInfo_Dynamic
{
    public string name;
    public bool isStatic, isAbstract;
    public InModuleIndex ownerType;
    public FieldInfo_Blit[] arguments;
    public InModuleIndex[] returns;
    public IFunctionImplementation implementation;

    public FunctionInfo_Dynamic(FunctionInfo_Blit blit)
    {
        this.name = blit.name;
        this.isStatic = blit.isStatic;
        this.isAbstract = blit.isAbstract;
        this.ownerType = blit.ownerType;
        this.arguments = blit.arguments;
        this.returns = blit.returns;
    }
}

public interface IFunctionImplementation
{
    void Implement(VM vm);
}

public class InModule_FunctionImplementation : IFunctionImplementation
{
    public int pointedOpCode;

    public void Implement(VM vm)
    {
        vm.current = pointedOpCode - 1;
    }
}

public class VM_FunctionImplementation : IFunctionImplementation
{
    public Action<VM> implementation;

    public void Implement(VM vm)
    {
        implementation(vm);
    }
}