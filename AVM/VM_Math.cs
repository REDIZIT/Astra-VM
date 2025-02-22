namespace AVM;

public partial class VM
{
    private void Add()
    {
        Math_Binary(OpCode.Add);
    }
    private void Sub()
    {
        Math_Binary(OpCode.Sub);
    }
    private void Mul()
    {
        Math_Binary(OpCode.Mul);
    }
    private void Div()
    {
        Math_Binary(OpCode.Div);
    }
    private void DivRemainder()
    {
        Math_Binary(OpCode.DivRemainder);
    }
    private void LeftBitShift()
    {
        Math_Binary(OpCode.LeftBitShift);
    }
    private void RightBitShift()
    {
        Math_Binary(OpCode.RightBitShift);
    }
    private void BitAnd()
    {
        Math_Binary(OpCode.BitAnd);
    }
    private void BitOr()
    {
        Math_Binary(OpCode.BitOr);
    }
    
    private void Negate()
    {
        Math_Unary(OpCode.Negate);
    }
    private void Increment()
    {
        Math_IncDec(OpCode.Increment);
    }
    private void Decrement()
    {
        Math_IncDec(OpCode.Decrement);
    }


    private void Math_Binary(OpCode op)
    {
        int aRbpOffset = NextInt();
        int bRbpOffset = NextInt();
        int resultRbpOffset = NextInt();

        byte sizeInBytes = Next();

        int aAddress = memory.ToAbs(aRbpOffset);
        int bAddress = memory.ToAbs(bRbpOffset);
        int resultAddress = memory.ToAbs(resultRbpOffset);

        if (sizeInBytes == 1) MathMemory_Bytes(aAddress, bAddress, resultAddress, op);
        else if (sizeInBytes == 2) MathMemory_Shorts(aAddress, bAddress, resultAddress, op);
        else if (sizeInBytes == 4) MathMemory_Ints(aAddress, bAddress, resultAddress, op);
        else if (sizeInBytes == 8) MathMemory_Longs(aAddress, bAddress, resultAddress, op);
        else throw new Exception($"Bad Math's sizeInBytes = {sizeInBytes}");
    }

    private void Math_Unary(OpCode op)
    {
        int aRbpOffset = NextInt();
        int resultRbpOffset = NextInt();
        byte sizeInBytes = Next();
        
        int aAddress = memory.ToAbs(aRbpOffset);
        int resultAddress = memory.ToAbs(resultRbpOffset);
        
        if (sizeInBytes == 1) MathMemory_Unary_Byte(aAddress, resultAddress, op);
        else if (sizeInBytes == 2) MathMemory_Unary_Short(aAddress, resultAddress, op);
        else if (sizeInBytes == 4) MathMemory_Unary_Int(aAddress, resultAddress, op);
        else if (sizeInBytes == 8) MathMemory_Unary_Long(aAddress, resultAddress, op);
        else throw new Exception($"Bad Math's sizeInBytes = {sizeInBytes}");
    }
    private void Math_IncDec(OpCode op)
    {
        int targetAddress = NextAddress();
        byte sizeInBytes = Next();
        
        if (sizeInBytes == 1) MathMemory_IncDec_Byte(targetAddress, op);
        else if (sizeInBytes == 2) MathMemory_IncDec_Short(targetAddress, op);
        else if (sizeInBytes == 4) MathMemory_IncDec_Int(targetAddress, op);
        else if (sizeInBytes == 8) MathMemory_IncDec_Long(targetAddress, op);
        else throw new Exception($"Bad Math's sizeInBytes = {sizeInBytes}");
    }

    private void MathMemory_Bytes(int aAddress, int bAddress, int resultAddress, OpCode op)
    {
        byte a = memory.Read(aAddress);
        byte b = memory.Read(bAddress);
        byte result = Math_Bytes(a, b, op);
        memory.Write(resultAddress, result);
    }
    private void MathMemory_Shorts(int aAddress, int bAddress, int resultAddress, OpCode op)
    {
        short a = memory.ReadShort(aAddress);
        short b = memory.ReadShort(bAddress);
        short result = Math_Shorts(a, b, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_Ints(int aAddress, int bAddress, int resultAddress, OpCode op)
    {
        int a = memory.ReadInt(aAddress);
        int b = memory.ReadInt(bAddress);
        int result = Math_Ints(a, b, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_Longs(int aAddress, int bAddress, int resultAddress, OpCode op)
    {
        long a = memory.ReadLong(aAddress);
        long b = memory.ReadLong(bAddress);
        long result = Math_Longs(a, b, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }

    
    
    private void MathMemory_Unary_Byte(int aAddress, int resultAddress, OpCode op)
    {
        byte a = memory.Read(aAddress);
        byte result = Math_Unary_Byte(a, op);
        memory.Write(resultAddress, result);
    }
    private void MathMemory_Unary_Short(int aAddress, int resultAddress, OpCode op)
    {
        short a = memory.ReadShort(aAddress);
        short result = Math_Unary_Short(a, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_Unary_Int(int aAddress, int resultAddress, OpCode op)
    {
        int a = memory.ReadInt(aAddress);
        int result = Math_Unary_Int(a, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_Unary_Long(int aAddress, int resultAddress, OpCode op)
    {
        long a = memory.ReadLong(aAddress);
        long result = Math_Unary_Long(a, op);
        memory.Write(resultAddress, BitConverter.GetBytes(result));
    }
    
    
    
    private void MathMemory_IncDec_Byte(int targetAddress, OpCode op)
    {
        byte a = memory.Read(targetAddress);
        byte result = Math_IncDec_Byte(a, op);
        memory.Write(targetAddress, result);
    }
    private void MathMemory_IncDec_Short(int targetAddress, OpCode op)
    {
        short a = memory.ReadShort(targetAddress);
        short result = Math_IncDec_Short(a, op);
        memory.Write(targetAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_IncDec_Int(int targetAddress, OpCode op)
    {
        int a = memory.ReadInt(targetAddress);
        int result = Math_IncDec_Int(a, op);
        memory.Write(targetAddress, BitConverter.GetBytes(result));
    }
    private void MathMemory_IncDec_Long(int targetAddress, OpCode op)
    {
        long a = memory.ReadLong(targetAddress);
        long result = Math_IncDec_Long(a, op);
        memory.Write(targetAddress, BitConverter.GetBytes(result));
    }
    
    
    private byte Math_Bytes(byte a, byte b, OpCode op)
    {
        switch (op)
        {
            case OpCode.Add: return (byte)(a + b);
            case OpCode.Sub: return (byte)(a - b);
            case OpCode.Mul: return (byte)(a * b);
            case OpCode.Div: return (byte)(a / b);
            case OpCode.DivRemainder: return (byte)(a % b);
            case OpCode.LeftBitShift: return (byte)(a << b);
            case OpCode.RightBitShift: return (byte)(a >> b);
            case OpCode.BitAnd: return (byte)(a & b);
            case OpCode.BitOr: return (byte)(a | b);
            default: throw new Exception($"Invalid math opcode {op}");
        }
    }
    private short Math_Shorts(short a, short b, OpCode op)
    {
        switch (op)
        {
            case OpCode.Add: return (short)(a + b);
            case OpCode.Sub: return (short)(a - b);
            case OpCode.Mul: return (short)(a * b);
            case OpCode.Div: return (short)(a / b);
            case OpCode.DivRemainder: return (short)(a % b);
            case OpCode.LeftBitShift: return (short)(a << b);
            case OpCode.RightBitShift: return (short)(a >> b);
            case OpCode.BitAnd: return (short)(a & b);
            case OpCode.BitOr: return (short)(a | b);
            default: throw new Exception($"Invalid math opcode {op}");
        }
    }
    private int Math_Ints(int a, int b, OpCode op)
    {
        switch (op)
        {
            case OpCode.Add: return a + b;
            case OpCode.Sub: return a - b;
            case OpCode.Mul: return a * b;
            case OpCode.Div: return a / b;
            case OpCode.DivRemainder: return a % b;
            case OpCode.LeftBitShift: return a << b;
            case OpCode.RightBitShift: return a >> b;
            case OpCode.BitAnd: return a & b;
            case OpCode.BitOr: return a | b;
            default: throw new Exception($"Invalid math opcode {op}");
        }
    }
    private long Math_Longs(long a, long b, OpCode op)
    {
        switch (op)
        {
            case OpCode.Add: return a + b;
            case OpCode.Sub: return a - b;
            case OpCode.Mul: return a * b;
            case OpCode.Div: return a / b;
            case OpCode.DivRemainder: return a % b;
            case OpCode.LeftBitShift: return a << (int)b;
            case OpCode.RightBitShift: return a >> (int)b;
            case OpCode.BitAnd: return a & b;
            case OpCode.BitOr: return a | b;
            default: throw new Exception($"Invalid binary math opcode {op}");
        }
    }

    private byte Math_Unary_Byte(byte a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Negate: return (byte)(-a);
            default: throw new Exception($"Invalid unary math opcode {op}");
        }
    }
    private short Math_Unary_Short(short a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Negate: return (short)(-a);
            default: throw new Exception($"Invalid unary math opcode {op}");
        }
    }
    private int Math_Unary_Int(int a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Negate: return -a;
            default: throw new Exception($"Invalid unary math opcode {op}");
        }
    }
    private long Math_Unary_Long(long a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Negate: return -a;
            default: throw new Exception($"Invalid unary math opcode {op}");
        }
    }
    
    
    
    private byte Math_IncDec_Byte(byte a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Increment: return ++a;
            case OpCode.Decrement: return --a;
            default: throw new Exception($"Invalid IncDec math opcode {op}");
        }
    }
    private short Math_IncDec_Short(short a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Increment: return ++a;
            case OpCode.Decrement: return --a;
            default: throw new Exception($"Invalid IncDec math opcode {op}");
        }
    }
    private int Math_IncDec_Int(int a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Increment: return ++a;
            case OpCode.Decrement: return --a;
            default: throw new Exception($"Invalid IncDec math opcode {op}");
        }
    }
    private long Math_IncDec_Long(long a, OpCode op)
    {
        switch (op)
        {
            case OpCode.Increment: return ++a;
            case OpCode.Decrement: return --a;
            default: throw new Exception($"Invalid IncDec math opcode {op}");
        }
    }
}