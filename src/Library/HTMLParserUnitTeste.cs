using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using ReswPlusLib.Shared.EmphasisedStrings;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;

namespace ReswPlusLibUnitTests
{
    [TestClass]
    public class HtmlParserUnitTest
    {
        private bool CheckRun(Run run, string text, FontWeight fontWeight, FontStyle fontStyle, TextDecorations textDecoration)
        {
            return run.Text == text
                && run.FontWeight.Weight == fontWeight.Weight
                && run.FontStyle == fontStyle
                && run.TextDecorations == textDecoration;
        }

        [UITestMethod]
        public void TestHTMLParserBasicString()
        {
            var res = HTMLParser.Parse("Hello").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "Hello", FontWeights.Normal, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse("  Hello").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "  Hello", FontWeights.Normal, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse("Hello  ").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "Hello  ", FontWeights.Normal, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse("  Hello World ").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "  Hello World ", FontWeights.Normal, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse(" ").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), " ", FontWeights.Normal, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse("").ToList();
            Assert.AreEqual(res.Count(), 0);
        }

        [UITestMethod]
        public void TestHTMLParserSimpleEmphasis()
        {
            var res = HTMLParser.Parse("<b>Hello</b>").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "Hello", FontWeights.Bold, FontStyle.Normal, TextDecorations.None));

            res = HTMLParser.Parse("<i>  Hello</i>").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "  Hello", FontWeights.Normal, FontStyle.Italic, TextDecorations.None));

            res = HTMLParser.Parse("<u>Hello  </u>").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "Hello  ", FontWeights.Normal, FontStyle.Normal, TextDecorations.Underline));

            res = HTMLParser.Parse("<s>  Hello World </s>").ToList();
            Assert.AreEqual(res.Count(), 1);
            Assert.IsTrue(CheckRun(res.First(), "  Hello World ", FontWeights.Normal, FontStyle.Normal, TextDecorations.Strikethrough));
        }
    }
}
