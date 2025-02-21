namespace AVM;

public partial class VM
{
    private void VMCommand()
    { 
        VMCommand_Cmd cmd = (VMCommand_Cmd)Next();

        if (cmd == VMCommand_Cmd.Print)
        {
            Print();
        }
        else
        {
            throw new Exception($"VM command {cmd} is not implemented.");
        }
    }

    private void Print()
    {
        int argumentsCount = NextInt();
        for (int i = 0; i < argumentsCount; i++)
        {
            int rbp = NextInt();
            byte size = Next();
            byte typeIndex = Next();

            int address = memory.ToAbs(rbp);
            byte[] value = memory.Read(address, size);

            if (typeIndex == 0)
            {
                bool b = value[0] > 0;
                Console.Write(b);
            }
            else if (typeIndex == 1)
            {
                byte v = value[0];
                Console.Write(v);
            }
            else if (typeIndex == 2)
            {
                short v = BitConverter.ToInt16(value);
                Console.Write(v);
            }
            else if (typeIndex == 3)
            {
                int v = BitConverter.ToInt32(value);
                Console.Write(v);
            }
            else if (typeIndex == 4)
            {
                long v = BitConverter.ToInt64(value);
                Console.Write(v);
            }
            else if (typeIndex == 5)
            {
                int ptrAddress = BitConverter.ToInt32(value);
                Console.Write("<0x" + ptrAddress.ToString("x") + ">");
            }
            else if (typeIndex == 6)
            {
                int ptrAddress = BitConverter.ToInt32(value);

                int strLen = memory.ReadInt(ptrAddress);
                char[] chars = new char[strLen];
                for (int j = 0; j < strLen; j++)
                {
                    chars[j] = (char)memory.Read(ptrAddress + sizeof(int) + j);
                }
                
                Console.Write(string.Concat(chars));
            }
        }
        
        Console.WriteLine();
    }
}