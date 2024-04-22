using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;
using System.Xml.Linq;

namespace dankeyboard.src.keyboard {
    public class KeyboardHook {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static IntPtr hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc proc = HookCallback;

        private DispatcherTimer? keyboardTimer;
        private static HashSet<Key> pressedKeys = new HashSet<Key>();
        private static Dictionary<Key, int> keyPressCounts = new Dictionary<Key, int>();
        private static Dictionary<Combination, int> combinationCounts = new Dictionary<Combination, int>();
        public class Combination : IEquatable<Combination> { 

            public string? Key { get; set; }
            public string? Modifier {  get; set; }

            public override int GetHashCode() {
                unchecked {
                    int hash = 17;
                    hash = hash * 23 + (Key != null ? Key.GetHashCode() : 0);
                    hash = hash * 23 + (Modifier != null ? Modifier.GetHashCode() : 0);
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return Equals(obj as Combination);
            }

            public bool Equals(Combination other) {
                if (other == null)
                    return false;

                return (Key == other.Key && Modifier == other.Modifier);
            }
            public override string ToString() {
                return $"{Modifier} + {Key}";
            }

        }


        public void StartKeyboardHook() {

            hookId = SetHook(proc);

            keyboardTimer = new DispatcherTimer();
            keyboardTimer.Interval = TimeSpan.FromMilliseconds(50);
            keyboardTimer.Tick += DetectTabAlt;
            keyboardTimer.Start();

            string filePath = "dankeyboard_data/keys.csv";
            if (File.Exists(filePath)) {
                // Read CSV file and populate dictionary
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && Enum.TryParse(parts[0], out Key key) && int.TryParse(parts[1], out int count)) {
                        keyPressCounts[key] = count;
                    }
                }
            }

            filePath = "dankeyboard_data/combination.csv";
            if (File.Exists(filePath)) {
                // Read CSV file and populate dictionary
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && parts[0] is string && int.TryParse(parts[1], out int count)) {

                        string entry = parts[0];

                        var keys = entry.Split(' ');
                        Debug.WriteLine(entry);

                        Combination duh = new Combination { Key = keys[1], Modifier = keys[0] };
                        combinationCounts[duh] = count;

                    }
                }
            }

        }

        public void CloseKeyboardHook() {
            
            if (keyboardTimer != null) {
                keyboardTimer.Stop();
            }
            SaveToCSV();
        }

        public Dictionary<Key, int> getKeyPressData() { 
            return keyPressCounts;
        }

        public Dictionary<Combination, int> getCombinationData() {
            return combinationCounts;
        }

        // this method primarily exists to detect when the alt tab functionality is detected
        // it does not detect the combination as one, but the keys individually
        // decent workaround, since alt is finnicky to deal with
        private void DetectTabAlt(object? sender, EventArgs e) {


            // alt key
            switch ((GetAsyncKeyState(0x12) & 0x8000)) {
                case 0:
                    pressedKeys.Remove(KeyInterop.KeyFromVirtualKey(0x12));
                    break;
                default:
                    if (!pressedKeys.Contains(KeyInterop.KeyFromVirtualKey(0x12))) {
                        ModifierKeys modifierKeys = Keyboard.Modifiers & ~ModifierKeys.Alt;
                        Key altKey = KeyInterop.KeyFromVirtualKey(0x12);
                        pressedKeys.Add(altKey);
                        if (keyPressCounts.ContainsKey(altKey)) {
                            keyPressCounts[altKey]++;
                            Debug.WriteLine(keyPressCounts[altKey]);
                        } else {
                            keyPressCounts[altKey] = 1;
                            Debug.WriteLine(keyPressCounts[altKey]);

                        }
                        Debug.WriteLine($"Key: {altKey}, Modifiers: {modifierKeys}");
                    }
                    break;

            }

            // tab key
            switch ((GetAsyncKeyState(0x09) & 0x8000)) {
                case 0:
                    pressedKeys.Remove(KeyInterop.KeyFromVirtualKey(0x09));
                    break;
                default:
                    if (!pressedKeys.Contains(KeyInterop.KeyFromVirtualKey(0x09))) {
                       
                        ModifierKeys modifierKeys = Keyboard.Modifiers & ~ModifierKeys.Alt;
                        Key tabKey = KeyInterop.KeyFromVirtualKey(0x09);
                        pressedKeys.Add(tabKey);

                        if (keyPressCounts.ContainsKey(tabKey)) {
                            keyPressCounts[tabKey]++;
                            Debug.WriteLine(keyPressCounts[tabKey]);

                        } else {
                            keyPressCounts[tabKey] = 1;
                            Debug.WriteLine(keyPressCounts[tabKey]);

                        }
                        Debug.WriteLine($"Key: {tabKey}, Modifiers: {modifierKeys}");
                    }
                    break;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (pressedKeys.Contains(key)) {
                    pressedKeys.Remove(key);
                }

            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {

                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                ModifierKeys modifierKeys = Keyboard.Modifiers & ~ModifierKeys.Alt;

                if (!pressedKeys.Contains(key) && key != Key.Tab) {

                    if (keyPressCounts.ContainsKey(key)) {
                        keyPressCounts[key]++;
                    } else {
                        keyPressCounts[key] = 1;
                    }
                    pressedKeys.Add(key);

                    if (modifierKeys != ModifierKeys.None) {

                        Debug.WriteLine(modifierKeys.ToString());
                        Combination c = new Combination { Key = key.ToString(), Modifier = modifierKeys.ToString() };

                        if (combinationCounts.ContainsKey(c)) {
                            combinationCounts[c]++;
                            Debug.Write(combinationCounts[c]);
                        } else {
                            combinationCounts[c] = 1;
                            Debug.Write(combinationCounts[c]);

                        }


                    }

                    Debug.WriteLine($"Key: {key}, Modifiers: {modifierKeys}");
                }

            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public void SaveToCSV() {

            StringBuilder csvContent = new StringBuilder();
            string folderPath = "dankeyboard_data";
            string fileName;
            string filePath;

            // only key csv
            fileName = "keys.csv";
            filePath = Path.Combine(folderPath, fileName);
            csvContent.AppendLine("KeyCode,Count");
            foreach (var kvp in keyPressCounts) {
                csvContent.AppendLine($"{kvp.Key},{kvp.Value}");
            }
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, csvContent.ToString());

            // key combination csv
            csvContent = new StringBuilder();
            fileName = "combination.csv";
            filePath = Path.Combine(folderPath, fileName);
            csvContent.AppendLine("Combination,Count");
            foreach (var c in combinationCounts) {
                csvContent.AppendLine($"{c.Key.Modifier} {c.Key.Key},{c.Value}");
            }
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, csvContent.ToString());

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            UnhookWindowsHookEx(hookId);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc) {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                if (curModule != null) { 
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(lpModuleName: curModule.ModuleName), 0);
                }
                return IntPtr.Zero; 
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

    }
}
