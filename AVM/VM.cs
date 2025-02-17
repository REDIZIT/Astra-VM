using System.Diagnostics;

namespace AVM;

public partial class VM
{
    private byte[] byteCode;
    private int current;
    public Memory memory = new();

    private Action[] methods;

    private List<OpCode> completedOpCodes = new();
    private const int COMPLETED_OPCODES_LIMIT = 1000;

    public VM()
    {
        methods = new Action[(byte)OpCode.Last];

        methods[(byte)OpCode.Allocate_Stack] = Allocate_Stack;
        methods[(byte)OpCode.Allocate_Heap] = Allocate_Heap;
        methods[(byte)OpCode.Deallocate_Stack] = Deallocate_Stack;
        methods[(byte)OpCode.FunctionPrologue] = FunctionPrologue;
        methods[(byte)OpCode.FunctionEpilogue] = FunctionEpilogue;
        methods[(byte)OpCode.Call] = Call;
        methods[(byte)OpCode.Return] = Return;
        methods[(byte)OpCode.Jump] = Jump;
        methods[(byte)OpCode.JumpIfFalse] = JumpIfFalse;
        methods[(byte)OpCode.Exit] = Exit;
        methods[(byte)OpCode.Mov] = Mov;
        
        methods[(byte)OpCode.Add] = Add;
        methods[(byte)OpCode.Sub] = Sub;
        methods[(byte)OpCode.Mul] = Mul;
        methods[(byte)OpCode.Div] = Div;
        methods[(byte)OpCode.LeftBitShift] = LeftBitShift;
        methods[(byte)OpCode.RightBitShift] = RightBitShift;
        methods[(byte)OpCode.BitAnd] = BitAnd;
        methods[(byte)OpCode.BitOr] = BitOr;
        methods[(byte)OpCode.Compare] = Compare;
        
        methods[(byte)OpCode.Negate] = Negate;
        
        methods[(byte)OpCode.ToPtr_ValueType] = ToPtr_ValueType;
        methods[(byte)OpCode.ToPtr_RefType] = ToPtr_RefType;
        methods[(byte)OpCode.PtrGet] = PtrGet;
        methods[(byte)OpCode.PtrSet] = PtrSet;
        methods[(byte)OpCode.PtrShift] = PtrShift;
        
        methods[(byte)OpCode.FieldAccess] = FieldAccess;
        
        methods[(byte)OpCode.AllocateRSPSaver] = AllocateRSPSaver;
        methods[(byte)OpCode.RestoreRSPSaver] = RestoreRSPSaver;
        methods[(byte)OpCode.DeallocateRSPSaver] = DeallocateRSPSaver;
        
        methods[(byte)OpCode.Cast] = Cast;
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
        
        while (current < byteCode.Length)
        {
            byte opCodeByte = Next();

            OpCode opCode = (OpCode)opCodeByte;
            
            if (opCodeByte > 0 && opCodeByte < methods.Length && methods[opCodeByte] != null)
            {
                methods[opCodeByte]();
                completedOpCodes.Add(opCode);

                if (completedOpCodes.Count >= COMPLETED_OPCODES_LIMIT)
                {
                    throw new Exception($"Too many opcodes completed ({COMPLETED_OPCODES_LIMIT}). Seems, there is a infinite loop.");
                }
            }
            else
            {
                throw new Exception($"Invalid opcode {opCodeByte} at pos {current - 1}");
            }
            
            // memory.Dump(stackDumpFile);
        }
        
        Console.WriteLine($"Successful executed in {w.ElapsedMilliseconds} ms with exit code {memory.ReadInt(0)}");
        
        
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
        return memory.ToAbs(NextInt());
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
        memory.Push(BitConverter.GetBytes(memory.basePointer));
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
    }
}