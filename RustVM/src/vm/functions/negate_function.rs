use crate::vm::vm::VM;
use paste::paste;

macro_rules! negate_sized {
     ($t:ty) => {
         paste! {
            fn [<negate_ $t>](a_value: &[u8]) -> Vec<u8> {
                let a = $t::from_ne_bytes(a_value.try_into().unwrap());
                (-a).to_ne_bytes().to_vec()
            }
        }
     };
}

negate_sized!(i8);
negate_sized!(i16);
negate_sized!(i32);
negate_sized!(i64);

pub fn negate(vm: &mut VM)
{
    let a_address = vm.next_address();
    let result_address = vm.next_address();
    let size_in_bytes = vm.byte_code.next();

    let a_value = vm.memory.read(a_address, size_in_bytes as i32);

    let result = match size_in_bytes
    {
        1 => negate_i8(a_value),
        2 => negate_i16(a_value),
        4 => negate_i32(a_value),
        8 => negate_i64(a_value),
        _ => panic!("Not supported number size ({size_in_bytes} bytes)")
    };

    vm.memory.write_vec(result_address, result);
}