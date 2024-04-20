using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.CodeDom;

namespace dankeyboard.src.keyboard
{
    public class KeyboardHeatmap
    {

        private int totalKeyPresses = 0;
        public class DataItem {
            public string? Key { get; set; }
            public int Count { get; set; }
        }

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<Key, int> keys)
        {

            totalKeyPresses = 0;
            // get total number of key presses
            foreach (KeyValuePair<Key, int> key in keys)
            {
                totalKeyPresses += key.Value;
            }
            
            Label? totalKeys = keyboardGrid.FindName("keyboardPressesTotal") as Label;
            totalKeys.Content = $"Total keys: {totalKeyPresses}";

            List<DataItem> keyData = new List<DataItem>{};
            Slider? keyboardSlider = keyboardGrid.FindName("keyboardHeatmapSlider") as Slider;
            int heatmapStrength = (int)keyboardSlider.Value;

            // loop through all keys and assign colors and fill table
            foreach (KeyValuePair<Key, int> key in keys) {

                Rectangle? rectangle = keyboardGrid.FindName(key.Key.ToString()) as Rectangle;
                Debug.WriteLine(Math.Round(key.Value / (double)totalKeyPresses * heatmapStrength, 2));
                double percentage = Math.Round(key.Value / (double)totalKeyPresses * heatmapStrength, 2);
                Color color = (Color)ColorConverter.ConvertFromString(GenerateGradientColor("#FFFFFF", "#FF0000", percentage));

                if (rectangle != null) {
                    rectangle.Fill = new SolidColorBrush(color);
                }

                keyData.Add(new DataItem { Key = key.Key.ToString(), Count = key.Value });

            }

            ListView? listView = keyboardGrid.FindName("displayKeyboardData") as ListView;
            listView.ItemsSource = keyData;
        }

        private static string GenerateGradientColor(string color1Hex, string color2Hex, double percentage) {
            if (percentage < 0 || percentage > 0.5)
                percentage = 0.5;

            // Convert hex strings to Color objects
            Color color1 = (Color)ColorConverter.ConvertFromString(color1Hex);
            Color color2 = (Color)ColorConverter.ConvertFromString(color2Hex);

            byte r = (byte)(color1.R + (color2.R - color1.R) * percentage * 2);
            byte g = (byte)(color1.G + (color2.G - color1.G) * percentage * 2);
            byte b = (byte)(color1.B + (color2.B - color1.B) * percentage * 2);

            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
