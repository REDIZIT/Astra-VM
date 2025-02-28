use std::fmt::Arguments;
use std::thread;
use std::time::Duration;
use crate::vm::opcodes::VMCommand_Cmd;
use crate::vm::vm::VM;
use crate::vm::winframework::WINFRAMEWORKDATA;

pub fn vm_command(vm: &mut VM)
{
    let cmd_byte = vm.byte_code.next();
    let cmd = VMCommand_Cmd::try_from(cmd_byte).expect(format!("Invalid VM command = {cmd_byte}").as_str());

    let mut arguments = Vec::new();
    let arguments_count = vm.byte_code.next_int();

    for _ in 0..arguments_count
    {
        let argument = VMCmdArgument
        {
            rbp: vm.byte_code.next_int(),
            size_in_bytes: vm.byte_code.next() as u8,
            type_index: vm.byte_code.next() as u8
        };
        arguments.push(argument);
    }

    match cmd {
        VMCommand_Cmd::Print => vm_print(vm, arguments),
        VMCommand_Cmd::Sleep => vm_sleep(vm, arguments),
        _ => panic!("Invalid VM command = {cmd_byte}")
    }
}

fn vm_print(vm: &VM, arguments: Vec<VMCmdArgument>)
{
    for arg in arguments
    {
        let address = vm.memory.to_abs(arg.rbp);
        let value = vm.memory.read(address, arg.size_in_bytes as i32);

        match arg.type_index {
            0 => print!("{}", value[0] > 0),
            1 => print!("{}", value[0]),
            2 => print!("{}", i16::from_ne_bytes(value.try_into().unwrap())),
            3 => print!("{}", i32::from_ne_bytes(value.try_into().unwrap())),
            4 => print!("{}", i64::from_ne_bytes(value.try_into().unwrap())),
            5 => {
                let ptr_address = i32::from_ne_bytes(value.try_into().unwrap());
                print!("<0x{:X}>", ptr_address);
            },
            6 => {
                let ptr_address = i32::from_ne_bytes(value.try_into().unwrap());

                let str_len = vm.memory.read_int(ptr_address);
                let str_value = vm.memory.read(address + 4, str_len);

                print!("'{:?}'", str_value);
            }
            _ => panic!("Failed to print argument with type_index = {}", arg.type_index)
        }
    }

    println!();
}

fn vm_sleep(vm: &VM, arguments: Vec<VMCmdArgument>)
{
    let duration_argument = &arguments[0];
    let address = vm.memory.to_abs(duration_argument.rbp);
    let value = vm.memory.read(address, duration_argument.size_in_bytes as i32);

    let duration = match duration_argument.type_index {
        1 => value[0] as u64,
        2 => i16::from_ne_bytes(value.try_into().unwrap()) as u64,
        3 => i32::from_ne_bytes(value.try_into().unwrap()) as u64,
        4 => i64::from_ne_bytes(value.try_into().unwrap()) as u64,
        _ => panic!("Failed to sleep due to invalid argument type_index = {}", duration_argument.type_index)
    };

    thread::sleep(Duration::from_millis(duration));
}

struct VMCmdArgument
{
    rbp: i32,
    size_in_bytes: u8,
    type_index: u8
}