﻿namespace AVM;

public static class Program
{
    public static void Main(string[] args)
    {
        VM vm = new();

        string filepath = "C:\\Users\\REDIZIT\\Documents\\GitHub\\Astra Projects\\Desktop\\bin\\project.asc";

        if (args.Length > 0)
        {
            filepath = args[0];
        }
        
        byte[] byteCode = File.ReadAllBytes(filepath);

        vm.Load(byteCode);
        vm.Execute();
    }
}