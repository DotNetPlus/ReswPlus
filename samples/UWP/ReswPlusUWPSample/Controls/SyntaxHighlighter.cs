using System;
using ColorCode;
using ColorCode.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ReswPlusUWPSample.Controls
{
    public static class SyntaxHighlighter
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SyntaxHighlighter),
            new PropertyMetadata(false, OnIsEnabledChanged));

        private static readonly DependencyProperty SourceCodeProperty = DependencyProperty.RegisterAttached(
            "SourceCode",
            typeof(string),
            typeof(SyntaxHighlighter),
            new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (!(dependencyObject is TextBlock textBlock))
            {
                return;
            }

            textBlock.Loaded -= OnLoaded;
            textBlock.ActualThemeChanged -= OnActualThemeChanged;

            if ((bool)args.NewValue)
            {
                textBlock.Loaded += OnLoaded;
                textBlock.ActualThemeChanged += OnActualThemeChanged;
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs args)
        {
            Format((TextBlock)sender);
        }

        private static void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            Format((TextBlock)sender);
        }

        private static void Format(TextBlock textBlock)
        {
            var sourceCode = (string)textBlock.GetValue(SourceCodeProperty);
            if (sourceCode == null)
            {
                sourceCode = textBlock.Text;
                textBlock.SetValue(SourceCodeProperty, sourceCode);
            }

            var language = sourceCode.TrimStart().StartsWith("<", StringComparison.Ordinal)
                ? Languages.Xml
                : Languages.CSharp;

            textBlock.Inlines.Clear();
            new RichTextBlockFormatter(textBlock.ActualTheme).FormatInlines(sourceCode, language, textBlock.Inlines);
        }
    }
}
