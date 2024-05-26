using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using static dankeyboard.src.mouse.MouseHook;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Windows.Forms;

namespace dankeyboard.src.monitor {

    public class MonitorHeatmap {

        private static MonitorInformation[] monitors;

        public class MonitorInformation { 
        
            public int? ID { get; set; }
            public string Name { get; set; }

            public bool Primary { get; set; }
        
        }

        public void ColorHeatmap(Grid monitorGrid) {

            System.Windows.Controls.ComboBox? monitorDropdown = monitorGrid.FindName("monitorDropdown") as System.Windows.Controls.ComboBox;
            System.Windows.Controls.CheckBox? checkboxGaussian = monitorGrid.FindName("checkboxGaussian") as System.Windows.Controls.CheckBox;
            Slider deviation = monitorGrid.FindName("sliderDeviation") as Slider;
            Slider sigma = monitorGrid.FindName("sliderSigma") as Slider;


            PlotView? plot = monitorGrid.FindName("displayPlot") as PlotView;

            // resolution of heatmap array
            int xSize = 160;
            int ySize = 90;
            double[,] data = new double[xSize, ySize];

            // open bin file
            string folderPath = "dankeyboard_data";
            string fileName = "mouse_coordinates.bin";
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath)) {

                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open))) {

                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        ClickInformation click = new ClickInformation();
                        click.x = reader.ReadDouble();
                        click.y = reader.ReadDouble();
                        click.monitor = reader.ReadInt32();

                        // is only first monitor
                        if (click.monitor == monitorDropdown.SelectedIndex) { 
                            data[(int)(click.x * (xSize - 1)), (int)(click.y * (ySize - 1))]++;
                        }
                    }
                }
            }

            if (checkboxGaussian.IsChecked == true) { 
                data = GaussianBlur.ApplyGaussianFilter(data, deviation.Value, sigma.Value);
            }

            SetPlotView(plot, data, monitorDropdown.SelectedIndex);        

        }

        private static void SetPlotView(PlotView plot, double[,] data, int monitor) {

            // heatmap data properties
            HeatMapSeries heatMapSeries = new HeatMapSeries {
                X0 = 0.0,
                X1 = 1.0,
                Y0 = 0.0,
                Y1 = 1.0,
                RenderMethod = HeatMapRenderMethod.Bitmap
            };

            heatMapSeries.Data = data;

            // plot colors
            OxyColor[] baseColors = {
                OxyColors.DarkBlue,
                OxyColors.Cyan,
                OxyColors.Yellow,
                OxyColors.Orange,
                OxyColors.DarkRed
            };

            var colorAxis = new LinearColorAxis {
                Position = AxisPosition.Right,
                Palette = OxyPalette.Interpolate(200, baseColors)
            };

            PlotModel plotModel = new PlotModel();

            if (monitor == -1) {
                plotModel.Title = "Select a monitor";
            } else {
                plotModel.Title = $"Monitor {monitor + 1}";
            }

            plotModel.Series.Add(heatMapSeries);
            plotModel.Axes.Add(colorAxis);

            LinearAxis xAxisSettings = new LinearAxis {
                Position = AxisPosition.Top,
                Minimum = 0,
                Maximum = 1,
                IsPanEnabled = false,
                IsZoomEnabled = false,
            };

            LinearAxis yAxisSettings = new LinearAxis {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 1,
                StartPosition = 1,
                EndPosition = 0,
                IsPanEnabled = false,
                IsZoomEnabled = false,
            };

            plotModel.Axes.Add(xAxisSettings);
            plotModel.Axes.Add(yAxisSettings);

            plot.Model = plotModel;

        }

        public void getMonitors(System.Windows.Controls.ComboBox monitorDropdown) {

            // Get the list of screens/monitors
            Screen[] screens = Screen.AllScreens;
            monitors = new MonitorInformation[screens.Length];

            // Output the name of each monitor
            for (int i = 0; i < screens.Length; i++) {
                Screen screen = screens[i];
                if (screen.Primary) {
                    monitors[i] = new MonitorInformation { ID = i, Name = $"{screen.DeviceName} (primary)", Primary = true };
                    Debug.WriteLine($"{screen.DeviceName} (primary)");
                } else {
                    monitors[i] = new MonitorInformation { ID = i, Name = screen.DeviceName, Primary = false };
                    Debug.WriteLine(screen.DeviceName);
                }
            }

            monitorDropdown.Items.Clear();
            foreach (MonitorInformation monitor in monitors) {
                monitorDropdown.Items.Add(monitor.Name);
            }

        }

    }

}
