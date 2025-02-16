namespace AVM;

public partial class VM
{
    private void Cast()
    {
        int variableAddress = NextAddress();
        byte variableSize = Next();
        int resultAddress = NextAddress();
        byte resultSize = Next();

        byte[] variableValue = memory.Read(variableAddress, variableSize);
        
        for (int i = 0; i < resultSize; i++)
        {
            if (i < variableValue.Length)
            {
                memory.Write(resultAddress + i, variableValue[i]);    
            }
            else
            {
                memory.Write(resultAddress + i, 0);
            }
        }
    }
}