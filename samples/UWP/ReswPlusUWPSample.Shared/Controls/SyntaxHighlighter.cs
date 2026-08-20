using ReswPlusSamples.SyntaxHighlighting;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

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

            textBlock.Inlines.Clear();
            var brushes = CreateBrushes(textBlock.ActualTheme);
            var position = 0;

            SyntaxTokenizer.Tokenize(sourceCode, token =>
            {
                AddRun(textBlock, sourceCode, position, token.Start - position, null);
                AddRun(textBlock, sourceCode, token.Start, token.Length, brushes[(int)token.Kind]);
                position = token.Start + token.Length;
            });

            AddRun(textBlock, sourceCode, position, sourceCode.Length - position, null);
        }

        private static void AddRun(TextBlock textBlock, string sourceCode, int start, int length, Brush foreground)
        {
            if (length == 0)
            {
                return;
            }

            var run = new Run { Text = sourceCode.Substring(start, length) };
            if (foreground != null)
            {
                run.Foreground = foreground;
            }

            textBlock.Inlines.Add(run);
        }

        private static Brush[] CreateBrushes(ElementTheme theme)
        {
            var dark = theme == ElementTheme.Dark;
            return new Brush[]
            {
                CreateBrush(dark ? 0x6A : 0x00, dark ? 0x99 : 0x80, dark ? 0x55 : 0x00),
                CreateBrush(dark ? 0x56 : 0x00, dark ? 0x9C : 0x00, dark ? 0xD6 : 0xFF),
                CreateBrush(dark ? 0xB5 : 0x09, dark ? 0xCE : 0x86, dark ? 0xA8 : 0x58),
                CreateBrush(dark ? 0xCE : 0xA3, dark ? 0x91 : 0x15, dark ? 0x78 : 0x15),
                CreateBrush(dark ? 0x9C : 0x81, dark ? 0xDC : 0x1F, dark ? 0xFE : 0x3F),
                CreateBrush(dark ? 0x56 : 0x00, dark ? 0x9C : 0x00, dark ? 0xD6 : 0xFF),
            };
        }

        private static SolidColorBrush CreateBrush(int red, int green, int blue)
        {
            return new SolidColorBrush(Color.FromArgb(0xFF, (byte)red, (byte)green, (byte)blue));
        }
    }
}
