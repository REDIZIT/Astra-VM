using System.Text;

namespace AVM;

public class MemoryLogger
{
    private Memory memory;
    private StringBuilder b = new();

    private const int PAD = 30;

    public MemoryLogger(Memory memory)
    {
        this.memory = memory;
    }
    
    public void Log_Allocate(int bytesToAllocate)
    {
        b.AppendLine($"Allocate stack {memory.stackPointer}..{memory.stackPointer + bytesToAllocate}".PadRight(PAD) + ToString(memory.Read(memory.stackPointer, (byte)bytesToAllocate)));
    }

    public void Log_Write(int address, byte value)
    {
        b.AppendLine($"Write at {address}:".PadRight(PAD) + ToString(memory.Read(address)) + " => " + ToString(value));
    }
    public void Log_Write(int address, byte[] value)
    {
        b.AppendLine($"Write at {address}..{address + value.Length}:".PadRight(PAD) + ToString(memory.Read(address, (byte)value.Length)) + " => " + ToString(value));
    }

    public void Log_Push(byte[] value)
    {
        b.AppendLine($"Push stack {memory.stackPointer}..{memory.stackPointer + value.Length}".PadRight(PAD) + ToString(memory.Read(memory.stackPointer, (byte)value.Length)) + " => " + ToString(value));
    }

    public void Log_Pop(byte bytesToPop)
    {
        b.AppendLine($"Pop stack {memory.stackPointer - bytesToPop}..{memory.stackPointer}".PadRight(PAD) + ToString(memory.Read(memory.stackPointer - bytesToPop, bytesToPop)));
    }

    private string ToString(byte value, bool hideZeros = true)
    {
        if (hideZeros && value == 0) return "--";
        return value.ToString("x2");
    }

    private string ToString(byte[] value)
    {
        string[] strings = new string[value.Length];
        bool hadAnyValue = false;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            byte v = value[i];

            if (v != 0) hadAnyValue = true;
            
            strings[i] = ToString(v, hadAnyValue == false);
        }

        return string.Join(" ", strings);
    }

    public override string ToString()
    {
        return b.ToString();
    }
}