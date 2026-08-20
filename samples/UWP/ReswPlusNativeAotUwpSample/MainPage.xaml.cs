using Windows.UI.Xaml.Controls;

// A Page already has a Resources property of its own, so the generated class is aliased rather than imported.
using AppStrings = ReswPlusNativeAotUwpSample.Strings.Resources;

namespace ReswPlusNativeAotUwpSample
{
    /// <summary>
    /// Shows what ReswPlus generates for a UWP project built on modern .NET.
    /// </summary>
    /// <remarks>
    /// Everything read here is generated from <c>Strings\en-US\Resources.resw</c>: the members don't exist until
    /// the resource does, so a renamed or removed resource is a build error rather than an empty string at
    /// runtime.
    /// </remarks>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // A plain resource, as a strongly typed property.
            StronglyTyped.Text = AppStrings.StronglyTypedExample;

            // A '#Format' tag turns the resource into a method taking the parameters it declares.
            Formatted.Text = AppStrings.Welcome("Rudy", 3);

            // A pluralized resource picks its form with the plural rules of the language, and the '_None' form
            // is used when the count is zero.
            NoItem.Text = AppStrings.Pictures(0);
            OneItem.Text = AppStrings.Pictures(1);
            SeveralItems.Text = AppStrings.Pictures(5);
        }
    }
}
