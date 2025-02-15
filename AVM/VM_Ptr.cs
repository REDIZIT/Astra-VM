namespace AVM;

public partial class VM
{
    private void ToPtr_ValueType()
    {
        int askedRbpOffset = NextInt();
        int resultAddress = memory.ToAbs(NextInt());

        int valueRbpOffset = memory.ToAbs(askedRbpOffset);
        memory.Write(resultAddress, BitConverter.GetBytes(valueRbpOffset));
    }
    private void ToPtr_RefType()
    {
        int askedRbpOffset = NextInt();
        int resultAddress = memory.ToAbs(NextInt());

        int variableRbpOffset = memory.ToAbs(askedRbpOffset);
        int valueRbpOffset = memory.ReadInt(variableRbpOffset);
        memory.Write(resultAddress, BitConverter.GetBytes(valueRbpOffset));
    }

    private void PtrGet()
    {
        int pointerRbpOffset = NextInt();
        int pointerAddress = memory.ToAbs(pointerRbpOffset);

        int resultRbpOffset = NextInt();
        int resultAddress = memory.ToAbs(resultRbpOffset);
        byte size = Next();
        
        int dopointedAddress = memory.ReadInt(pointerAddress);
        
        byte[] value = memory.Read(dopointedAddress, size);
        memory.Write(resultAddress, value);
    }
    private void PtrSet()
    {
        int pointerRbpOffset = NextInt();
        int pointerAddress = memory.ToAbs(pointerRbpOffset);

        int valueRbpOffset = NextInt();
        int valueAddress = memory.ToAbs(valueRbpOffset);
        byte size = Next();

        int dopointedAddress = memory.ReadInt(pointerAddress);
        
        byte[] value = memory.Read(valueAddress, size);
        memory.Write(dopointedAddress, value);
    }

    private void PtrShift()
    {
        byte mode = Next();
        int pointerRbpOffset = NextInt();
        int pointerAddress = memory.ToAbs(pointerRbpOffset);
        int shiftValue;

        if (mode == 0)
        {
            shiftValue = NextInt();
        }
        else
        {
            int shiftRbpOffset = NextInt();
            int shiftAddress = memory.ToAbs(shiftRbpOffset);

            int additionalShift = NextInt();
            
            byte size = Next();

            byte[] shiftValueBytes = memory.Read(shiftAddress, size);

            shiftValue = BitConverter.ToInt32(shiftValueBytes) + additionalShift;
        }

        int pointerValue = memory.ReadInt(pointerAddress);
        pointerValue += shiftValue;
        memory.WriteInt(pointerAddress, pointerValue);
    }
}