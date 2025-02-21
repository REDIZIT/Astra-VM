namespace AVM;

public enum OpCode : byte
{
    Invalid,
    
    Allocate_Stack,
    Allocate_Heap,
    Deallocate_Stack,
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
    
    FieldAccess,
    
    AllocateRSPSaver,
    RestoreRSPSaver,
    DeallocateRSPSaver,
    
    Cast,
    
    Section,
    
    Last
}

public enum Allocate_Stack_Mode : byte
{
    WithDefaultValue = 0,
    PushAlreadyAllocatedVariable = 1
}