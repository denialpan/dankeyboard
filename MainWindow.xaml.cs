using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using dankeyboard.src.keyboard;
using dankeyboard.src.mouse;
using Hardcodet.Wpf.TaskbarNotification;
using System.ComponentModel;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private static KeyboardHeatmap? keyboardHeatmap;
        private static KeyboardHook? keyboardHook;

        private static MouseHook? mouseHook;

        private static Dictionary<Key, int>? keyPresses;
        private static Dictionary<MouseButton, int>? mousePresses;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public MainWindow()
        {
            InitializeComponent();


            Loaded += StartDanKeyboard;
            Closed += CloseDanKeyboard;

        }

        private void StartDanKeyboard(object sender, RoutedEventArgs e) {

            keyboardHook = new KeyboardHook();
            keyboardHook.StartKeyboardHook();
            keyPresses = keyboardHook.getKeyPressData();

            keyboardHeatmap = new KeyboardHeatmap();
            keyboardHeatmap.ColorHeatmap(KeyboardTab, keyPresses);

            mouseHook = new MouseHook();
            mouseHook.StartMouseHook();
            mousePresses = mouseHook.getMousePressData();

            MouseHeatmap mouseHeatmap = new MouseHeatmap();
            mouseHeatmap.ColorHeatmap(KeyboardTab, mousePresses);

        }

        private void CloseDanKeyboard(object? sender, EventArgs e) {
            if (keyboardHook != null) {
                keyboardHook.CloseKeyboardHook();
            }
            if (mouseHook != null) {
                mouseHook.CloseMouseHook();
            }
        }

        // Minimize to system tray when application is minimized.
        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                Hide(); // Hide the main window
                notifyIcon.Visibility = Visibility.Visible; // Show the NotifyIcon in the system tray
            }

            base.OnStateChanged(e);
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
            // Show the main window when the NotifyIcon is double-clicked
            Show();
            WindowState = WindowState.Normal;
            Activate(); // Bring the window to the front
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
            // Close the application when the "Close" menu item is clicked
            Application.Current.Shutdown();
        }
    }
}