using System.Diagnostics;
using System.Reflection;

namespace AVM;

public partial class VM
{
    public int exitCode;
    
    private byte[] byteCode;
    private int current;
    public Memory memory = new();

    private Action[] methods;

    private List<OpCode> completedOpCodes = new();
    private int completedOpCodesCount = 0;
    private const int COMPLETED_OPCODES_LIMIT = 1000;

    private class Record
    {
        public OpCode code;
        public int count;
        public long totalTicks;

        public float AvgTicksPerCount => totalTicks / (float)count;
        public float Ms => totalTicks / 10_000f;
    }

    public VM()
    {
        methods = new Action[(byte)OpCode.Last];

        HashSet<string> opCodeNames = new(Enum.GetNames<OpCode>());

        foreach (MethodInfo methodInfo in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (opCodeNames.Contains(methodInfo.Name))
            {
                OpCode opCode = Enum.Parse<OpCode>(methodInfo.Name);
                methods[(byte)opCode] = methodInfo.CreateDelegate<Action>(this);
            }
        }
    }
    
    public void Load(byte[] byteCode)
    {
        this.byteCode = byteCode;
        current = 0;
    }

    public void Execute()
    {
        string stackDumpFile = "dumps/stack.raw";
        
        Stopwatch w = Stopwatch.StartNew();

        Dictionary<OpCode, Record> records = new();
        
        while (current < byteCode.Length)
        {
            byte opCodeByte = Next();

            OpCode opCode = (OpCode)opCodeByte;
            
            if (opCodeByte > 0 && opCodeByte < methods.Length && methods[opCodeByte] != null)
            {
                long beginTicks = DateTime.Now.Ticks;
                methods[opCodeByte]();
                long endTicks = DateTime.Now.Ticks;

                if (records.ContainsKey(opCode) == false)
                {
                    records.Add(opCode, new Record()
                    {
                        code = opCode
                    });
                }
                Record record = records[opCode];
                record.totalTicks += endTicks - beginTicks;
                record.count++;

                //completedOpCodes.Add(opCode);
                completedOpCodesCount++;

                //if (completedOpCodesCount >= COMPLETED_OPCODES_LIMIT)
                //{
                //    throw new Exception($"Too many opcodes completed ({COMPLETED_OPCODES_LIMIT}). Seems, there is a infinite loop.");
                //}
            }
            else
            {
                throw new Exception($"Invalid opcode {opCodeByte} at pos {current - 1}");
            }
            
            // memory.Dump(stackDumpFile);
        }

        exitCode = memory.ReadInt(dataSectionSize);
        Console.WriteLine($"Successful executed in {w.ElapsedMilliseconds} ms with exit code {exitCode}");

        Console.WriteLine();
        foreach (Record r in records.OrderByDescending(kv => kv.Value.AvgTicksPerCount).Select(kv => kv.Value))
        {
            Console.WriteLine($"{r.code}: ".PadRight(20) + $"{r.Ms.ToString("0.0")} ms ({r.totalTicks} ticks)".PadRight(26) + $"{r.count} times ({r.AvgTicksPerCount.ToString("0.0")} ticks/invoke)");
        }


        // memory.Dump(stackDumpFile);
    }
    
    private byte Next()
    {
        byte b = byteCode[current];
        current++;
        return b;
    }

    private int NextInt()
    {
        return BitConverter.ToInt32(Next(sizeof(int)));
    }

    private int NextAddress()
    {
        int rbpOffset = NextInt();
        return memory.ToAbs(rbpOffset);
    }

    private byte[] Next(byte bytesCount)
    {
        byte[] bytes = new byte[bytesCount];
        for (int i = 0; i < bytesCount; i++)
        {
            bytes[i] = byteCode[current + i];
        }

        current += bytesCount;
        return bytes;
    }
    private void FunctionPrologue()
    {
        memory.PushInt(memory.basePointer);
        memory.basePointer = memory.stackPointer;
    }
    
    private void FunctionEpilogue()
    {
        memory.stackPointer = memory.basePointer;
        memory.basePointer = memory.PopInt();
    }

    private void Call()
    {
        memory.PushInt(current - 1);

        int opCodePointer = NextInt();
        current = opCodePointer;
    }

    private void Return()
    {
        int callOpCodePointer = memory.PopInt();
        if (byteCode[callOpCodePointer] != (byte)OpCode.Call)
        {
            throw new Exception($"Return opcode at {current - 1} does not return to call opcode, but points to {byteCode[callOpCodePointer]} at {callOpCodePointer}. Seems, you have invalid stack. Make sure, that Return opcode will pop pointer to Call opcode.");
        }

        current = callOpCodePointer + 1 + sizeof(int); // + 1 (OpCode.Call) + int (pointer to label)
    }

    private void Mov()
    {
        byte dstType = Next();

        int dstAddress;
        
        if (dstType == 1)
        {
            // Dst is pointer
            dstAddress = NextAddress();
        }
        else if (dstType == 2)
        {
            // Dst is address behind pointer
            dstAddress = NextAddress();
            dstAddress = memory.ReadInt(dstAddress);
        }
        else
        {
            throw new Exception($"Invalid mov dst type {dstType}");
        }
        

        byte srcType = Next();

        if (srcType == 1)
        {
            // src is rbp offset
            int srcAddress = NextAddress();

            byte sizeInBytes = Next();

            Mov_Stack_to_Stack(dstAddress, srcAddress, sizeInBytes);
        }
        else if (srcType == 2)
        {
            // src is stack address
            
            byte srcSizeInBytes = Next();
            byte[] srcValue = Next(srcSizeInBytes);

            memory.Write(dstAddress, srcValue);
        }
        else if (srcType == 3)
        {
            // src is value behind stack address
            int srcAddress = NextAddress();
            byte srcSizeInBytes = Next();

            byte[] srcValue = memory.Read(srcAddress, srcSizeInBytes);

            memory.Write(dstAddress, srcValue);
        }
        else if (srcType == 4)
        {
            // src is abs address
            int srcAddress = NextInt();
            byte srcSizeInBytes = Next();

            byte[] srcValue = memory.Read(srcAddress, srcSizeInBytes);

            memory.Write(dstAddress, srcValue);
        }
        else
        {
            throw new Exception($"Invalid mov src type {srcType}");
        }
    }

    private void Mov_RBP_to_RBP(int dstRbpOffset, int srcRbpOffset, byte sizeInBytes)
    {
        Mov_Stack_to_Stack(memory.ToAbs(dstRbpOffset), memory.ToAbs(srcRbpOffset), sizeInBytes);
    }
    private void Mov_Stack_to_Stack(int dstAddress, int srcAddress, byte sizeInBytes)
    {
        byte[] srcValue = memory.Read(srcAddress, sizeInBytes);
        
        memory.Write(dstAddress, srcValue);
    }

    private void FieldAccess()
    {
        int baseOffset = NextInt();
        int fieldOffset = NextInt();
        byte fieldValueSize = Next();
        byte isGetter = Next();
        int resultAddress = NextAddress();

        int addressInStack = memory.ToAbs(baseOffset);
        int addressInHeap = memory.ReadInt(addressInStack);

        // fieldPointer is pointing to valid address of ref-type.field
        int fieldPointer = addressInHeap + fieldOffset;
        
        // If we don't need a pointer (like setter), but want to get a value (like getter)
        if (isGetter > 0)
        {
            // Depoint rbx to get actual field value due to getter
            byte[] value = memory.Read(fieldPointer, fieldValueSize);
            
            // Put in result a value (not fixed size) of field (getter)
            memory.Write(resultAddress, value);
        }
        else
        {
            // Put in result a pointer (fixed size) to field (setter)
            memory.WriteInt(resultAddress, fieldPointer);
        }   
    }
    
    
    private void Exit()
    {
        current = byteCode.Length;

        foreach (Form form in forms)
        {
            form.Close();
        }
    }
}