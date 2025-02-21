namespace AVM;

public partial class VM
{
    private int dataSectionSize;
    
    private void Section()
    {
        byte sectionType = Next();

        if (sectionType == 0)
        {
            SectionData();
        }
        else if (sectionType == 1)
        {
            // Do nothing
        }
        else
        {
            throw new Exception($"Invalid section type = {sectionType}");
        }
    }

    private void SectionData()
    {
        dataSectionSize = NextInt();
        memory.Write(0, byteCode[current..(current + dataSectionSize)], noLogs: true);

        current += dataSectionSize;

        byte opCode = Next();
        if (opCode != (byte)OpCode.Section)
        {
            throw new Exception($"Invalid data section. After data section expected another Section opcode, but got {opCode}");
        }
        
        byte sectionType = Next();
        if (sectionType != 1)
        {
            throw new Exception($"Invalid data section. After data section expected Code (1) section, but got sectionType = {sectionType}");
        }

        memory.basePointer += dataSectionSize;
        memory.stackPointer += dataSectionSize;
        memory.heapPointer += dataSectionSize;
    }
}