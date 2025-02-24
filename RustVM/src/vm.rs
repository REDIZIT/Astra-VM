use crate::binary_file::BinaryFile;

pub struct VM<'a>
{
    pub byte_code: &'a mut BinaryFile,
    pub memory: Memory,
}

impl VM<'_>
{
    pub fn next_address(&mut self) -> usize
    {
        let rbp_offset = self.byte_code.next_int() as usize;
        self.memory.to_abs(rbp_offset)
    }
}

pub struct Memory
{
    pub bytes: [u8; Memory::STACK_SIZE],
    pub stack_pointer: usize,
    pub base_pointer: usize,
    pub heap_pointer: usize,
}

impl Memory
{
    const STACK_SIZE: usize = 512;

    pub fn new() -> Self
    {
        Self {
            bytes: [0; Memory::STACK_SIZE],
            stack_pointer: 0,
            base_pointer: 0,
            heap_pointer: Memory::STACK_SIZE
        }
    }

    pub fn allocate_stack(&mut self, bytes_to_allocate: usize) -> usize
    {
        let pointer = self.stack_pointer;

        self.stack_pointer += bytes_to_allocate;
        if self.stack_pointer >= Memory::STACK_SIZE
        {
            panic!("Failed to allocate {bytes_to_allocate} bytes on stack due to stack overflow")
        }

        pointer
    }

    pub fn write_slice(&mut self, address: usize, bytes: &[u8])
    {
        self.bytes[address..(address + bytes.len())].copy_from_slice(bytes);
    }
    pub fn write_vec(&mut self, address: usize, bytes: Vec<u8>)
    {
        self.bytes[address..(address + bytes.len())].copy_from_slice(&*bytes);
    }

    pub fn read(&self, address: usize, count: usize) -> &[u8]
    {
        &self.bytes[address..(address + count)]
    }

    pub fn to_abs(&self, rbp_offset: usize) -> usize
    {
        self.base_pointer + rbp_offset
    }
}