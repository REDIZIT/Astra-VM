use paste::paste;
use crate::vm::VM;



macro_rules! math_compare_op_sized {
    ($t:ty, $name:ident, $op:tt) => {
        paste! {
            fn [<compare_ $name _ $t>](a_value: &[u8], b_value: &[u8]) -> u8 {
                let a = $t::from_ne_bytes(a_value.try_into().unwrap());
                let b = $t::from_ne_bytes(b_value.try_into().unwrap());
                let result = a $op b;
                if result { 1u8 } else { 0u8 }
            }
        }
    };
}

macro_rules! math_compare_op_each_size {
    ($name:ident, $op:tt) => {
        math_compare_op_sized!(i8, $name, $op);
        math_compare_op_sized!(i16, $name, $op);
        math_compare_op_sized!(i32, $name, $op);
        math_compare_op_sized!(i64, $name, $op);
    };
}

macro_rules! math_compare_op {
    ($name:ident, $op:tt) => {
        paste! {
            pub fn [<compare_ $name>](a_value: &[u8], b_value: &[u8], size_in_bytes: u8) -> u8
            {
                match size_in_bytes
                {
                    1 => [<compare_ $name _i8>](a_value, b_value),
                    2 => [<compare_ $name _i16>](a_value, b_value),
                    4 => [<compare_ $name _i32>](a_value, b_value),
                    8 => [<compare_ $name _i64>](a_value, b_value),
                    _ => panic!("Not supported number size ({size_in_bytes} bytes)")
                }
            }

            math_compare_op_each_size!($name, $op);
        }
    };
}

math_compare_op!(e, ==);
math_compare_op!(ne, !=);
math_compare_op!(g, >);
math_compare_op!(ge, >=);
math_compare_op!(l, <);
math_compare_op!(le, <=);

pub fn compare(vm: &mut VM)
{
    let a_address = vm.next_address();
    let b_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();
    let result_address = vm.next_address();
    let op = vm.byte_code.next();

    let a_value = vm.memory.read(a_address, size_in_bytes as i32);
    let b_value = vm.memory.read(b_address, size_in_bytes as i32);

    let result = match op {
        0 => compare_e(a_value, b_value, size_in_bytes),
        1 => compare_ne(a_value, b_value, size_in_bytes),
        2 => compare_g(a_value, b_value, size_in_bytes),
        3 => compare_ge(a_value, b_value, size_in_bytes),
        4 => compare_l(a_value, b_value, size_in_bytes),
        5 => compare_le(a_value, b_value, size_in_bytes),
        _ => panic!("Invalid compare operator {op}"),
    };
    
    vm.memory.write_byte(result_address, result);
}

