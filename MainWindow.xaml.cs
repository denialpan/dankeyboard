using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static IntPtr hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc proc = HookCallback;

        private DispatcherTimer timer;
        private static HashSet<Key> pressedKeys = new HashSet<Key>();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50); // Adjust interval as needed
            timer.Tick += Timer_Tick;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hookId = SetHook(proc);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // this method primarily exists to detect when the alt tab functionality is detected
            // it does not detect the combination as one, but the keys individually
            // decent workaround, since alt is finnicky to deal with


            // alt key
            switch ((GetAsyncKeyState(0x12) & 0x8000)) {
                case 0:
                    pressedKeys.Remove(KeyInterop.KeyFromVirtualKey(0x12));
                    break;
                default:
                    if (!pressedKeys.Contains(KeyInterop.KeyFromVirtualKey(0x12)))
                    {
                        pressedKeys.Add(KeyInterop.KeyFromVirtualKey(0x12));
                        Debug.WriteLine("Alt key pressed");
                    }
                    break;

            }

            // tab key
            switch ((GetAsyncKeyState(0x09) & 0x8000))
            {
                case 0:
                    pressedKeys.Remove(KeyInterop.KeyFromVirtualKey(0x09));
                    break;
                default:
                    if (!pressedKeys.Contains(KeyInterop.KeyFromVirtualKey(0x09)))
                    {
                        pressedKeys.Add(KeyInterop.KeyFromVirtualKey(0x09));
                        Debug.WriteLine("Tab key pressed");
                    }
                    break;

            }

        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        protected override void OnClosed(EventArgs e)
        {
            timer.Stop();
            base.OnClosed(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(hookId);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (pressedKeys.Contains(key)) {
                    pressedKeys.Remove(key);
                }

            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                ModifierKeys modifierKeys = Keyboard.Modifiers & ~ModifierKeys.Alt;

                if (!pressedKeys.Contains(key) && key != Key.Tab) { 
                    Debug.WriteLine($"Key: {key}, Modifiers: {modifierKeys}");
                    pressedKeys.Add(key);
                }

            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}