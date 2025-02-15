namespace AVM;

public enum OpCode : byte
{
    Invalid,
    
    Allocate_Stack,
    FunctionPrologue,
    FunctionEpilogue,
    Call,
    Return,
    Jump,
    JumpIfFalse,
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
    Compare,
    
    Negate,

    ToPtr_ValueType,
    ToPtr_RefType,
    PtrGet,
    PtrSet,
    PtrShift,
    
    Last
}