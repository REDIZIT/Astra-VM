use crate::binary_file::BinaryFile;
use crate::compiled_module::CompiledModule;
use crate::memory::Memory;

pub struct VM<'a>
{
    pub byte_code: &'a mut BinaryFile,
    pub memory: Memory,
    pub module: &'a CompiledModule,
}

impl VM<'_>
{
    pub fn next_address(&mut self) -> i32
    {
        let rbp_offset = self.byte_code.next_int();
        self.memory.to_abs(rbp_offset)
    }
}