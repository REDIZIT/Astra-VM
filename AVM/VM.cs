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
    }
    
    public void Load(byte[] byteCode)
    {
        this.byteCode = byteCode;
        current = 0;
    }

    public void Execute()
    {
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
                throw new Exception($"Invalid opcode {opCodeByte} at pos {current}");
            }
        }
        
        Console.WriteLine($"Successful executed in {w.ElapsedMilliseconds} ms with exit code {memory.ReadInt(0)}");
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
        int pointer = memory.Allocate_Stack(bytesToAllocate);
        memory.WriteFrom(pointer, byteCode, current, bytesToAllocate);
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

        int returnValue = NextInt();
        if (returnValue != 0)
        {
            int dstRbp = NextInt();
            int srcRbp = NextInt();

            Mov_RBP_to_RBP(dstRbp, srcRbp, 4);
        }
        
        current += callOpCodePointer + 1 + sizeof(int); // + 1 (OpCode.Call) + int (pointer to label)
    }

    private void Mov()
    {
        byte dstType = Next();

        if (dstType != 1) throw new Exception("Not supported");

        int dstRbpOffset = NextInt();
        int dstAddress = memory.basePointer - dstRbpOffset;

        byte srcType = Next();

        if (srcType == 1)
        {
            int srcRbpOffset = NextInt();
            int srcAddress = memory.basePointer - srcRbpOffset;

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
        Mov_Stack_to_Stack(memory.basePointer - dstRbpOffset, memory.basePointer - srcRbpOffset, sizeInBytes);
    }
    private void Mov_Stack_to_Stack(int dstAddress, int srcAddress, byte sizeInBytes)
    {
        byte[] srcValue = memory.Read(srcAddress, sizeInBytes);
        
        memory.Write(dstAddress, srcValue);
    }

    private void Exit()
    {
        current = byteCode.Length;
    }
}