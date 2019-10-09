// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using Windows.UI.Xaml;

namespace ReswPlusLib
{
    public static class TextBlock
    {
        public static string GetHtmlText(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlTextProperty);
        }

        public static void SetHtmlText(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlTextProperty, value);
        }

        public static DependencyProperty HtmlTextProperty { get; } = DependencyProperty.RegisterAttached("HtmlText", typeof(string), typeof(TextBlock), new PropertyMetadata(null, OnHtmlTextChanged));

        private static void OnHtmlTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is Windows.UI.Xaml.Controls.TextBlock textBlock)
            {
                textBlock.Inlines.Clear();
                var inlines = Html.HTMLParser.Parse((string)e.NewValue);
                foreach (var inline in inlines)
                {
                    textBlock.Inlines.Add(inline);
                }
            }
        }
    }
}
