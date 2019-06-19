using ReswPlusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TestCSharpUWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            CodeBehindSlider.Value = 4.3;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (DistanceCodeBehindTextBlock == null)
                return;
            var pluralFormat = ResourceLoader.GetForCurrentView().GetPlural("RunDistance", e.NewValue);
            DistanceCodeBehindTextBlock.Text = string.Format(pluralFormat, e.NewValue);
        }
    }
}
