use crate::vm::VM;
use paste::paste;


macro_rules! math_binary_op_sized {
    ($t:ty, $name:ident, $op:tt) => {
        paste! {
            fn [<$name _ $t>](a_value: &[u8], b_value: &[u8]) -> Vec<u8> {
                let a = $t::from_ne_bytes(a_value.try_into().unwrap());
                let b = $t::from_ne_bytes(b_value.try_into().unwrap());
                (a $op b).to_ne_bytes().to_vec()
            }
        }
    };
}
macro_rules! math_binary_op_each_size {
    ($name:ident, $op:tt) => {
        math_binary_op_sized!(i8, $name, $op);
        math_binary_op_sized!(i16, $name, $op);
        math_binary_op_sized!(i32, $name, $op);
        math_binary_op_sized!(i64, $name, $op);
    };
}

macro_rules! math_binary_op {
    ($name:ident, $op:tt) => {
		paste! {
			pub fn $name(vm: &mut VM)
			{
				let a_address = vm.next_address();
				let b_address = vm.next_address();
				let result_address = vm.next_address();
				let size_in_bytes = vm.byte_code.next();

				let a_value = vm.memory.read(a_address, size_in_bytes as i32);
				let b_value = vm.memory.read(b_address, size_in_bytes as i32);

				let result = match size_in_bytes
				{
					1 => [<$name _i8>](a_value, b_value),
					2 => [<$name _i16>](a_value, b_value),
					4 => [<$name _i32>](a_value, b_value),
					8 => [<$name _i64>](a_value, b_value),
					_ => panic!("Not supported number size ({size_in_bytes} bytes)")
				};

				vm.memory.write_vec(result_address, result);
			}
            
            math_binary_op_each_size!($name, $op);
		}
	};
}

math_binary_op!(add, +);
math_binary_op!(sub, -);
math_binary_op!(mul, *);
math_binary_op!(div, /);
math_binary_op!(div_remainder, %);
math_binary_op!(left_bit_shift, <<);
math_binary_op!(right_bit_shift, >>);
math_binary_op!(bit_and, &);
math_binary_op!(bit_or, |);



macro_rules! math_unary_op_sized {
    ($t:ty, $name:ident, $op:expr) => {
        paste! {
            fn [<$name _ $t>](a_value: &[u8]) -> Vec<u8> {
                let a = $t::from_ne_bytes(a_value.try_into().unwrap());
                ($op(a)).to_ne_bytes().to_vec()
            }
        }
    };
}
macro_rules! math_unary_op_each_size {
    ($name:ident, $op:expr) => {
        math_unary_op_sized!(i8, $name, $op);
        math_unary_op_sized!(i16, $name, $op);
        math_unary_op_sized!(i32, $name, $op);
        math_unary_op_sized!(i64, $name, $op);
    };
}
macro_rules! math_unary_op {
    ($name:ident, $op:expr) => {
		paste! {
			pub fn $name(vm: &mut VM)
			{
				let value_address = vm.next_address();
				let size_in_bytes = vm.byte_code.next();

				let a_value = vm.memory.read(value_address, size_in_bytes as i32);

				let result = match size_in_bytes
				{
					1 => [<$name _i8>](a_value),
					2 => [<$name _i16>](a_value),
					4 => [<$name _i32>](a_value),
					8 => [<$name _i64>](a_value),
					_ => panic!("Not supported number size ({size_in_bytes} bytes)")
				};

				vm.memory.write_vec(value_address, result);
			}

            math_unary_op_each_size!($name, $op);
		}
	};
}

math_unary_op!(increment, |a| a + 1);
math_unary_op!(decrement, |a| a - 1);

pub fn negate(vm: &mut VM)
{
    let a_address = vm.next_address();
    let result_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();
    
    let a_value = vm.memory.read(a_address, size_in_bytes as i32);
    let result = !as_bool(a_value);
    
    vm.memory.write_vec(result_address, bool_to_vec(result, size_in_bytes as usize));
}

fn as_bool(bytes: &[u8]) -> bool
{
    for b in bytes
    {
        if *b > 0
        {
            return true
        }
    }
    false
}
fn bool_to_vec(value: bool, size: usize) -> Vec<u8>
{
    let mut vec = Vec::new();
    vec.push(if value { 1u8 } else { 0u8 });
    
    for _ in (1..size)
    {
        vec.push(0)
    }    
    vec
}