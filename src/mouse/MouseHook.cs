using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;

namespace dankeyboard.src.mouse {
    public class MouseHook {

        private const int WH_MOUSE_LL = 14;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;

        private static int _hookHandle = 0;
        private static HookProc _hookProc = HookCallback;
        private static Dictionary<MouseButton, int> mousePressCounts = new Dictionary<MouseButton, int>();

        public void StartMouseHook() {
            _hookHandle = SetHook(_hookProc);

            string filePath = "dankeyboard_data/mouse.csv";
            if (File.Exists(filePath)) {
                // Read CSV file and populate dictionary
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && Enum.TryParse(parts[0], out MouseButton mb) && int.TryParse(parts[1], out int count)) {
                        mousePressCounts[mb] = count;
                    }
                }
            }
        }

        public void CloseMouseHook() {
            UnhookWindowsHookEx(_hookHandle);
            SaveToCSV();
        }

        private static int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN || wParam == (IntPtr)WM_MBUTTONDOWN)) {
                // Extracting mouse coordinates from lParam
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int x = hookStruct.pt.x;
                int y = hookStruct.pt.y;

                // Determine which mouse button was clicked
                MouseButton buttonClicked;
                switch ((int)wParam) {
                    case WM_LBUTTONDOWN:
                        buttonClicked = MouseButton.Left;
                        break;
                    case WM_RBUTTONDOWN:
                        buttonClicked = MouseButton.Right;
                        break;
                    case WM_MBUTTONDOWN:
                        buttonClicked = MouseButton.Middle;
                        break;
                    default:
                        buttonClicked = MouseButton.Left;
                        break;
                }

                if (mousePressCounts.ContainsKey(buttonClicked)) {
                    mousePressCounts[buttonClicked]++;
                    Debug.WriteLine(mousePressCounts[buttonClicked]);
                } else {
                    mousePressCounts[buttonClicked] = 1;
                    Debug.WriteLine(mousePressCounts[buttonClicked]);

                }

                Debug.WriteLine($"Mouse button {buttonClicked} clicked at ({x}, {y})");
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }
        public void SaveToCSV() {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("KeyCode,Count");
            foreach (var kvp in mousePressCounts) {
                csvContent.AppendLine($"{kvp.Key},{kvp.Value}");
            }

            string folderPath = "dankeyboard_data";
            string fileName = "mouse.csv";
            string filePath = Path.Combine(folderPath, fileName);
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, csvContent.ToString());
        }

        public Dictionary<MouseButton, int> getMousePressData() {
            return mousePressCounts;
        }

        // Structure for mouse hook data
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        // Structure for mouse point
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int x;
            public int y;
        }

        private int SetHook(HookProc proc) {
            IntPtr hInstance = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            return SetWindowsHookEx(WH_MOUSE_LL, proc, hInstance, 0);
        }

        // Importing necessary functions from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Delegate for the hook procedure
        private delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    }
}
