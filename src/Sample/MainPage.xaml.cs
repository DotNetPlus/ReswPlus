// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using ReswPlusSample.Pages;
using Windows.UI.Xaml.Controls;


namespace ReswPlusSample
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            NavFrame.Navigate(typeof(StronglyTypedSamplePage), null);
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
                            NavFrame.Navigate(typeof(StronglyTypedSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                    case "StringFormat":
                        {
                            NavFrame.Navigate(typeof(StringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                    case "LiteralFormat":
                        {
                            NavFrame.Navigate(typeof(LiteralStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                            break;
                        }
                    case "StringReferenceFormat":
                        {
                            NavFrame.Navigate(typeof(ReferenceStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                            break;
                        }
                    case "MacroFormat":
                        {
                            NavFrame.Navigate(typeof(MacroSamplePage), null, args.RecommendedNavigationTransitionInfo);
                            break;
                        }
                    case "NamedStringFormat":
                        {
                            NavFrame.Navigate(typeof(NamedStringFormatSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                    case "Pluralization":
                        {
                            NavFrame.Navigate(typeof(BasicPluralizationSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                    case "AdvancedPluralization":
                        {
                            NavFrame.Navigate(typeof(AdvancedPluralizationSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                    case "Variants":
                        {
                            NavFrame.Navigate(typeof(VariantsSamplePage), null, args.RecommendedNavigationTransitionInfo);
                        }
                        break;
                }
            }
        }
    }
}
