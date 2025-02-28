mod binary_file;
mod compiled_module;
mod opcodes;
mod vm;
mod memory;
mod functions;

use std::env;
use std::fs::File;
use std::io::Read;
use num_enum::TryFromPrimitive;
use paste::paste;
use stopwatch::Stopwatch;
use binary_file::BinaryFile;
use compiled_module::deserialize_module_from_bytes;
use functions::get_functions;
use memory::Memory;
use vm::VM;
use crate::vm::opcodes::OpCode;

#[macro_export] macro_rules! debug_log {
    ($($arg:tt)*) => {
		// #[cfg(debug_assertions)]
		// {
		// 	println!($($arg)*);
		// }
	};
}

pub fn vm_start()
{
    let mut asc_path = "C:/Users/REDIZIT/Documents/GitHub/Astra Projects/Desktop/bin/project.asc";
    let mut opcodes_limit = -1;

    let args: Vec<String> = env::args().collect();
    if args.len() > 1
    {
        asc_path = args[1].as_str();
        println!("asc_path = {asc_path}")
    }
    if args.len() > 2
    {
        opcodes_limit = args[2].parse().unwrap();
        println!("opcodes_limit = {opcodes_limit}")
    }


    let file = File::open(asc_path);
    let mut buffer = Vec::new();
    file.unwrap().read_to_end(&mut buffer).unwrap();

    let module = deserialize_module_from_bytes(&buffer);


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
        if opcodes_limit != -1 && total_opcodes_completed > opcodes_limit {
            panic!("Too many opcodes completed. Seems there is an infinite loop.")
        }
        total_opcodes_completed += 1;

        let byte_opcode = vm.byte_code.next();
        
        // let opcode = OpCode::try_from(byte_opcode).expect("Invalid opcode");
        // println!("{:?}", opcode);

        functions[byte_opcode as usize](&mut vm);
    }

    w.stop();

    let exit_code = vm.memory.read_int(vm.memory.data_section_size);
    println!("Successful executed in {} ms with exit code {}", w.elapsed_ms(), exit_code);
}