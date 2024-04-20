﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace dankeyboard.src.mouse {
    public class MouseHeatmap {

        private int totalMousePresses = 0;

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<MouseButton, int> mouseButtons) {
            // get total number of key presses
            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {
                totalMousePresses += mb.Value;
            }

            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {

                Rectangle? rectangle;
                double percentage = Math.Round(mb.Value / (double)totalMousePresses * 5, 2);
                Debug.WriteLine(percentage);
                Color color = (Color)ColorConverter.ConvertFromString(GenerateGradientColor("#FFFFFF", "#FF0000", percentage));

                switch (mb.Key.ToString()) {
                    case "Left":
                        rectangle = keyboardGrid.FindName("M_Left") as Rectangle;
                        rectangle.Fill = new SolidColorBrush(color);
                        break;
                    case "Middle":
                        rectangle = keyboardGrid.FindName("M_Middle") as Rectangle;
                        rectangle.Fill = new SolidColorBrush(color);
                        break;
                    case "Right":
                        rectangle = keyboardGrid.FindName("M_Right") as Rectangle;
                        rectangle.Fill = new SolidColorBrush(color);
                        break;
                    default:
                        break;
                }
            }
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