mod binary_file;
mod compiled_module;
mod opcodes;
mod functions;
mod vm;
mod memory;

use std::fs::File;
use std::io::Read;
use num_enum::TryFromPrimitive;
use stopwatch::Stopwatch;
use crate::binary_file::BinaryFile;
use crate::compiled_module::deserialize_module_from_bytes;
use crate::functions::get_functions;
use crate::memory::Memory;
use crate::opcodes::OpCode;
use crate::vm::VM;

fn main() {
    let asc_path = "C:/Users/REDIZIT/Documents/GitHub/Astra Projects/Desktop/bin/project.asc";

    let file = File::open(asc_path);
    let mut buffer = Vec::new();
    file.unwrap().read_to_end(&mut buffer).unwrap();

    let module = deserialize_module_from_bytes(&buffer);

    println!("{:?} byte code", module.managed_code.bytes);


    let functions = get_functions();

    let mut byte_code = BinaryFile::new(&module.managed_code.bytes);
    let mut total_opcodes_completed = 0;

    let mut vm = VM {
        byte_code: &mut byte_code,
        memory: Memory::new(),
        module: &module
    };

    let mut w = Stopwatch::start_new();

    while vm.byte_code.can_next()
    {
        if total_opcodes_completed > 1000 {
            panic!("Too many opcodes completed. Seems there is an infinite loop.")
        }
        total_opcodes_completed += 1;

        let byte_opcode = vm.byte_code.next();
        let opcode = OpCode::try_from(byte_opcode).expect("Invalid opcode");

        println!("{:?}", opcode);

        functions[byte_opcode as usize](&mut vm);
    }

    w.stop();

    let exit_code = vm.memory.read_int(vm.memory.data_section_size);
    println!("Successful executed in {} ms with exit code {}", w.elapsed_ms(), exit_code);
}