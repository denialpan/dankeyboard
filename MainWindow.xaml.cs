using System.Windows;
using System.Windows.Input;
using dankeyboard.src.keyboard;
using dankeyboard.src.mouse;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using dankeyboard.src.monitor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private static KeyboardHeatmap? keyboardHeatmap;
        private static KeyboardHook? keyboardHook;

        private static MouseHook? mouseHook;
        private static MonitorHeatmap? monitorHeatmap;
        private static MouseHeatmap? mouseHeatmap;

        private static Dictionary<Key, int>? keyPresses;
        private static Dictionary<KeyboardHook.Combination, int> combinationPresses;
        private static Dictionary<MouseButton, int>? mousePresses;


        public MainWindow()
        {
            InitializeComponent();

            // really quick fix for default values, will change later maybe
            checkboxGaussian.IsChecked = true;
            monitorDropdown.SelectedIndex = 0;

            // initialize data folder to fix crashing on fresh install
            string dataFolder = @".\dankeyboard_data";
            if (!Directory.Exists(dataFolder)) {
                Directory.CreateDirectory(dataFolder);
            }

            Loaded += StartDanKeyboard;
            Closed += CloseDanKeyboard;

        }

        // start all keyboard and mouse hooks
        private void StartDanKeyboard(object sender, RoutedEventArgs e) {

            keyboardHook = new KeyboardHook();
            keyboardHook.StartKeyboardHook(KeyboardMouseTab);
            keyPresses = keyboardHook.getKeyPressData();
            combinationPresses = keyboardHook.getCombinationData();

            keyboardHeatmap = new KeyboardHeatmap();
            keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);

            mouseHook = new MouseHook();
            mouseHook.StartMouseHook(KeyboardMouseTab);
            mousePresses = mouseHook.getMousePressData();

            mouseHeatmap = new MouseHeatmap();
            mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);

            monitorHeatmap = new MonitorHeatmap();
            monitorHeatmap.ColorHeatmap(MonitorTab);
            monitorHeatmap.getMonitors(monitorDropdown);
            
        }

        private void CloseDanKeyboard(object? sender, EventArgs e) {
            if (keyboardHook != null) {
                keyboardHook.CloseKeyboardHook();
            }
            if (mouseHook != null) {
                mouseHook.CloseMouseHook();
            }
        }

        // minimize to system tray when application is minimized
        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                Hide();
                notifyIcon.Visibility = Visibility.Visible; // show NotifyIcon in the system tray
            }

            base.OnStateChanged(e);
        }

        // show when NotifyIcon is double clicked
        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
            
            // refresh heatmaps and save data
            keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);
            mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);
            keyboardHook.SaveToCSV();
            mouseHook.SaveToCSV();

            Show();
            WindowState = WindowState.Normal;
            Activate();

        }

        // shut down option on NotifyIcon right click
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
            keyboardHook.SaveToCSV();
            mouseHook.SaveToCSV();
            System.Windows.Application.Current.Shutdown();
        }

        // sort keyboard table data
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

        // sort mouse table data
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

        // sort combination table data
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

        // on keyboard slider heatmap intensity change
        private void KeyboardSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (keyboardHeatmap != null && keyPresses != null) { 
                keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);
            }
        }
        // on mouse slider heatmap intensity change
        private void MouseSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (mouseHeatmap != null && mousePresses != null) {
                mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);
            }
        }

        // load heatmap
        private void buttonLoadHeatmap_Click(object sender, RoutedEventArgs e) {
            monitorHeatmap.ColorHeatmap(MonitorTab);
        }

        // options window
        private void Options_click(object sender, RoutedEventArgs e) {
            Options optionsWindow = new Options();
            optionsWindow.Closed += OptionsWindow_Closed;
            optionsWindow.ShowDialog();
        }

        // on options closed
        private void OptionsWindow_Closed(object sender, EventArgs e) {
            if (keyboardHeatmap != null && keyPresses != null) {
                keyboardHeatmap.ColorHeatmap(KeyboardMouseTab, keyPresses, combinationPresses);
            }
            if (mouseHeatmap != null && mousePresses != null) {
                mouseHeatmap.ColorHeatmap(KeyboardMouseTab, mousePresses);
            }
        }

        // open directory data folder
        private void OpenDataDirectory(object sender, RoutedEventArgs e) {
            string directoryPath = @"dankeyboard_data";

            try {
                Process.Start(new ProcessStartInfo {
                    FileName = directoryPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to open directory: {ex.Message}");
            }
        }
    }
}