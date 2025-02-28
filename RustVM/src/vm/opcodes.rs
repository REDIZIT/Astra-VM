use num_enum::TryFromPrimitive;

#[derive(Debug, TryFromPrimitive)]
#[repr(u8)]
pub enum OpCode
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
    DivRemainder,
    LeftBitShift,
    RightBitShift,
    BitAnd,
    BitOr,
    Compare,

    Negate,
    Increment,
    Decrement,

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

    VMCommand,

    Last
}

#[derive(Debug, TryFromPrimitive)]
#[repr(u8)]
pub enum Allocate_Stack_Mode
{
    WithDefaultValue = 0,
    PushAlreadyAllocatedVariable = 1
}

#[derive(Debug, TryFromPrimitive)]
#[repr(u8)]
pub enum VMCommand_Cmd
{
    Print,
    CreateWindow,
    Sleep,
}