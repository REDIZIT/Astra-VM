use std::collections::HashMap;
use std::ptr::NonNull;
use std::sync::Mutex;
use std::thread;
use lazy_static::lazy_static;
use windows::core::s;
use windows::Win32::Foundation::*;
use crate::vm::compiled_module::{CompiledModule, FunctionInfo_Blit};
use crate::vm::vm::VM;

use windows::{
    core::*, Win32::Foundation::*, Win32::Graphics::Gdi::ValidateRect,
    Win32::System::LibraryLoader::GetModuleHandleA, Win32::UI::WindowsAndMessaging::*,
};
use windows::Win32::Graphics::Direct2D::*;
use windows::Win32::Graphics::Direct2D::Common::{D2D1_ALPHA_MODE_IGNORE, D2D1_COLOR_F, D2D1_COMPOSITE_MODE_SOURCE_OVER, D2D1_PIXEL_FORMAT};
use windows::Win32::Graphics::Direct3D11::*;
use windows::Win32::Graphics::Direct3D::*;
use windows::Win32::Graphics::Dxgi::*;
use windows::Win32::Graphics::Dxgi::DXGI_PRESENT_TEST;
use windows::Win32::Graphics::Dxgi::Common::{DXGI_FORMAT_B8G8R8A8_UNORM, DXGI_SAMPLE_DESC};
use windows::Win32::Graphics::Gdi::*;
use windows::Win32::System::Com::{CoCreateInstance, CLSCTX_ALL};
use windows::Win32::System::Performance::QueryPerformanceFrequency;
use windows::Win32::UI::Animation::*;
use windows_numerics::Matrix3x2;
use crate::vm::interrupt;
use crate::example::main_example;

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
    pub hwnd: HWND,
    pub on_paint_inmodule_index: i32,

    factory: ID2D1Factory1,

    target: Option<ID2D1DeviceContext>,
    brush: Option<ID2D1SolidColorBrush>,
    dpi: f32,
    visible: bool,
    occlusion: u32,
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
        ).unwrap();

        let window = Window::new(hwnd, on_paint_inmodule_index).unwrap();

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

                let mut data = WINFRAMEWORKDATA.lock().unwrap();
                let window = data.windows.get_mut(&(window_hwnd.0 as i32)).expect(format!("There is no window registered for received wndproc HWND = {:?}", window_hwnd.0).as_str());
                let on_paint_inmodule_index = window.on_paint_inmodule_index;


                // let vm: &mut VM = data.vm.unwrap().as_mut();
                // let function = &vm.module.table.functions[on_paint_inmodule_index as usize];
                // let new_current = function.pointed_opcode as i32;
                // interrupt(vm, new_current);
                // drop(vm);

                // println!("WM_PAINT");

                // draw_gdi(window_hwnd);
                draw_dx(window);

                

                ValidateRect(Option::from(window_hwnd), None);

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

fn draw_dx(window: &mut Window)
{
    unsafe {
        let mut ps = PAINTSTRUCT::default();
        BeginPaint(window.hwnd, &mut ps);
        window.render().unwrap();
        EndPaint(window.hwnd, &ps);
    }
}

impl Window
{
    fn new(hwnd: HWND, on_paint_inmodule_index: i32) -> Result<Self>
    {        
        let factory = create_factory()?;

        let mut dpi = 0.0;
        let mut dpiy = 0.0;
        unsafe { factory.GetDesktopDpi(&mut dpi, &mut dpiy) };
        
        Ok(Self {
            id: hwnd.0 as i32,
            hwnd,
            on_paint_inmodule_index,
            factory,
            target: None,
            brush: None,
            dpi,
            visible: false,
            occlusion: 0
        })
    }
    
    fn render(&mut self) -> Result<()>
    {
        if self.target.is_none() {
            let device = create_device()?;
            let target = create_render_target(&self.factory, &device)?;
            unsafe { target.SetDpi(self.dpi, self.dpi) };

            self.brush = create_brush(&target).ok();
            self.target = Some(target);
        }
        
        let target = self.target.as_ref().unwrap();
        
        unsafe { target.BeginDraw() };
        self.draw(target);

        unsafe {
            target.EndDraw(None, None)?;
        }

        // if let Err(error) = self.present(1, 0) {
        //     println!("{}", error);
        //     if error.code() == DXGI_STATUS_OCCLUDED {
        //         // self.occlusion = unsafe {
        //         //     self.dxfactory
        //         //         .RegisterOcclusionStatusWindow(self.handle, WM_USER)?
        //         // };
        //         self.visible = false;
        //     } else {
        //         self.release_device();
        //     }
        // }

        Ok(())
    }
    
    fn draw(&self, target: &ID2D1DeviceContext)
    {
        unsafe {
            target.Clear(Some(&D2D1_COLOR_F {
                r: 1.0,
                g: 0.0,
                b: 1.0,
                a: 1.0,
            }));


            // let previous = target.GetTarget().unwrap();
            // target.SetTarget(&previous);
            // target.SetTransform(&Matrix3x2::translation(5.0, 5.0));

            // target.DrawImage(
            //     &shadow.GetOutput().unwrap(),
            //     None,
            //     None,
            //     D2D1_INTERPOLATION_MODE_LINEAR,
            //     D2D1_COMPOSITE_MODE_SOURCE_OVER,
            // );

            // target.SetTransform(&Matrix3x2::identity());

            // target.DrawImage(
            //     clock,
            //     None,
            //     None,
            //     D2D1_INTERPOLATION_MODE_LINEAR,
            //     D2D1_COMPOSITE_MODE_SOURCE_OVER,
            // );
        }
    }

    // fn present(&self, sync: u32, flags: DXGI_PRESENT) -> Result<()> {
    //     unsafe { self.swapchain.as_ref().unwrap().Present(sync, flags).ok() }
    // }
}
fn create_factory() -> Result<ID2D1Factory1> {
    let mut options = D2D1_FACTORY_OPTIONS::default();

    if cfg!(debug_assertions) {
        options.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
    }

    unsafe { D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, Some(&options)) }
}

fn create_device_with_type(drive_type: D3D_DRIVER_TYPE) -> Result<ID3D11Device> {
    let mut flags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

    if cfg!(debug_assertions) {
        flags |= D3D11_CREATE_DEVICE_DEBUG;
    }

    let mut device = None;

    unsafe {
        D3D11CreateDevice(
            None,
            drive_type,
            HMODULE::default(),
            flags,
            None,
            D3D11_SDK_VERSION,
            Some(&mut device),
            None,
            None,
        )
            .map(|()| device.unwrap())
    }
}

fn create_device() -> Result<ID3D11Device> {
    let mut result = create_device_with_type(D3D_DRIVER_TYPE_HARDWARE);
    //
    // let result = unsafe { D3D11CreateDevice(None, D3D_FEATURE_LEVEL_12_0, std::ptr::null_mut()) };

    unsafe {
        let mut flags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

        // if cfg!(debug_assertions) {
        //     flags |= D3D11_CREATE_DEVICE_DEBUG;
        // }

        let mut device = None;

        let mut result = D3D11CreateDevice(
            None,
            D3D_DRIVER_TYPE_HARDWARE,
            HMODULE::default(),
            flags,
            None,
            D3D11_SDK_VERSION,
            Some(&mut device),
            None,
            None,
        ).map(|()| device.unwrap());

        // if let Err(err) = &result {
        //     if err.code() == DXGI_ERROR_UNSUPPORTED {
        //         result = create_device_with_type(D3D_DRIVER_TYPE_WARP);
        //     }
        // }

        if let Err(e) = &result {
            println!("Ошибка при создании устройства D3D12: {:?}", e);
        }

        result
    }



}

fn create_render_target(
    factory: &ID2D1Factory1,
    device: &ID3D11Device,
) -> Result<ID2D1DeviceContext> {
    unsafe {
        let d2device = factory.CreateDevice(&device.cast::<IDXGIDevice>()?)?;

        let target = d2device.CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS_NONE)?;

        target.SetUnitMode(D2D1_UNIT_MODE_DIPS);

        Ok(target)
    }
}

fn get_dxgi_factory(device: &ID3D11Device) -> Result<IDXGIFactory2> {
    let dxdevice = device.cast::<IDXGIDevice>()?;
    unsafe { dxdevice.GetAdapter()?.GetParent() }
}

fn create_swapchain_bitmap(swapchain: &IDXGISwapChain1, target: &ID2D1DeviceContext) -> Result<()> {
    let surface: IDXGISurface = unsafe { swapchain.GetBuffer(0)? };

    let props = D2D1_BITMAP_PROPERTIES1 {
        pixelFormat: D2D1_PIXEL_FORMAT {
            format: DXGI_FORMAT_B8G8R8A8_UNORM,
            alphaMode: D2D1_ALPHA_MODE_IGNORE,
        },
        dpiX: 96.0,
        dpiY: 96.0,
        bitmapOptions: D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
        ..Default::default()
    };

    unsafe {
        let bitmap = target.CreateBitmapFromDxgiSurface(&surface, Some(&props))?;
        target.SetTarget(&bitmap);
    };

    Ok(())
}

fn create_swapchain(device: &ID3D11Device, window: HWND) -> Result<IDXGISwapChain1> {
    let factory = get_dxgi_factory(device)?;

    let props = DXGI_SWAP_CHAIN_DESC1 {
        Format: DXGI_FORMAT_B8G8R8A8_UNORM,
        SampleDesc: DXGI_SAMPLE_DESC {
            Count: 1,
            Quality: 0,
        },
        BufferUsage: DXGI_USAGE_RENDER_TARGET_OUTPUT,
        BufferCount: 2,
        SwapEffect: DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
        ..Default::default()
    };

    unsafe { factory.CreateSwapChainForHwnd(device, window, &props, None, None) }
}
fn create_brush(target: &ID2D1DeviceContext) -> Result<ID2D1SolidColorBrush> {
    let color = D2D1_COLOR_F {
        r: 0.92,
        g: 0.38,
        b: 0.208,
        a: 1.0,
    };

    let properties = D2D1_BRUSH_PROPERTIES {
        opacity: 0.8,
        transform: Matrix3x2::identity(),
    };

    unsafe { target.CreateSolidColorBrush(&color, Some(&properties)) }
}




fn draw_gdi(window_hwnd: HWND)
{
    // unsafe {
    //     let mut ps = PAINTSTRUCT::default();
    //     let hdc = BeginPaint(window_hwnd, &mut ps);
    // 
    //     // Определяем координаты квадрата
    //     let rect = RECT {
    //         left: 50,
    //         top: 50,
    //         right: 200,
    //         bottom: 200,
    //     };
    // 
    //     // Выбираем цвет заливки
    //     let brush = CreateSolidBrush(COLORREF::default()); // Красный
    // 
    //     // Заливаем квадрат
    //     FillRect(hdc, &rect, brush);
    // 
    //     // Освобождаем ресурсы
    //     DeleteObject(brush);
    //     EndPaint(window_hwnd, &ps);
    // }
}