using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static dankeyboard.src.keyboard.KeyboardHeatmap;

namespace dankeyboard.src.mouse {
    public class MouseHeatmap {

        private int totalMousePresses = 0;

        public class DataItem {
            public string? MouseButton { get; set; }
            public int Count { get; set; }
            public string Percentage { get; set; }  
        }

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<MouseButton, int> mouseButtons) {
            // get total number of key presses

            totalMousePresses = 0;

            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {
                totalMousePresses += mb.Value;
            }

            Label? totalMouse = keyboardGrid.FindName("mousePressesTotal") as Label;
            totalMouse.Content = $"Total mouses: {totalMousePresses}";

            List<DataItem> mouseData = new List<DataItem> {};
            Slider? mouseSlider = keyboardGrid.FindName("mouseHeatmapSlider") as Slider;
            double heatmapStrength = mouseSlider.Value;

            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {

                Button? button;
                double percentage = Math.Round(mb.Value / (double)totalMousePresses, 2);
                Color color = (Color)ColorConverter.ConvertFromString(GenerateGradientColor("#FFFFFF", "#FF0000", percentage * heatmapStrength));

                switch (mb.Key.ToString()) {
                    case "Left":
                        button = keyboardGrid.FindName("M_Left") as Button;
                        button.Background = new SolidColorBrush(color);
                        break;
                    case "Middle":
                        button = keyboardGrid.FindName("M_Middle") as Button;
                        button.Background = new SolidColorBrush(color);
                        break;
                    case "Right":
                        button = keyboardGrid.FindName("M_Right") as Button;
                        button.Background = new SolidColorBrush(color);
                        break;
                    default:
                        break;
                }

                string p = (percentage * 100).ToString("0.00");
                mouseData.Add(new DataItem { MouseButton = mb.Key.ToString(), Count = mb.Value, Percentage = $"{p}%" });
            }

            ListView? listView = keyboardGrid.FindName("displayMouseData") as ListView;
            listView.ItemsSource = mouseData.OrderByDescending(item => item.Count).ToList();
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
