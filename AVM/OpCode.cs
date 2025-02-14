namespace AVM;

public enum OpCode : byte
{
    Invalid,
    
    Allocate_Stack,
    FunctionPrologue,
    FunctionEpilogue,
    Call,
    Return,
    Exit,
    Mov,
    
    Last
}