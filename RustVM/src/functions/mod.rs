mod math_functions;
mod compare_functions;

use crate::functions::compare_functions::compare;
use crate::functions::math_functions::{*};
use crate::vm::VM;

pub fn get_functions() -> [fn(&mut VM); 37]
{
    let functions =
    [
        do_nothing,
        allocate_stack,
        allocate_heap,
        deallocate_stack,
        prologue,
        epilogue,
        call,
        _return,
        jump,
        jump_if_false,
        exit,
        mov,
        add,
        sub,
        mul,
        div,
        div_remainder,
        left_bit_shift,
        right_bit_shift,
        bit_and,
        bit_or,
        compare,
        negate,
        increment,
        decrement,
        to_ptr_value_type,
        to_ptr_ref_type,
        ptr_get,
        ptr_set,
        ptr_shift,
        field_access,
        allocate_rsp_saver,
        restore_rsp_saver,
        deallocate_rsp_saver,
        cast,
        section,
        vm_command
    ];
    functions
}

fn do_nothing(_vm: &mut VM)
{
}
fn allocate_stack(vm: &mut VM)
{
    let mode = vm.byte_code.next();

    if mode == 0
    {
        let bytes_to_allocate = vm.byte_code.next() as i32;

        let address = vm.memory.allocate_stack(bytes_to_allocate);
        let default_value = vm.byte_code.next_range(bytes_to_allocate as usize);
        vm.memory.write_slice(address, default_value)
    }
    else if mode == 1
    {
        let variable_to_push_address = vm.next_address();
        let size = vm.byte_code.next() as i32;

        let address = vm.memory.allocate_stack(size);

        let value = vm.memory.read(variable_to_push_address, size).to_vec();
        vm.memory.write_vec(address, value);
    }
    else
    {
        panic!("Invalid stack allocation mode {mode}")
    }
}
fn allocate_heap(vm: &mut VM)
{
    let mode = vm.byte_code.next();
    let storage_address = vm.next_address();

    if mode == 0
    {
        let bytes_to_allocate = vm.byte_code.next_int();

        let pointer = vm.memory.allocate_heap(bytes_to_allocate);
        vm.memory.write_int(storage_address, pointer as i32)
    }
}
fn deallocate_stack(vm: &mut VM)
{
    let bytes_to_deallocate = vm.byte_code.next_int();
    vm.memory.deallocate_stack(bytes_to_deallocate);
}
fn prologue(vm: &mut VM)
{
    vm.memory.push_int(vm.memory.base_pointer as i32);
    vm.memory.base_pointer = vm.memory.stack_pointer;
}
fn epilogue(vm: &mut VM)
{
    vm.memory.stack_pointer = vm.memory.base_pointer;
    vm.memory.base_pointer = vm.memory.pop_int();
}
fn call(vm: &mut VM)
{
    vm.memory.push_int((vm.byte_code.current - 1) as i32);

    let inmodule_function_index = vm.byte_code.next_int() as usize;
    let function_info = &vm.module.table.functions[inmodule_function_index];

    vm.byte_code.current = function_info.pointed_opcode as usize - 1;
}
fn _return(vm: &mut VM)
{
    let call_op_code_pointer = vm.memory.pop_int();
    vm.byte_code.current = (call_op_code_pointer + 1 + 4) as usize; // + 1 (OpCode.Call) + int (pointer to label)
}
fn jump(vm: &mut VM)
{
    let address = vm.byte_code.next_int();
    vm.byte_code.current = address as usize;
}
fn jump_if_false(vm: &mut VM)
{
    let jump_address = vm.byte_code.next_int();
    let condition_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();

    let mut is_true = false;
    for b in vm.memory.read(condition_address, size_in_bytes as i32)
    {
        if *b > 0
        {
            is_true = true;
            break;
        }
    }

    if is_true == false
    {
        vm.byte_code.current = jump_address as usize;
    }
}
fn exit(vm: &mut VM)
{
    vm.byte_code.current = vm.byte_code.bytes.len();
}

fn mov(vm: &mut VM)
{
    let dst_mode = vm.byte_code.next();
    let dst_address: i32;

    if dst_mode == 1
    {
        // Dst is pointer
        dst_address = vm.next_address();
    }
    else if dst_mode == 2
    {
        // Dst is address behind pointer
        let dst_ptr_address = vm.next_address();
        dst_address = vm.memory.read_int(dst_ptr_address);
    }
    else
    {
        panic!("Invalid mov's dst_mode {dst_mode}");
    }


    let src_mode = vm.byte_code.next();

    if src_mode == 1
    {
        // src is rbp offset
        let src_address = vm.next_address();

        let size_in_bytes = vm.byte_code.next() as i32;

        vm.memory.copy(src_address, dst_address, size_in_bytes);
    }
    else if src_mode == 2
    {
        // src is stack address

        let src_size_in_bytes = vm.byte_code.next();
        let src_value = vm.byte_code.next_range(src_size_in_bytes as usize);
        vm.memory.write_slice(dst_address, src_value);
    }
    else if src_mode == 3
    {
        // src is value behind stack address
        let src_address = vm.next_address();
        let src_size_in_bytes = vm.byte_code.next() as i32;

        vm.memory.copy(src_address, dst_address, src_size_in_bytes);
    }
    else if src_mode == 4
    {
        // src is abs address
        let src_address = vm.byte_code.next_int();
        let src_size_in_bytes = vm.byte_code.next() as i32;

        vm.memory.copy(src_address, dst_address, src_size_in_bytes);
    }
    else
    {
        panic!("Invalid mov's src_mode {src_mode}");
    }
}

fn to_ptr_value_type(vm: &mut VM)
{
    let asked_variable_address = vm.next_address();
    let result_address = vm.next_address();

    vm.memory.write_int(result_address, asked_variable_address);
}

fn to_ptr_ref_type(vm: &mut VM)
{
    let asked_variable_address = vm.next_address();
    let result_address = vm.next_address();

    let value_address = vm.memory.read_int(asked_variable_address); // depoint one more time

    vm.memory.write_int(result_address, value_address);
}

fn ptr_get(vm: &mut VM)
{
    let pointer_address = vm.next_address();
    let result_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();

    let depointed_address = vm.memory.read_int(pointer_address);
    vm.memory.copy(depointed_address, result_address, size_in_bytes as i32);
}

fn ptr_set(vm: &mut VM)
{
    let pointer_address = vm.next_address();
    let value_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();

    let depointed_address = vm.memory.read_int(pointer_address);
    vm.memory.copy(value_address, depointed_address, size_in_bytes as i32);
}

fn ptr_shift(vm: &mut VM)
{
    let mode = vm.byte_code.next();
    let pointer_address = vm.next_address();

    let shift_value;

    if mode == 0
    {
        shift_value = vm.byte_code.next_int();
    }
    else
    {
        let shift_address = vm.next_address();
        let additional_shift = vm.byte_code.next_int();
        let size_in_bytes = vm.byte_code.next();

        shift_value = vm.memory.read_int(shift_address) + additional_shift;
    }

    let mut pointer_value = vm.memory.read_int(pointer_address);
    pointer_value += shift_value;
    vm.memory.write_int(pointer_address, pointer_value);
}

fn field_access(vm: &mut VM)
{
    let base_offset = vm.byte_code.next_int();
    let field_offset = vm.byte_code.next_int();
    let field_value_size = vm.byte_code.next();
    let is_getter = vm.byte_code.next();
    let result_address = vm.next_address();

    let address_in_stack = vm.memory.to_abs(base_offset);
    let address_in_heap = vm.memory.read_int(address_in_stack);

    // fieldPointer is pointing to valid address of ref-type.field
    let field_pointer = address_in_heap + field_offset;


    // If we don't need a pointer (like setter), but want to get a value (like getter)
    if is_getter > 0
    {
        // Depoint rbx to get actual field value due to getter
        // Put in result a value (not fixed size) of field (getter)
        vm.memory.copy(field_pointer, result_address, field_value_size as i32);
    }
    else
    {
        // Put in result a pointer (fixed size) to field (setter)
        vm.memory.write_int(result_address, field_pointer);
    }
}

fn allocate_rsp_saver(vm: &mut VM) {
    panic!("Legacy method")
}

fn restore_rsp_saver(vm: &mut VM) {
    panic!("Legacy method")
}

fn deallocate_rsp_saver(vm: &mut VM) {
    panic!("Legacy method")
}

fn cast(vm: &mut VM)
{
    let variable_address = vm.next_address();
    let variable_size = vm.byte_code.next();
    let result_address = vm.next_address();
    let result_size = vm.byte_code.next();

    let variable_value = vm.memory.read(variable_address, variable_size as i32).to_vec();

    for i in 0..result_size as i32
    {
        if i < variable_value.len() as i32
        {
            vm.memory.write_byte(result_address + i, variable_value[i as usize]);
        }
        else
        {
            vm.memory.write_byte(result_address + i, 0);
        }
    }
}

fn section(vm: &mut VM) {
    
    let mode = vm.byte_code.next();
    
    if mode == 0
    {
        // Data section
        let data_section_size = vm.byte_code.next_int();

        vm.byte_code.current += data_section_size as usize;
        
        vm.memory.data_section_size = data_section_size;
        vm.memory.stack_pointer += data_section_size;
        vm.memory.heap_pointer += data_section_size;
        vm.memory.base_pointer += data_section_size;
    }

    let next_section_opcode = vm.byte_code.next();
    let next_mode = vm.byte_code.next();
}

fn vm_command(vm: &mut VM)
{
    panic!("Not supported yet");
}