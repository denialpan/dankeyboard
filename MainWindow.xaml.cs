using dankeyboard.src;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dankeyboard
{
    public partial class MainWindow : Window {

        private KeyboardHeatmap kbhm;
        public MainWindow() {
            InitializeComponent();
            kbhm = new KeyboardHeatmap();

            // method loaded upon window open
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Call YourMethod when the window is loaded
            kbhm.testMethod();
        }
    }
}