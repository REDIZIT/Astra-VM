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
    
    Add,
    Sub,
    Mul,
    Div,
    LeftBitShift,
    RightBitShift,
    BitAnd,
    BitOr,
    
    Last
}