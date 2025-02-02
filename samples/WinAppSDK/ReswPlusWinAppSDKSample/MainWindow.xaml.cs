using Microsoft.UI.Xaml;

namespace ReswPlusWinAppSDKSample;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        AppWindow.SetIcon("Assets/StoreLogo.ico");
        this.InitializeComponent();
    }
}
