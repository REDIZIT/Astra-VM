namespace AVM;

public partial class VM
{
    private void Compare()
    {
        int aAddress = NextAddress();
        int bAddress = NextAddress();
        byte size = Next();
        int resultAddress = NextAddress();
        byte op = Next();
        
        if (size == 1) CompareMemory_Bytes(aAddress, bAddress, resultAddress, op);
        else if (size == 2) CompareMemory_Shorts(aAddress, bAddress, resultAddress, op);
        else if (size == 4) CompareMemory_Ints(aAddress, bAddress, resultAddress, op);
        else if (size == 8) CompareMemory_Longs(aAddress, bAddress, resultAddress, op);
        else throw new Exception($"Unknown compare variable size {size}");
    }

    private void CompareMemory_Bytes(int aAddress, int bAddress, int resultAddress, byte op)
    {
        byte a = memory.Read(aAddress);
        byte b = memory.Read(bAddress);
        bool result = Compare_Byte(a, b, op);
        memory.Write(resultAddress, result ? (byte)1 : (byte)0);
    }
    private void CompareMemory_Shorts(int aAddress, int bAddress, int resultAddress, byte op)
    {
        short a = memory.ReadShort(aAddress);
        short b = memory.ReadShort(bAddress);
        bool result = Compare_Shorts(a, b, op);
        memory.Write(resultAddress, result ? (byte)1 : (byte)0);
    }
    private void CompareMemory_Ints(int aAddress, int bAddress, int resultAddress, byte op)
    {
        int a = memory.ReadInt(aAddress);
        int b = memory.ReadInt(bAddress);
        bool result = Compare_Ints(a, b, op);
        memory.Write(resultAddress, result ? (byte)1 : (byte)0);
    }
    private void CompareMemory_Longs(int aAddress, int bAddress, int resultAddress, byte op)
    {
        long a = memory.ReadLong(aAddress);
        long b = memory.ReadLong(bAddress);
        bool result = Compare_Longs(a, b, op);
        memory.Write(resultAddress, result ? (byte)1 : (byte)0);
    }

    private bool Compare_Byte(byte a, byte b, byte op)
    {
        switch (op)
        {
            case 0: return a == b;
            case 1: return a != b;
            case 2: return a > b;
            case 3: return a >= b;
            case 4: return a < b;
            case 5: return a <= b;
            default: throw new Exception($"Unknown compare operator {op}");
        }
    }
    private bool Compare_Shorts(short a, short b, byte op)
    {
        switch (op)
        {
            case 0: return a == b;
            case 1: return a != b;
            case 2: return a > b;
            case 3: return a >= b;
            case 4: return a < b;
            case 5: return a <= b;
            default: throw new Exception($"Unknown compare operator {op}");
        }
    }
    private bool Compare_Ints(int a, int b, byte op)
    {
        switch (op)
        {
            case 0: return a == b;
            case 1: return a != b;
            case 2: return a > b;
            case 3: return a >= b;
            case 4: return a < b;
            case 5: return a <= b;
            default: throw new Exception($"Unknown compare operator {op}");
        }
    }
    private bool Compare_Longs(long a, long b, byte op)
    {
        switch (op)
        {
            case 0: return a == b;
            case 1: return a != b;
            case 2: return a > b;
            case 3: return a >= b;
            case 4: return a < b;
            case 5: return a <= b;
            default: throw new Exception($"Unknown compare operator {op}");
        }
    }
}