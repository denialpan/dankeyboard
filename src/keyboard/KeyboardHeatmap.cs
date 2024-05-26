using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Xml.Linq;
using System.IO;

namespace dankeyboard.src.keyboard {

    // display all keyboard related data
    public class KeyboardHeatmap {

        private int totalKeyPresses = 0; // total key presses from all key input
        private int totalCombinationPresses = 0; // total key combinations from all combination input (i.e. Shift + A, Ctrl + Z)
        private Grid keyboardGridGlobal; // global keyboard UI element passed in

        // custom object to store data about a key
        public class KeyItemData {

            public string? Key { get; set; }
            public int? Count { get; set; }
            public string? Percentage { get; set; }
        }

        // custom object to store data about a combination
        public class CombinationItemData { 
            public string? Combination { get; set; }
            public int? Count { get; set; }
            public string? Percentage { get; set; }
        }

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<Key, int> keys, Dictionary<KeyboardHook.Combination, int> combinations) {

            // load config
            string configFilePath = @"dankeyboard_data\config.xml";
            string colorMin;
            string colorMax;

            if (File.Exists(configFilePath)) {

                XDocument config = XDocument.Load(configFilePath);
                colorMin = config.Root.Element("keyboardMin").Value;
                colorMax = config.Root.Element("keyboardMax").Value;

            } else {

                XDocument newConfig = new XDocument(
                    new XElement("Configuration",
                        new XElement("keyboardMin", "#FFFFFF"),
                        new XElement("mouseMin", "#FFFFFF"),
                        new XElement("keyboardMax", "#FF0000"),
                        new XElement("mouseMax", "#FF0000")
                    )
                );

                colorMin = "#FFFFFF";
                colorMax = "#FF0000";
                newConfig.Save(configFilePath);
            }

            totalKeyPresses = 0;
            totalCombinationPresses = 0;
            keyboardGridGlobal = keyboardGrid;

            // get total number of key presses
            foreach (KeyValuePair<Key, int> key in keys) {
                totalKeyPresses += key.Value;
            }

            // get total number of combinations
            foreach (KeyValuePair<KeyboardHook.Combination, int> combination in combinations) {
                totalCombinationPresses += combination.Value;
            }
            
            // set UI Label text
            Label? totalKeys = keyboardGrid.FindName("keyboardPressesTotal") as Label;
            totalKeys.Content = $"Total keys: {totalKeyPresses}";
            Label? totalCombinations = keyboardGrid.FindName("combinationPressesTotal") as Label;
            totalCombinations.Content = $"Total combinations: {totalCombinationPresses}";

            // get keyboard heatmap strength from UI Slider
            Slider? keyboardSlider = keyboardGrid.FindName("keyboardHeatmapSlider") as Slider;
            int heatmapStrength = (int)keyboardSlider.Value;


            // loop through all keys and assign colors and fill tables
            List<KeyItemData> keyData = new List<KeyItemData>{};
            List<CombinationItemData> combinationData = new List<CombinationItemData>();

            foreach (KeyValuePair<Key, int> key in keys) {

                // find UI Button to set fill color
                Button? butt = keyboardGrid.FindName(key.Key.ToString()) as Button;

                Debug.WriteLine(Math.Round(key.Value / (double)totalKeyPresses * heatmapStrength, 2));
                double percentage = key.Value / (double)totalKeyPresses;
                Color color = (Color)ColorConverter.ConvertFromString(generateGradientColor(colorMin, colorMax, percentage * heatmapStrength));

                if (butt != null) {
                    butt.Background = new SolidColorBrush(color);
                    butt.Click += scrollToKey;
                }

                // fill in table with KeyItemData
                string p = (percentage * 100).ToString("0.000");
                keyData.Add(new KeyItemData { Key = key.Key.ToString(), Count = key.Value, Percentage = $"{p}%" });

            }

            // loop through all combinations and fill table
            foreach (KeyValuePair<KeyboardHook.Combination, int> combination in combinations) {

                Debug.WriteLine(Math.Round(combination.Value / (double)totalCombinationPresses * heatmapStrength, 2));
                double percentage = combination.Value / (double)totalCombinationPresses * 100;

                // fill in table with CombinationItemData
                string p = percentage.ToString("0.000");
                combinationData.Add(new CombinationItemData { Combination = combination.Key.ToString(), Count = combination.Value, Percentage = $"{p}%" });

            }

            // set content of keys and combinations table
            ListView? keyboardListView = keyboardGrid.FindName("displayKeyboardData") as ListView;
            keyboardListView.ItemsSource = keyData.OrderByDescending(item => item.Count).ToList();
            ListView? combinationListView = keyboardGrid.FindName("displayCombinationData") as ListView;
            combinationListView.ItemsSource = combinationData.OrderByDescending(item => item.Count).ToList();

        }

        // method to scroll to key in table depending on clicked UI Button
        private void scrollToKey(object sender, RoutedEventArgs e) {

            ListView keyList = keyboardGridGlobal.FindName("displayKeyboardData") as ListView;
            Button button = sender as Button;
            foreach (KeyItemData item in keyList.Items) {
                if (item.Key.Equals(button.Name)) {
                    keyList.ScrollIntoView(item);
                    keyList.SelectedItem = item;

                }
            }

        }

        // generate fill color based on percentage
        private static string generateGradientColor(string color1Hex, string color2Hex, double percentage) {
            if (percentage < 0 || percentage > 0.5)
                percentage = 0.5;

            // convert hex strings to Color objects
            Color color1 = (Color)ColorConverter.ConvertFromString(color1Hex);
            Color color2 = (Color)ColorConverter.ConvertFromString(color2Hex);

            byte r = (byte)(color1.R + (color2.R - color1.R) * percentage * 2);
            byte g = (byte)(color1.G + (color2.G - color1.G) * percentage * 2);
            byte b = (byte)(color1.B + (color2.B - color1.B) * percentage * 2);

            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
