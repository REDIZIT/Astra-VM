namespace AVM;

public partial class VM
{
    private void ToPtr_ValueType()
    {
        int askedVariableAddress = NextAddress();
        int resultAddress = NextAddress();

        memory.Write(resultAddress, BitConverter.GetBytes(askedVariableAddress));
    }
    private void ToPtr_RefType()
    {
        int askedVariableRbp = NextInt();
        int askedVariableAddress = memory.ToAbs(askedVariableRbp);
        int resultAddress = NextAddress();

        int valueAddress = memory.ReadInt(askedVariableAddress); // depoint one more time
        
        memory.Write(resultAddress, BitConverter.GetBytes(valueAddress));
    }

    private void PtrGet()
    {
        int pointerRbpOffset = NextInt();
        int pointerAddress = memory.ToAbs(pointerRbpOffset);

        int resultRbpOffset = NextInt();
        int resultAddress = memory.ToAbs(resultRbpOffset);
        byte size = Next();
        
        int depointedAddress = memory.ReadInt(pointerAddress);
        
        byte[] value = memory.Read(depointedAddress, size);
        memory.Write(resultAddress, value);
    }
    private void PtrSet()
    {
        int pointerRbpOffset = NextInt();
        int pointerAddress = memory.ToAbs(pointerRbpOffset);

        int valueRbpOffset = NextInt();
        int valueAddress = memory.ToAbs(valueRbpOffset);
        byte size = Next();

        int depointedAddress = memory.ReadInt(pointerAddress);
        
        byte[] value = memory.Read(valueAddress, size);
        memory.Write(depointedAddress, value);
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