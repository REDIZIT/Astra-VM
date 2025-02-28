use std::thread;
use std::time::Duration;
use windows::core::s;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use crate::vm::compiled_module::{CompiledModule, FunctionInfo_Blit};
use crate::vm::vm::VM;

use windows::{
    core::*, Win32::Foundation::*, Win32::Graphics::Gdi::ValidateRect,
    Win32::System::LibraryLoader::GetModuleHandleA, Win32::UI::WindowsAndMessaging::*,
};

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

pub fn call(vm: &mut VM, function_info: &FunctionInfo_Blit)
{
    if function_info.name == "New"
    {
        println!("Create new window");
        thread::spawn(create_window);
        
        thread::sleep(Duration::from_millis(3000));
        println!("Crated");
    }
    else 
    { 
        panic!("Failed to call function '{}' in module 'winframework'", {&function_info.name})
    }
}

fn create_window()
{
    unsafe {
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

        CreateWindowExA(
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

        let mut message = MSG::default();

        while GetMessageA(&mut message, None, 0, 0).into() {
            DispatchMessageA(&message);
        }
    }
}

extern "system" fn wndproc(window: HWND, message: u32, wparam: WPARAM, lparam: LPARAM) -> LRESULT {
    unsafe {
        match message {
            WM_PAINT => {
                println!("WM_PAINT");
                _ = ValidateRect(window, None);
                LRESULT(0)
            }
            WM_DESTROY => {
                println!("WM_DESTROY");
                PostQuitMessage(0);
                LRESULT(0)
            }
            _ => DefWindowProcA(window, message, wparam, lparam),
        }
    }
}