namespace AVM;

public static class Program
{
    public static void Main(string[] args)
    {
        VM vm = new();

        // byte[] byteCode = File.ReadAllBytes("../../../docs/example.ab");
        byte[] byteCode = File.ReadAllBytes("C:/Users/REDIZIT/Documents/GitHub/AstraOS/main/vscode project/build/project.nasm");

        vm.Load(byteCode);
        vm.Execute();
    }
}