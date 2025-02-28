pub struct Memory
{
    pub bytes: [u8; Memory::STACK_SIZE as usize],
    pub stack_pointer: i32,
    pub base_pointer: i32,
    pub heap_pointer: i32,
    pub data_section_size: i32,
}

impl Memory
{
    pub const STACK_SIZE: i32 = 512;

    pub fn new() -> Self
    {
        Self {
            bytes: [0; Memory::STACK_SIZE as usize],
            stack_pointer: 0,
            base_pointer: 0,
            heap_pointer: Memory::STACK_SIZE / 2,
            data_section_size: 0,
        }
    }

    pub fn allocate_stack(&mut self, bytes_to_allocate: i32) -> i32
    {
        let pointer = self.stack_pointer;

        self.stack_pointer += bytes_to_allocate;
        if self.stack_pointer >= Memory::STACK_SIZE
        {
            panic!("Failed to allocate {bytes_to_allocate} bytes on stack due to stack overflow")
        }

        pointer
    }
    pub fn allocate_heap(&mut self, bytes_to_allocate: i32) -> i32
    {
        let pointer = self.heap_pointer;
        self.heap_pointer += bytes_to_allocate;
        pointer
    }
    pub fn deallocate_stack(&mut self, bytes_to_deallocate: i32)
    {
        self.stack_pointer -= bytes_to_deallocate;
    }

    pub fn push_int(&mut self, value: i32)
    {
        self.write_int(self.stack_pointer, value);
        self.stack_pointer += 4;
    }
    pub fn pop_int(&mut self) -> i32
    {
        self.stack_pointer -= 4;
        self.read_int(self.stack_pointer)
    }

    pub fn write_slice(&mut self, address: i32, bytes: &[u8])
    {
        self.slice(address, bytes.len() as i32).copy_from_slice(bytes);
    }
    pub fn write_vec(&mut self, address: i32, bytes: Vec<u8>)
    {
        self.slice(address, bytes.len() as i32).copy_from_slice(&*bytes);
    }
    pub fn write_int(&mut self, address: i32, value: i32)
    {
        self.slice(address, 4).copy_from_slice(value.to_ne_bytes().as_slice());
    }
    pub fn write_byte(&mut self, address: i32, value: u8)
    {
        self.bytes[address as usize] = value;
    }

    pub fn read(&self, address: i32, count: i32) -> &[u8]
    {
        &self.bytes[(address as usize)..((address + count) as usize)]
    }
    pub fn slice(&mut self, address: i32, count: i32) -> &mut [u8]
    {
        &mut self.bytes[(address as usize)..((address + count) as usize)]
    }
    pub fn read_int(&self, address: i32) -> i32
    {
        i32::from_ne_bytes(self.read(address, 4).try_into().unwrap())
    }

    pub fn copy(&mut self, src_address: i32, dst_address: i32, count: i32)
    {
        let src_slice = self.read(src_address, count).to_vec();
        self.slice(dst_address, count).copy_from_slice(&*src_slice);
    }

    pub fn to_abs(&self, rbp_offset: i32) -> i32
    {
        self.base_pointer + rbp_offset
    }
}