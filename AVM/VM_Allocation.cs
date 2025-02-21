namespace AVM;

public partial class VM
{
    private void Allocate_Stack()
    {
        byte mode = Next();

        if (mode == 0)
        {
            byte bytesToAllocate = Next();
        
            int address = memory.Allocate_Stack(bytesToAllocate);
            byte[] defaultValue = byteCode[current..(current + bytesToAllocate)];
            memory.Write(address, defaultValue);
            
            current += bytesToAllocate;
        }
        else if (mode == 1)
        {
            int variableToPushAddress = NextAddress();
            byte size = Next();
            
            int address = memory.Allocate_Stack(size);

            byte[] value = memory.Read(variableToPushAddress, size);
            memory.Write(address, value);
        }
        else
        {
            throw new Exception($"Invalid stack allocation mode {mode}");
        }
    }

    private void Allocate_Heap()
    {
        byte mode = Next();
        int storageAddress = NextAddress();

        if (mode == 0)
        {
            int bytesToAllocate = NextInt();

            int pointer = memory.Allocate_Heap(bytesToAllocate);
            memory.WriteInt(storageAddress, pointer);
        }
        else if (mode == 1)
        {
            int bytesToAllocateVariableAddress = NextAddress();
            byte size = Next();


            int bytesToAllocate;
            if (size == 1) bytesToAllocate = memory.Read(bytesToAllocateVariableAddress);
            else if (size == 2) bytesToAllocate = memory.ReadShort(bytesToAllocateVariableAddress);
            else if (size == 4) bytesToAllocate = memory.ReadInt(bytesToAllocateVariableAddress);
            else if (size == 8) bytesToAllocate = (int)memory.ReadLong(bytesToAllocateVariableAddress);
            else throw new Exception($"Invalid heap allocation bytes variable size {size}");

            int pointer = memory.Allocate_Heap(bytesToAllocate);
            memory.WriteInt(storageAddress, pointer);
        }
        else
        {
            throw new Exception($"Invalid heap allocation mode {mode}");
        }
    }

    private void Deallocate_Stack()
    {
        int bytesToDeallocate = NextInt();
        memory.Deallocate_Stack(bytesToDeallocate);
    }

    private void AllocateRSPSaver()
    {
        // memory.PushInt(memory.stackPointer);
        
        memory.PushInt(memory.basePointer);
        memory.basePointer = memory.stackPointer;
    }
    private void RestoreRSPSaver()
    {
        // int savedRSP = NextInt();
        // memory.stackPointer = -savedRSP; // restore negative rsp
        
        memory.stackPointer = memory.basePointer;
    }
    private void DeallocateRSPSaver()
    {
        // memory.stackPointer = memory.PopInt();
        
        memory.stackPointer = memory.basePointer;
        memory.basePointer = memory.PopInt();
    }
}