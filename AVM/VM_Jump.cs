namespace AVM;

public partial class VM
{
    private void Jump()
    {
        int address = NextInt();
        current = address;
    }

    private void JumpIfFalse()
    {
        int jumpAddress = NextInt();
        int conditionAddress = NextAddress();
        byte size = Next();

        bool isTrue = false;
        foreach (byte b in memory.Read(conditionAddress, size))
        {
            if (b > 0)
            {
                isTrue = true;
                break;
            }
        }

        if (isTrue == false)
        {
            current = jumpAddress;
        }
    }
}