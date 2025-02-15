using System.Diagnostics;

namespace AVM;

public class VM
{
    private byte[] byteCode;
    private int current;
    private Memory memory = new();

    private Action[] methods;

    public VM()
    {
        methods = new Action[(byte)OpCode.Last];

        methods[(byte)OpCode.Allocate_Stack] = Allocate_Stack;
        methods[(byte)OpCode.FunctionPrologue] = FunctionPrologue;
        methods[(byte)OpCode.FunctionEpilogue] = FunctionEpilogue;
        methods[(byte)OpCode.Call] = Call;
        methods[(byte)OpCode.Return] = Return;
        methods[(byte)OpCode.Exit] = Exit;
        methods[(byte)OpCode.Mov] = Mov;
        
        methods[(byte)OpCode.Add] = Add;
    }
    
    public void Load(byte[] byteCode)
    {
        this.byteCode = byteCode;
        current = 0;
    }

    public void Execute()
    {
        string stackDumpFile = "../../../dumps/stack.raw";
        
        Stopwatch w = Stopwatch.StartNew();
        
        while (current < byteCode.Length)
        {
            byte opCodeByte = Next();

            OpCode opCode = (OpCode)opCodeByte;
            
            if (opCodeByte > 0 && opCodeByte < methods.Length && methods[opCodeByte] != null)
            {
                methods[opCodeByte]();
            }
            else
            {
                throw new Exception($"Invalid opcode {opCodeByte} at pos {current - 1}");
            }
            
            memory.Dump(stackDumpFile);
        }
        
        Console.WriteLine($"Successful executed in {w.ElapsedMilliseconds} ms with exit code {memory.ReadInt(0)}");
        
        
        memory.Dump(stackDumpFile);
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
    
    private void Allocate_Stack()
    {
        byte bytesToAllocate = Next();
        
        int address = memory.Allocate_Stack(bytesToAllocate);
        byte[] defaultValue = byteCode[current..(current + bytesToAllocate)];
        // memory.Write(address, defaultValue);
        
        current += bytesToAllocate;
    }

    private void FunctionPrologue()
    {
        memory.Push(BitConverter.GetBytes(memory.basePointer));
        memory.basePointer = memory.stackPointer;
    }
    
    private void FunctionEpilogue()
    {
        memory.stackPointer = memory.basePointer;
        memory.basePointer = BitConverter.ToInt32(memory.Pop(sizeof(int)));
    }

    private void Call()
    {
        memory.Push(BitConverter.GetBytes(current - 1));
        
        int opCodePointer = BitConverter.ToInt32(Next(sizeof(int)));
        current = opCodePointer;
    }

    private void Return()
    {
        int callOpCodePointer = BitConverter.ToInt32(memory.Pop(sizeof(int)));
        if (byteCode[callOpCodePointer] != (byte)OpCode.Call)
        {
            throw new Exception($"Return opcode at {current - 1} does not return to call opcode, but points to {byteCode[callOpCodePointer]} at {callOpCodePointer}. Seems, you have invalid stack. Make sure, that Return opcode will pop pointer to Call opcode.");
        }

        current += callOpCodePointer + 1 + sizeof(int); // + 1 (OpCode.Call) + int (pointer to label)
    }

    private void Mov()
    {
        byte dstType = Next();

        if (dstType != 1) throw new Exception("Not supported");

        int dstRbpOffset = NextInt();
        int dstAddress = memory.ToAbs(dstRbpOffset);

        byte srcType = Next();

        if (srcType == 1)
        {
            int srcRbpOffset = NextInt();
            int srcAddress = memory.ToAbs(srcRbpOffset);

            byte sizeInBytes = Next();

            Mov_Stack_to_Stack(dstAddress, srcAddress, sizeInBytes);
        }
        else
        {
            if (srcType != 2) throw new Exception("Not supported");

            byte srcSizeInBytes = Next();
            byte[] srcValue = Next(srcSizeInBytes);

            memory.Write(dstAddress, srcValue);
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

    private void Add()
    {
        int aRbpOffset = NextInt();
        int bRbpOffset = NextInt();
        int resultRbpOffset = NextInt();

        byte sizeInBytes = Next();

        int aAddress = memory.ToAbs(aRbpOffset);
        int bAddress = memory.ToAbs(bRbpOffset);
        int resultAddress = memory.ToAbs(resultRbpOffset);
        
        if (sizeInBytes == 1) Add_Bytes(aAddress, bAddress, resultAddress);
        else if (sizeInBytes == 2) Add_Shorts(aAddress, bAddress, resultAddress);
        else if (sizeInBytes == 4) Add_Ints(aAddress, bAddress, resultAddress);
        else if (sizeInBytes == 8) Add_Longs(aAddress, bAddress, resultAddress);
        else throw new Exception($"Bad Add's sizeInBytes = {sizeInBytes}");
    }

    private void Add_Bytes(int aAddress, int bAddress, int resultAddress)
    {
        byte a = memory.Read(aAddress);
        byte b = memory.Read(bAddress);
        byte result = (byte)(a + b);
        memory.Write(resultAddress, result);
    }
    private void Add_Shorts(int aAddress, int bAddress, int resultAddress)
    {
        short a = memory.ReadShort(aAddress);
        short b = memory.ReadShort(bAddress);
        short result = (short)(a + b);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void Add_Ints(int aAddress, int bAddress, int resultAddress)
    {
        int a = memory.ReadInt(aAddress);
        int b = memory.ReadInt(bAddress);
        int result = a + b;
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void Add_Longs(int aAddress, int bAddress, int resultAddress)
    {
        long a = memory.ReadLong(aAddress);
        long b = memory.ReadLong(bAddress);
        long result = a + b;
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }

    
    
    private void Exit()
    {
        current = byteCode.Length;
    }
}