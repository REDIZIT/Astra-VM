pub struct BinaryFile
{
    pub bytes: Vec<u8>,
    pub current: usize,
}

impl BinaryFile
{
    pub fn new(bytes: Vec<u8>) -> Self
    {
        Self {
            bytes,
            current: 0
        }
    }
    pub fn next(&mut self) -> u8
    {
        let byte: u8 = self.bytes[self.current];
        self.current += 1;
        byte
    }
    pub fn next_range(&mut self, count: usize) -> &[u8]
    {
        let slice: &[u8] = self.bytes[self.current..self.current + count].try_into().unwrap();

        self.current += count;

        slice
    }

    pub fn next_int(&mut self) -> u32
    {
        u32::from_ne_bytes(self.next_range(4).try_into().unwrap())
    }

    pub fn next_string(&mut self) -> String
    {
        let length = self.next_int();
        String::from_utf8(Vec::from(self.next_range(length as usize))).unwrap()
    }

    pub fn next_bool(&mut self) -> bool
    {
        self.next() > 0
    }
    
    pub fn can_next(&self) -> bool {
        self.current < self.bytes.len() - 1
    }
}