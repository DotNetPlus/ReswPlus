using Microsoft.UI.Xaml.Controls;
using ReswPlusWinAppSDKSample.Pages;

namespace ReswPlusWinAppSDKSample;

public sealed partial class MainControl : UserControl
{
    public MainControl()
    {
        InitializeComponent();
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        _ = NavFrame.Navigate(typeof(StronglyTypedSamplePage), null);
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag != null)
        {
            var navItemTag = args.InvokedItemContainer.Tag.ToString();
            switch (navItemTag)
            {
                case "StronglyTyped":
                    {
                        _ = NavFrame.Navigate(typeof(StronglyTypedSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
                case "StringFormat":
                    {
                        _ = NavFrame.Navigate(typeof(StringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
                case "LiteralFormat":
                    {
                        _ = NavFrame.Navigate(typeof(LiteralStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        break;
                    }
                case "StringReferenceFormat":
                    {
                        _ = NavFrame.Navigate(typeof(ReferenceStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        break;
                    }
                case "MacroFormat":
                    {
                        _ = NavFrame.Navigate(typeof(MacroSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        break;
                    }
                case "NamedStringFormat":
                    {
                        _ = NavFrame.Navigate(typeof(NamedStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
                case "Pluralization":
                    {
                        _ = NavFrame.Navigate(typeof(BasicPluralizationSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
                case "AdvancedPluralization":
                    {
                        _ = NavFrame.Navigate(typeof(AdvancedPluralizationSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
                case "Variants":
                    {
                        _ = NavFrame.Navigate(typeof(VariantsSamplePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                    break;
            }
        }
    }
}
