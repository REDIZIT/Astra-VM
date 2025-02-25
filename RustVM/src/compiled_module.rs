use crate::binary_file::BinaryFile;

pub fn deserialize_module_from_bytes(buffer: &Vec<u8>) -> CompiledModule {

    let mut file: BinaryFile = BinaryFile::new(buffer);
    let module = deserialize_module(&mut file);
    module
}

fn deserialize_module(file: &mut BinaryFile) -> CompiledModule
{
    CompiledModule {
        table: deserialize_metatable(file),
        managed_code: deserialize_managed_code(file)
    }
}

fn deserialize_metatable(file: &mut BinaryFile) -> MetaTable
{
    MetaTable {
        types: deserialize_types(file),
        functions: deserialize_functions(file),
    }
}

fn deserialize_managed_code(file: &mut BinaryFile) -> ManagedCode
{
    let count = file.next_int();

    ManagedCode {
        bytes: Vec::from(file.next_range(count as usize))
    }
}

fn deserialize_functions(file: &mut BinaryFile) -> Vec<FunctionInfo_Blit>
{
    let count = file.next_int();
    (0..count).map(|_| deserialize_function(file)).collect()
}

fn deserialize_function(file: &mut BinaryFile) -> FunctionInfo_Blit
{
    FunctionInfo_Blit {
        name: file.next_string(),
        is_static: file.next_bool(),
        is_abstract: file.next_bool(),
        owner_type: file.next_uint(),
        arguments: deserialize_fields(file),
        returns: deserialize_indexes(file),
        pointed_opcode: file.next_uint()
    }
}

fn deserialize_types(file: &mut BinaryFile) -> Vec<TypeInfo_Blit>
{
    let count = file.next_int();
    let mut types = Vec::with_capacity(count as usize);

    for _ in 0..count
    {
        types.push(deserialize_type(file))
    }

    types
}

fn deserialize_type(file: &mut BinaryFile) -> TypeInfo_Blit
{
    let type_info: TypeInfo_Blit = TypeInfo_Blit {
        name: file.next_string(),
        is_value_type: file.next_bool(),
        fields: deserialize_fields(file),
        functions: deserialize_indexes(file)
    };

    type_info
}

fn deserialize_fields(file: &mut BinaryFile) -> Vec<FieldInfo_Blit>
{
    let count = file.next_int();
    let mut fields = Vec::with_capacity(count as usize);

    for _ in 0..count
    {
        fields.push(deserialize_field(file))
    }

    fields
}

fn deserialize_field(file: &mut BinaryFile) -> FieldInfo_Blit
{
    FieldInfo_Blit {
        name: file.next_string(),
        type_index: file.next_uint()
    }
}

fn deserialize_indexes(file: &mut BinaryFile) -> Vec<u32>
{
    let count = file.next_int();
    (0..count).map(|_| file.next_uint()).collect()
}



pub struct CompiledModule {
    pub table: MetaTable,
    pub managed_code: ManagedCode
}
pub struct MetaTable {
    pub types: Vec<TypeInfo_Blit>,
    pub functions: Vec<FunctionInfo_Blit>
}
pub struct ManagedCode {
    pub bytes: Vec<u8>
}
pub struct TypeInfo_Blit {
    pub name: String,
    pub is_value_type: bool,
    pub fields: Vec<FieldInfo_Blit>,
    pub functions: Vec<u32>,
}
pub struct FieldInfo_Blit {
    pub name: String,
    pub type_index: u32,
}
pub struct FunctionInfo_Blit {
    pub name: String,
    pub is_static: bool,
    pub is_abstract: bool,
    pub owner_type: u32,
    pub arguments: Vec<FieldInfo_Blit>,
    pub returns: Vec<u32>,
    pub pointed_opcode: u32
}