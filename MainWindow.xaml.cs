using dankeyboard.src;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static bool controlPressed = false;
        private static bool shiftPressed = false;

        private static IntPtr hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc keyboardProc = HookCallback;
        private static HashSet<Keys> pressedKeys = new HashSet<Keys>();

        private KeyboardHeatmap kbhm;
        public MainWindow() {
            InitializeComponent();
            kbhm = new KeyboardHeatmap();

            // method loaded upon window open
            Loaded += MainWindow_Loaded;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Call YourMethod when the window is loaded
            kbhm.color_heatmap(KeyboardTab);
            hookId = SetHook(keyboardProc);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hookId);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                Debug.WriteLine("Key: " + key);
                
                pressedKeys.Remove(key);

                if (key == Keys.ControlL || key == Keys.ControlR)
                {
                    controlPressed = false;
                }
                else if (key == Keys.ShiftL || key == Keys.ShiftR)
                {
                    shiftPressed = false;
                }

            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (!pressedKeys.Contains(key)) {
                    pressedKeys.Add(key);

                    if (controlPressed)
                    {
                        Debug.WriteLine("Ctrl + " + key);
                    }
                    else if (shiftPressed)
                    {
                        Debug.WriteLine("Shift + " + key);
                    }

                    // shift and ctrl modifier 
                    if (key == Keys.ControlL || key == Keys.ControlR) {
                        controlPressed = true;
                    } else if (key == Keys.ShiftL || key == Keys.ShiftR) {
                        shiftPressed = true;
                    }

                }

            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static bool isModifierKey(Keys key) {
            return key == Keys.ControlL || key == Keys.AltL;
        }

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Virtual key codes enum
        private enum Keys : int
        {
            // Modifier keys
            AltL = 0xA4,
            AltR = 0xA5,
            ControlL = 0xA2,
            ControlR = 0xA3,
            ShiftL = 0xA0,
            ShiftR = 0xA1,
            WindowsL = 0x5B,
            WindowsR = 0x5C,

            // Function keys
            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B,

            // Alphanumeric keys
            A = 0x41,
            B = 0x42,
            C = 0x43,
            D = 0x44,
            E = 0x45,
            F = 0x46,
            G = 0x47,
            H = 0x48,
            I = 0x49,
            J = 0x4A,
            K = 0x4B,
            L = 0x4C,
            M = 0x4D,
            N = 0x4E,
            O = 0x4F,
            P = 0x50,
            Q = 0x51,
            R = 0x52,
            S = 0x53,
            T = 0x54,
            U = 0x55,
            V = 0x56,
            W = 0x57,
            X = 0x58,
            Y = 0x59,
            Z = 0x5A,

            // Mathematical keys
            Key_0 = 0x30,
            Key_1 = 0x31,
            Key_2 = 0x32,
            Key_3 = 0x33,
            Key_4 = 0x34,
            Key_5 = 0x35,
            Key_6 = 0x36,
            Key_7 = 0x37,
            Key_8 = 0x38,
            Key_9 = 0x39,
            Key_Minus = 0xBD,
            Key_Plus = 0xBB,

            // Numpad keys
            Num_NumLock = 0x90,
            Num_Divide = 0x6F,
            Num_Multiply = 0x6A,
            Num_Minus = 0x6D,
            Num_Plus = 0x6B,
            Num_0 = 0x60,
            Num_1 = 0x61,
            Num_2 = 0x62,
            Num_3 = 0x63,
            Num_4 = 0x64,
            Num_5 = 0x65,
            Num_6 = 0x66,
            Num_7 = 0x67,
            Num_8 = 0x68,
            Num_9 = 0x69,
            Num_Decimal = 0x6E,

            // Symbols
            Tilde = 0xC0,
            BracketL = 0xDB,
            BracketR = 0xDD,
            BackSlash = 0xDC,
            ForwardSlash = 0xBF,
            Semicolon = 0xBA,
            Quote = 0xDE,
            Comma = 0xBC,
            Period = 0xBE,

            // Other keys
            Space = 0x20,
            Enter = 0x0D,
            Tab = 0x09,
            Backspace = 0x08,
            Escape = 0x1B,
            Delete = 0x2E,
            Insert = 0x2D,
            Home = 0x24,
            End = 0x23,
            PageUp = 0x21,
            PageDown = 0x22,
            Left = 0x25,
            Up = 0x26,
            Right = 0x27,
            Down = 0x28,
            CapsLock = 0x14,
            
            // Random keys
            PrintScreen = 0x2C,
            ScrollLock = 0x91,
            PauseBreak = 0x13,
            WhoUsesThis = 0x5D
        }

    }
}