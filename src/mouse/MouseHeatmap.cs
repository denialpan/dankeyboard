using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;


namespace dankeyboard.src.mouse {

    // display all mouse related data 
    // more detailed comments are found in KeyboardHeatmap.cs
    // this is a more simplified smaller version of that
    // i also don't want to repeat comments too
    public class MouseHeatmap {

        private int totalMousePresses = 0; // total mouse clicks from all clicks

        // custom object to store click data
        public class ClickItem {
            public string? MouseButton { get; set; }
            public int Count { get; set; }
            public string Percentage { get; set; }  
        }

        public void ColorHeatmap(Grid keyboardGrid, Dictionary<MouseButton, int> mouseButtons) {

            string configFilePath = @"dankeyboard_data\config.xml";
            string colorMin;
            string colorMax;

            if (File.Exists(configFilePath)) {
                XDocument config = XDocument.Load(configFilePath);
                colorMin = config.Root.Element("mouseMin").Value;
                colorMax = config.Root.Element("mouseMax").Value;
            } else {

                // create a new config file with default values
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

            // get total number of key presses
            totalMousePresses = 0;

            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {
                totalMousePresses += mb.Value;
            }

            Label? totalMouse = keyboardGrid.FindName("mousePressesTotal") as Label;
            totalMouse.Content = $"Total mouses: {totalMousePresses}";

            List<ClickItem> mouseData = new List<ClickItem> {};
            Slider? mouseSlider = keyboardGrid.FindName("mouseHeatmapSlider") as Slider;
            double heatmapStrength = mouseSlider.Value;

            foreach (KeyValuePair<MouseButton, int> mb in mouseButtons) {

                Button? button;
                double percentage = mb.Value / (double)totalMousePresses;
                Color color = (Color)ColorConverter.ConvertFromString(GenerateGradientColor(colorMin, colorMax, percentage * heatmapStrength));

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

                string p = (percentage * 100).ToString("0.000");
                mouseData.Add(new ClickItem { MouseButton = mb.Key.ToString(), Count = mb.Value, Percentage = $"{p}%" });
            }

            ListView? listView = keyboardGrid.FindName("displayMouseData") as ListView;
            listView.ItemsSource = mouseData.OrderByDescending(item => item.Count).ToList();
        }

        private static string GenerateGradientColor(string color1Hex, string color2Hex, double percentage) {
            if (percentage < 0 || percentage > 0.5)
                percentage = 0.5;

            Color color1 = (Color)ColorConverter.ConvertFromString(color1Hex);
            Color color2 = (Color)ColorConverter.ConvertFromString(color2Hex);

            byte r = (byte)(color1.R + (color2.R - color1.R) * percentage * 2);
            byte g = (byte)(color1.G + (color2.G - color1.G) * percentage * 2);
            byte b = (byte)(color1.B + (color2.B - color1.B) * percentage * 2);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

    }
}
