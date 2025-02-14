namespace AVM;

public class Memory
{
    public int basePointer;
    public int stackPointer;
    
    private byte[] stack;

    public Memory()
    {
        stack = new byte[1024];
    }

    public int Allocate_Stack(int bytesToAllocate)
    {
        int pointer = stackPointer;
        stackPointer += bytesToAllocate;
        return pointer;
    }

    public void Write(int address, byte[] value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            stack[address + i] = value[i];
        }
    }

    public void WriteFrom(int address, byte[] sourceBytes, int startIndex, int length)
    {
        for (int i = 0; i < length; i++)
        {
            stack[address] = sourceBytes[startIndex + i];
        }
    }

    public int ReadInt(int address)
    {
        return BitConverter.ToInt32(stack, address);
    }

    public byte[] Read(int address, byte sizeInBytes)
    {
        byte[] bytes = new byte[sizeInBytes];
        for (int i = 0; i < sizeInBytes; i++)
        {
            bytes[i] = stack[stackPointer + i];
        }
        return bytes;
    }

    public void Push(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            stack[stackPointer + i] = bytes[i];
        }
        
        stackPointer += bytes.Length;
    }

    public byte[] Pop(byte bytesToPop)
    {
        stackPointer -= bytesToPop;
        
        return Read(stackPointer, bytesToPop);
    }
}