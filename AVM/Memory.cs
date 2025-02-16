using System.Text;

namespace AVM;

public class Memory
{
    public int basePointer;
    public int stackPointer;

    private int heapPointer;
    
    private byte[] stack, heap;

    private MemoryLogger logger;

    public string Log => logger.ToString();

    public Memory()
    {
        stack = new byte[1024];
        heap = new byte[1024];

        logger = new(this);
    }

    public int Allocate_Stack(int bytesToAllocate)
    {
        int pointer = stackPointer;

        logger.Log_Allocate(bytesToAllocate);
        
        stackPointer += bytesToAllocate;
        return pointer;
    }

    public int Allocate_Heap(int bytesToAllocate)
    {
        int pointer = heapPointer;
        heapPointer += bytesToAllocate;
        return pointer;
    }

    public void Deallocate_Stack(int bytesToDeallocate)
    {
        stackPointer -= bytesToDeallocate;
    }

    public void Write(int address, byte value)
    {
        if (address < 0 || address >= stack.Length)
        {
            throw new Exception($"Out of stack bounds {address}");
        }

        logger.Log_Write(address, value);
        
        stack[address] = value;
    }

    public void WriteShort(int address, short value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void WriteInt(int address, int value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void WriteLong(int address, long value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void Write(int address, byte[] value, bool noLogs = false)
    {
        if (address < 0 || address + value.Length >= stack.Length)
        {
            throw new Exception($"Out of stack bounds {address}");
        }
        
        if (!noLogs) logger.Log_Write(address, value);
        
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
        logger.Log_Push(bytes);
        
        Write(stackPointer, bytes, true);
        stackPointer += bytes.Length;
    }

    public byte[] Pop(byte bytesToPop)
    {
        logger.Log_Pop(bytesToPop);
        
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