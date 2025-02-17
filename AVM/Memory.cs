using System.Text;

namespace AVM;

public class Memory
{
    public int basePointer;
    public int stackPointer;
    public int heapPointer;
    
    public readonly int stackSize = 512;
    
    private byte[] bytes;
    private MemoryLogger logger;

    public string Log => logger.ToString();

    public Memory()
    {
        bytes = new byte[1024];

        basePointer = stackPointer = 0;
        heapPointer = stackSize;

        logger = new(this);
    }

    public int Allocate_Stack(int bytesToAllocate)
    {
        int pointer = stackPointer;

        logger.Log_Allocate(bytesToAllocate);
        
        stackPointer += bytesToAllocate;
        if (stackPointer >= stackSize)
        {
            throw new Exception($"StackOverFlow stack pointer = {stackPointer}, stackSize = {stackSize}");
        }
        
        return pointer;
    }

    public int Allocate_Heap(int bytesToAllocate)
    {
        logger.Log_AllocateHeap(bytesToAllocate);
        
        int pointer = heapPointer;
        heapPointer += bytesToAllocate;
        return pointer;
    }

    public void Deallocate_Stack(int bytesToDeallocate)
    {
        logger.Log_Deallocate(bytesToDeallocate);
        stackPointer -= bytesToDeallocate;
    }

    public void Write(int address, byte value)
    {
        if (address < 0 || address >= bytes.Length)
        {
            throw new Exception($"Write at {address} out of memory bounds ({bytes.Length})");
        }

        logger.Log_Write(address, value);
        
        bytes[address] = value;
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
        if (address < 0 || address + value.Length >= bytes.Length)
        {
            throw new Exception($"Write at {address}..{address + value.Length} out of memory bounds ({bytes.Length})");
        }
        
        if (!noLogs) logger.Log_Write(address, value);
        
        for (int i = 0; i < value.Length; i++)
        {
            bytes[address + i] = value[i];
        }
    }

    public byte Read(int address)
    {
        if (address < 0 || address >= bytes.Length)
        {
            throw new Exception($"Read at {address} out of memory bounds ({bytes.Length})");
        }
        return bytes[address];
    }
    public short ReadShort(int address)
    {
        return BitConverter.ToInt16(bytes, address);
    }
    public int ReadInt(int address)
    {
        return BitConverter.ToInt32(bytes, address);
    }
    public long ReadLong(int address)
    {
        return BitConverter.ToInt64(bytes, address);
    }

    public byte[] Read(int address, byte sizeInBytes)
    {
        if (address < 0 || address + sizeInBytes >= bytes.Length)
        {
            throw new Exception($"Read at {address}..{address + sizeInBytes} out of memory bounds ({bytes.Length})");
        }
        
        byte[] value = new byte[sizeInBytes];
        for (int i = 0; i < sizeInBytes; i++)
        {
            value[i] = bytes[address + i];
        }
        return value;
    }

    public void PushInt(int value)
    {
        Push(BitConverter.GetBytes(value));
    }
    public void Push(byte[] bytes)
    {
        logger.Log_Push(bytes);
        
        Write(stackPointer, bytes, true);
        stackPointer += bytes.Length;
    }

    public int PopInt()
    {
        return BitConverter.ToInt32(Pop(sizeof(int)));
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
        File.WriteAllBytes(filepath, bytes);
    }
}