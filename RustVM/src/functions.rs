use std::fs::metadata;
use crate::vm::VM;

pub fn get_functions() -> [fn(&mut VM); 37]
{
    let functions =
    [
        invalid,
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

fn invalid(vm: &mut VM)
{
    
}
fn allocate_stack(vm: &mut VM)
{
    let mode = vm.byte_code.next();
    
    if mode == 0
    {
        let bytes_to_allocate = vm.byte_code.next() as usize;
        
        let address = vm.memory.allocate_stack(bytes_to_allocate);
        let default_value = vm.byte_code.next_range(bytes_to_allocate);
        vm.memory.write_slice(address, default_value)
    }
    else if mode == 1
    {
        let variable_to_push_address = vm.next_address();
        let size = vm.byte_code.next() as usize;
        
        let address = vm.memory.allocate_stack(size);
        
        let value = vm.memory.read(variable_to_push_address, size).to_vec();
        vm.memory.write_vec(address, value);
    }
    else
    {
        panic!("Invalid stack allocation mode {mode}")
    }
    
    println!("Mode = {mode}")
}
fn allocate_heap(vm: &mut VM){
    
}
fn deallocate_stack(vm: &mut VM) {
    
}
fn prologue(vm: &mut VM) {

}
fn epilogue(vm: &mut VM) {

}
fn call(vm: &mut VM) {

}
fn _return(vm: &mut VM) {

}
fn jump(vm: &mut VM) {

}
fn jump_if_false(vm: &mut VM) {

}
fn exit(vm: &mut VM) {

}
fn mov(vm: &mut VM) {

}
fn add(vm: &mut VM) {

}
fn sub(vm: &mut VM) {

}
fn mul(vm: &mut VM) {

}
fn div(vm: &mut VM) {

}
fn div_remainder(vm: &mut VM) {

}
fn left_bit_shift(vm: &mut VM) {

}
fn right_bit_shift(vm: &mut VM) {

}
fn bit_and(vm: &mut VM) {

}
fn bit_or(vm: &mut VM) {

}
fn compare(vm: &mut VM) {

}
fn negate(vm: &mut VM) {

}
fn increment(vm: &mut VM) {

}
fn decrement(vm: &mut VM) {

}

fn to_ptr_value_type(vm: &mut VM) {

}

fn to_ptr_ref_type(vm: &mut VM) {

}

fn ptr_get(vm: &mut VM) {

}

fn ptr_set(vm: &mut VM) {

}

fn ptr_shift(vm: &mut VM) {

}

fn field_access(vm: &mut VM) {

}

fn allocate_rsp_saver(vm: &mut VM) {

}

fn restore_rsp_saver(vm: &mut VM) {

}

fn deallocate_rsp_saver(vm: &mut VM) {

}

fn cast(vm: &mut VM) {

}

fn section(vm: &mut VM) {
    
    let mode = vm.byte_code.next();
    
    if mode == 0
    {
        // Data section
        let data_section_size = vm.byte_code.next_int() as usize;
        
        vm.byte_code.current += data_section_size;
        
        vm.memory.stack_pointer += data_section_size;
        vm.memory.heap_pointer += data_section_size;
        vm.memory.base_pointer += data_section_size;
    }
    
    let next_section_opcode = vm.byte_code.next();
    let next_mode = vm.byte_code.next();
}

fn vm_command(vm: &mut VM) {

}