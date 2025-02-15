namespace AVM;

public static class Program
{
    public static void Main(string[] args)
    {
        VM vm = new();

        string filepath = "C:/Users/REDIZIT/Documents/GitHub/AstraOS/main/vscode project/build/project.nasm";
        byte[] byteCode = File.ReadAllBytes(filepath);

        vm.Load(byteCode);
        vm.Execute();
    }
}