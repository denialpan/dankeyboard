using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Xml.Linq;

namespace dankeyboard {
    public partial class Options : Window {

        private const string configFilePath = @"dankeyboard_data\config.xml";
        public Options() {
            
            InitializeComponent();
            XDocument config = readConfigFile();

            colorPickerKeyboardMin.SelectedColor = (Color)ColorConverter.ConvertFromString(config.Root.Element("keyboardMin").Value);
            colorPickerKeyboardMax.SelectedColor = (Color)ColorConverter.ConvertFromString(config.Root.Element("keyboardMax").Value);
            colorPickerMouseMin.SelectedColor = (Color)ColorConverter.ConvertFromString(config.Root.Element("mouseMin").Value);
            colorPickerMouseMax.SelectedColor = (Color)ColorConverter.ConvertFromString(config.Root.Element("mouseMax").Value); 
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e) {

            WriteToConfigFile("keyboardMin", $"{colorPickerKeyboardMin.SelectedColor}");
            WriteToConfigFile("keyboardMax", $"{colorPickerKeyboardMax.SelectedColor}");
            WriteToConfigFile("mouseMin", $"{colorPickerMouseMin.SelectedColor}");
            WriteToConfigFile("mouseMax", $"{colorPickerMouseMax.SelectedColor}");

            Close();
        }

        private XDocument readConfigFile() {
            if (File.Exists(configFilePath)) {
                return XDocument.Load(configFilePath);
            } else {
                // create config file with default values
                XDocument newConfig = new XDocument(
                    new XElement("Configuration",
                        new XElement("keyboardMin", "#FFFFFF"),
                        new XElement("mouseMin", "#FFFFFF"),
                        new XElement("keyboardMax", "#FF0000"),
                        new XElement("mouseMax", "#FF0000")
                    )
                );
                newConfig.Save(configFilePath);
                return newConfig;
            }
        }

        private void WriteToConfigFile(string settingName, string settingValue) {
            XDocument config = readConfigFile();

            XElement settingElement = config.Root.Element(settingName);
            if (settingElement == null) {
                settingElement = new XElement(settingName, settingValue);
                config.Root.Add(settingElement);
            } else {
                settingElement.Value = settingValue;
            }

            config.Save(configFilePath);
        }
    }
}
