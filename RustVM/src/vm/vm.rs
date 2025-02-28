use crate::vm::binary_file::BinaryFile;
use crate::vm::compiled_module::CompiledModule;
use crate::vm::memory::Memory;

pub struct VM
{
    pub byte_code: Box<BinaryFile>,
    pub memory: Memory,
    pub module: Box<CompiledModule>,
}

impl VM
{
    pub fn next_address(&mut self) -> i32
    {
        let rbp_offset = self.byte_code.next_int();
        self.memory.to_abs(rbp_offset)
    }
}