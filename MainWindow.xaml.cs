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
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private static KeyboardHeatmap? keyboardHeatmap;
        private static KeyboardHook? keyboardHook;

        private static MouseHook? mouseHook;
        private static MouseHeatmap? mouseHeatmap;


        private static Dictionary<Key, int>? keyPresses;
        private static Dictionary<KeyboardHook.Combination, int> combinationPresses;
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

            GenerateHeatmap();

            Loaded += StartDanKeyboard;
            Closed += CloseDanKeyboard;

        }

        private void GenerateHeatmap() {
            // Create a writable bitmap with desired dimensions
            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;
            WriteableBitmap heatmapBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Apply heatmap effect based on your data points
            ApplyHeatmapEffect(heatmapBitmap);

            // Display the heatmap image
            heatmapImage.Source = heatmapBitmap;
        }

        private void ApplyHeatmapEffect(WriteableBitmap bitmap) {
            // Fill the background with gray color
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            for (int y = 0; y < bitmap.PixelHeight; y++) {
                for (int x = 0; x < bitmap.PixelWidth; x++) {
                    int index = (y * stride) + (x * 4);
                    pixels[index] = 0x80; // Blue (50% intensity)
                    pixels[index + 1] = 0x80; // Green (50% intensity)
                    pixels[index + 2] = 0x80; // Red (50% intensity)
                    pixels[index + 3] = 0xFF; // Alpha
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);

            // Draw dots on top of the gray background
            int dotSize = 5; // Size of the dots
            for (int i = 0; i < 5; i++) {
                int centerX = 50 * (i + 1); // X-coordinate of the center of the dot
                int centerY = 100; // Y-coordinate of the center of the dot

                // Loop through the area of the dot to set pixels
                for (int y = centerY - dotSize / 2; y < centerY + dotSize / 2; y++) {
                    for (int x = centerX - dotSize / 2; x < centerX + dotSize / 2; x++) {
                        if (x >= 0 && x < bitmap.PixelWidth && y >= 0 && y < bitmap.PixelHeight) {
                            int index = (y * stride) + (x * 4);
                            pixels[index] = 0xFF; // Blue
                            pixels[index + 1] = 0x00; // Green
                            pixels[index + 2] = 0x00; // Red
                            pixels[index + 3] = 0xFF; // Alpha
                        }
                    }
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);
        }

        private void StartDanKeyboard(object sender, RoutedEventArgs e) {

            keyboardHook = new KeyboardHook();
            keyboardHook.StartKeyboardHook();
            keyPresses = keyboardHook.getKeyPressData();
            combinationPresses = keyboardHook.getCombinationData();

            keyboardHeatmap = new KeyboardHeatmap();
            keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);

            mouseHook = new MouseHook();
            mouseHook.StartMouseHook();
            mousePresses = mouseHook.getMousePressData();

            mouseHeatmap = new MouseHeatmap();
            mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);

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
            //refresh heatmaps and save data
            keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);
            mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);
            keyboardHook.SaveToCSV();
            mouseHook.SaveToCSV();

            Show();
            WindowState = WindowState.Normal;
            Activate(); // Bring the window to the front
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
            keyboardHook.SaveToCSV();
            mouseHook.SaveToCSV();
            Application.Current.Shutdown();
        }

        private void SortKeyboardData(object sender, RoutedEventArgs e) {

            var columnHeader = sender as GridViewColumnHeader;
            string? columnName = columnHeader?.Content as string;

            if (!string.IsNullOrEmpty(columnName)) {
                ListSortDirection newSortDirection = ListSortDirection.Ascending;
                if (columnHeader.Tag != null && (ListSortDirection)columnHeader.Tag == ListSortDirection.Ascending) {
                    newSortDirection = ListSortDirection.Descending;
                }

                columnHeader.Tag = newSortDirection;

                ICollectionView view = CollectionViewSource.GetDefaultView(displayKeyboardData.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(columnName, newSortDirection));
            }
        }

        private void SortMouseData(object sender, RoutedEventArgs e) {
            var columnHeader = sender as GridViewColumnHeader;
            string? columnName = columnHeader?.Content as string;

            if (!string.IsNullOrEmpty(columnName)) {
                ListSortDirection newSortDirection = ListSortDirection.Ascending;
                if (columnHeader.Tag != null && (ListSortDirection)columnHeader.Tag == ListSortDirection.Ascending) {
                    newSortDirection = ListSortDirection.Descending;
                }

                columnHeader.Tag = newSortDirection;

                ICollectionView view = CollectionViewSource.GetDefaultView(displayMouseData.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(columnName, newSortDirection));
            }
        }

        private void SortCombinationData(object sender, RoutedEventArgs e) {
            var columnHeader = sender as GridViewColumnHeader;
            string? columnName = columnHeader?.Content as string;

            if (!string.IsNullOrEmpty(columnName)) {
                ListSortDirection newSortDirection = ListSortDirection.Ascending;
                if (columnHeader.Tag != null && (ListSortDirection)columnHeader.Tag == ListSortDirection.Ascending) {
                    newSortDirection = ListSortDirection.Descending;
                }

                columnHeader.Tag = newSortDirection;

                ICollectionView view = CollectionViewSource.GetDefaultView(displayCombinationData.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(columnName, newSortDirection));
            }
        }

        private void KeyboardSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (keyboardHeatmap != null && keyPresses != null) { 
                keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);
            }
        }

        private void MouseSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (mouseHeatmap != null && mousePresses != null) {
                mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);
            }
        }
    }
}