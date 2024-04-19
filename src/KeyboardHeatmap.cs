using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace dankeyboard.src
{
    public class KeyboardHeatmap {

        public void color_heatmap(Grid keyboardGrid) {

            Color color = (Color)ColorConverter.ConvertFromString("#0077FF");

            foreach (var rect in keyboardGrid.Children) {

                if (rect is FrameworkElement frameworkElement && frameworkElement is Rectangle)
                {
                    Rectangle rectangle = (Rectangle)frameworkElement;

                    switch (rectangle.Name) {

                        case "Key_A":
                            rectangle.Fill = new SolidColorBrush(color);
                            break;
                        case "Key_1":
                            rectangle.Fill = new SolidColorBrush(color);
                            break;
                        default:
                            break;

                    }

                }

            }

        }

    }
}
