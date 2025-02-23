namespace AVM;

public partial class VM
{
    private List<Form> forms = new();

    private void VMCommand()
    { 
        VMCommand_Cmd cmd = (VMCommand_Cmd)Next();

        if (cmd == VMCommand_Cmd.Print)
        {
            Print();
        }
        else if (cmd == VMCommand_Cmd.CreateWindow)
        {
            CreateWindow();
        }
        else if (cmd == VMCommand_Cmd.Sleep)
        {
            Sleep();
        }
        else
        {
            throw new Exception($"VM command {cmd} is not implemented.");
        }
    }

    private void CreateWindow()
    {
        int argumentsCount = NextInt();

        NextVMVariable(out int retRpb, out _, out _);

        Application.EnableVisualStyles();

        Form form = new();
        forms.Add(form);
        Thread t = StartTheThread(form);

        Window window = new Window()
        {
            test = 78
        };

        int heapPtr = memory.Allocate_Heap(sizeof(int));
        byte[] bytes = BytesSerializer.Serialize(window);
        memory.Write(heapPtr, bytes);

        memory.WriteInt(memory.ToAbs(retRpb), heapPtr);
    }
    private Thread StartTheThread(Form form)
    {
        var t = new Thread(() =>
        {
            Application.Run(form);
        });
        t.Start();
        return t;
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

    private void Sleep()
    {
        int argumentsCount = NextInt();
        NextVMVariable(out int rbp, out _, out _);

        int ms = memory.ReadInt(memory.ToAbs(rbp));

        Thread.Sleep(ms);
    }

    private void NextVMVariable(out int rbp, out byte size, out byte typeIndex)
    {
        rbp = NextInt();
        size = Next();
        typeIndex = Next();
    }
}

public struct Window
{
    public int test;
}
