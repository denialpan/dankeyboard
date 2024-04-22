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
            Loaded += StartDanKeyboard;
            Closed += CloseDanKeyboard;

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