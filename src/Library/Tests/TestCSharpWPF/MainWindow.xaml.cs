using ReswPlusLib;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace TestCSharpWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ResourceManager _resourceManager;

        public MainWindow()
        {
            _resourceManager = TestCSharpWPF.Resources.Resources.ResourceManager;

            InitializeComponent();
            CodeBehindSlider.Value = 4.3;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DistanceCodeBehindTextBlock == null)
            {
                return;
            }

            var pluralFormat = _resourceManager.GetPlural("RunDistance", e.NewValue);
            DistanceCodeBehindTextBlock.Text = string.Format(pluralFormat, e.NewValue);
        }
    }
}
