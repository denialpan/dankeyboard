using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace dankeyboard.src.keyboard
{
    public class KeyboardHeatmap
    {

        private int totalKeyPresses = 0;

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<Key, int> keys)
        {
            // get total number of key presses
            foreach (KeyValuePair<Key, int> key in keys)
            {
                totalKeyPresses += key.Value;
            }

            // loop through all keys and assign colors
            foreach (KeyValuePair<Key, int> key in keys)
            {

                Rectangle? rectangle = keyboardGrid.FindName(key.Key.ToString()) as Rectangle;
                Debug.WriteLine(Math.Round(key.Value / (double)totalKeyPresses * 10, 2));
                double percentage = Math.Round(key.Value / (double)totalKeyPresses * 10, 2);
                Color color = (Color)ColorConverter.ConvertFromString(GenerateGradientColor("#FFFFFF", "#FF0000", percentage));

                if (rectangle != null)
                {

                    rectangle.Fill = new SolidColorBrush(color);
                }

            }
        }

        private static string GenerateGradientColor(string color1Hex, string color2Hex, double percentage)
        {
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
