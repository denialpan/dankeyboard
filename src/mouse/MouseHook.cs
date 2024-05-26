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
using System.Windows.Forms;
using System.Windows.Controls;
using dankeyboard.src.history;

namespace dankeyboard.src.mouse {

    // mouse detection and saving data
    public class MouseHook {

        private const int WH_MOUSE_LL = 14;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;

        private static int _hookHandle = 0;
        private static HookProc _hookProc = HookCallback;
        private static Dictionary<MouseButton, int> mousePressCounts = new Dictionary<MouseButton, int>();
        private static List<ClickInformation> monitorClicks = new List<ClickInformation>();

        private static Grid kb = new Grid();

        // custom object to store data about a mouse coordinate on monitor and the monitor clicked on
        public class ClickInformation { 
            public double x; 
            public double y;
            public int monitor;
        }

        public void StartMouseHook(Grid keyboardGrid) {
            _hookHandle = SetHook(_hookProc);

            kb = keyboardGrid;

            // needs to randomly clear the console of all text, doesn't seem to be a way to set no text as default...?
            System.Windows.Controls.RichTextBox? historyConsole = kb.FindName("displayHistory") as System.Windows.Controls.RichTextBox;
            historyConsole.Document.Blocks.Clear();
            historyConsole.AppendText("\n");

            string filePath = "dankeyboard_data/mouse.csv";
            if (File.Exists(filePath)) {
                // read CSV file and populate dictionary
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines.Skip(1)) { // skip header of Mouse, Value
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
                
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                // current screen that was clicked on
                Screen currentScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                // identify screen clicked by index
                int monitorIndex = Array.IndexOf(Screen.AllScreens, currentScreen);

                // get relative X and Y positions in monitor's bounds
                double relativeX = Math.Round((double)(hookStruct.pt.x - currentScreen.Bounds.Left) / currentScreen.Bounds.Width, 2);
                double relativeY = Math.Round((double)(hookStruct.pt.y - currentScreen.Bounds.Top) / currentScreen.Bounds.Height, 2);

                Debug.WriteLine($"{relativeX}, {relativeY} on screen {monitorIndex}");

                ClickInformation clickInformation = new ClickInformation();
                clickInformation.x = relativeX;
                clickInformation.y = relativeY;
                clickInformation.monitor = monitorIndex;

                monitorClicks.Add(clickInformation);

                // get mouse button clicked
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
                } else {
                    mousePressCounts[buttonClicked] = 1;
                }

                HistoryConsole.updateConsole(kb, $"{buttonClicked} clicked");
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

            // bin filename
            filePath = Path.Combine(folderPath, "mouse_coordinates.bin");  

            // write monitor clicks to binary file
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Append))) {
                foreach (ClickInformation click in monitorClicks) {
                    writer.Write(click.x);
                    writer.Write(click.y);
                    writer.Write(click.monitor);
                }
            }
        }

        public Dictionary<MouseButton, int> getMousePressData() {
            return mousePressCounts;
        }

        public List<ClickInformation> getMonitorClicksData() {
            return monitorClicks;
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
