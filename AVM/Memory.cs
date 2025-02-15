using System.Text;

namespace AVM;

public class Memory
{
    public int basePointer;
    public int stackPointer;
    
    private byte[] stack;

    private StringBuilder logger = new();

    public string Log => logger.ToString();

    public Memory()
    {
        stack = new byte[1024];
        // stackPointer = stack.Length;
        // basePointer = stackPointer;
    }

    public int Allocate_Stack(int bytesToAllocate)
    {
        int pointer = stackPointer;

        logger.AppendLine($"Allocate stack {stackPointer}..{stackPointer + bytesToAllocate}");
            
        stackPointer += bytesToAllocate;
        return pointer;
    }

    public void Write(int address, byte value)
    {
        logger.AppendLine($"Write at {address}");
        
        stack[address] = value;
    }
    public void Write(int address, byte[] value)
    {
        logger.AppendLine($"Write {address}..{address + value.Length} ");
        
        for (int i = 0; i < value.Length; i++)
        {
            stack[address + i] = value[i];
        }
    }

    public byte Read(int address)
    {
        return stack[address];
    }
    public short ReadShort(int address)
    {
        return BitConverter.ToInt16(stack, address);
    }
    public int ReadInt(int address)
    {
        return BitConverter.ToInt32(stack, address);
    }
    public long ReadLong(int address)
    {
        return BitConverter.ToInt64(stack, address);
    }

    public byte[] Read(int address, byte sizeInBytes)
    {
        byte[] bytes = new byte[sizeInBytes];
        for (int i = 0; i < sizeInBytes; i++)
        {
            bytes[i] = stack[address + i];
        }
        return bytes;
    }

    public void Push(byte[] bytes)
    {
        logger.AppendLine($"Push stack {stackPointer}..{stackPointer + bytes.Length}");
        
        Write(stackPointer, bytes);
        stackPointer += bytes.Length;
    }

    public byte[] Pop(byte bytesToPop)
    {
        logger.AppendLine($"Pop stack {stackPointer - bytesToPop}..{stackPointer}");
        
        stackPointer -= bytesToPop;
        return Read(stackPointer, bytesToPop);
    }


    public int ToAbs(int rbpOffset)
    {
        return basePointer - rbpOffset;
    }

    public void Dump(string filepath)
    {
        File.WriteAllBytes(filepath, stack);
    }
}