using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using dankeyboard.src.keyboard;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private static KeyboardHeatmap ?keyboardHeatmap;
        private static KeyboardHook ?keyboardHook;

        private static Dictionary<Key, int> ?keyPresses;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

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

        }

        private void CloseDanKeyboard(object? sender, EventArgs e) {
            if (keyboardHook != null) {
                keyboardHook.CloseKeyboardHook();
            }
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(UpdateMouseLocation);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // Update every 100 milliseconds
            dispatcherTimer.Start();

        }

        private void UpdateMouseLocation(object ?sender, EventArgs e) {
            POINT point;
            if (GetCursorPos(out point)) {
                // Convert screen coordinates to WPF window coordinates
                //Point wpfPoint = PointFromScreen(new Point(point.X, point.Y));

                // Update the text block with mouse location
                Debug.WriteLine($"Mouse Location (Screen): {point.X}, {point.Y}");
            }
        }

    }
}