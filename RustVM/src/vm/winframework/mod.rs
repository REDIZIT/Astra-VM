use std::collections::HashMap;
use std::ops::Deref;
use std::ptr::NonNull;
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Duration;
use lazy_static::lazy_static;
use windows::core::s;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use crate::vm::compiled_module::{CompiledModule, FunctionInfo_Blit};
use crate::vm::vm::VM;

use windows::{
    core::*, Win32::Foundation::*, Win32::Graphics::Gdi::ValidateRect,
    Win32::System::LibraryLoader::GetModuleHandleA, Win32::UI::WindowsAndMessaging::*,
};
use crate::vm::interrupt;

lazy_static! {
	pub static ref WINFRAMEWORKDATA: Mutex<WinFrameworkData> = Mutex::new(WinFrameworkData {
		windows: HashMap::new(),
		vm: None,
	});
}

unsafe impl Send for WinFrameworkData {}
unsafe impl Sync for WinFrameworkData {}


pub struct WinFrameworkData
{
    pub windows: HashMap<i32, Window>,
    pub vm: Option<NonNull<VM>>
}
#[derive(Debug)]
pub struct Window
{
    pub id: i32,
    pub on_paint_inmodule_index: i32,
}


pub fn apply(module: &mut CompiledModule)
{
    let window_type = module.table.types.iter().position(|t| t.name == "Window").unwrap() as u32;

    for f in &mut module.table.functions
    {
        if f.owner_type == window_type
        {
            if f.name == "New"
            {
                f.pointed_module = 1;
                f.pointed_opcode = 0;
            }
            else
            {
                panic!("Unknown function {}", f.name);
            }
        }
    }
}

pub fn set_vm(vm: &mut VM)
{
    let mut data = WINFRAMEWORKDATA.lock().unwrap();
    data.vm = Some(NonNull::from(vm));
}

pub fn call(vm: &VM, function_info: &FunctionInfo_Blit)
{
    if function_info.name == "New"
    {
        let on_paint_inmodule_index = vm.memory.read_int(vm.memory.stack_pointer - 4 - 4);

        println!("Create new window: {}", on_paint_inmodule_index);

        create_window(on_paint_inmodule_index);

        println!("Window created")
    }
    else
    {
        panic!("Failed to call function '{}' in module 'winframework'", {&function_info.name})
    }
}

// fn create_window(on_paint_inmodule_index: i32)
fn create_window(on_paint_inmodule_index: i32)
{
    thread::spawn(move || unsafe {

        let instance = GetModuleHandleA(None).unwrap();
        let window_class = s!("window");

        let wc = WNDCLASSA {
            hCursor: LoadCursorW(None, IDC_ARROW).unwrap(),
            hInstance: instance.into(),
            lpszClassName: window_class,

            style: CS_HREDRAW | CS_VREDRAW,
            lpfnWndProc: Some(wndproc),
            ..Default::default()
        };

        let atom = RegisterClassA(&wc);
        debug_assert!(atom != 0);


        let hwnd = CreateWindowExA(
            WINDOW_EX_STYLE::default(),
            window_class,
            s!("This is NOT a sample window"),
            WS_OVERLAPPEDWINDOW | WS_VISIBLE,
            CW_USEDEFAULT,
            CW_USEDEFAULT,
            CW_USEDEFAULT,
            CW_USEDEFAULT,
            None,
            None,
            None,
            None,
        );

        let window = Window
        {
            id: hwnd.0 as i32,
            on_paint_inmodule_index
        };

        let mut data = WINFRAMEWORKDATA.lock().unwrap();
        data.windows.insert(window.id, window);
        drop(data);

        let mut message = MSG::default();

        while GetMessageA(&mut message, None, 0, 0).into() {
            DispatchMessageA(&message);
        }
    });
}

extern "system" fn wndproc(window_hwnd: HWND, message: u32, wparam: WPARAM, lparam: LPARAM) -> LRESULT {
    unsafe {
        // println!("wndproc: {}", message);
        match message {
            WM_PAINT => {

                let data = WINFRAMEWORKDATA.lock().unwrap();
                let window = data.windows.get(&(window_hwnd.0 as i32)).expect(format!("There is no window registered for received wndproc HWND = {}", window_hwnd.0).as_str());
                let on_paint_inmodule_index = window.on_paint_inmodule_index;


                let vm: &mut VM = data.vm.unwrap().as_mut();
                let function = &vm.module.table.functions[on_paint_inmodule_index as usize];
                let new_current = function.pointed_opcode as i32;

                // println!("WM_PAINT");

                interrupt(vm, new_current);

                ValidateRect(window_hwnd, None);

                LRESULT(0)
            },
            // WM_PAINT => {
            //     println!("WM_PAINT");
            //     _ = ValidateRect(window, None);
            //     LRESULT(0)
            // }
            // WM_DESTROY => {
            //     println!("WM_DESTROY");
            //     PostQuitMessage(0);
            //     LRESULT(0)
            // }
            _ => DefWindowProcA(window_hwnd, message, wparam, lparam),
        }
    }
}