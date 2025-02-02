using ReswPlus.Core.ResourceParser;
using Xunit;

namespace ReswPlusUnitTests;

public class FormatTagLiterals
{
    [Fact]
    public void TestParseParameters_OneLiteral()
    {
        foreach (var literal in new[] {
            "",
            " ",
            "   ",
            "test",
            "Plural",
            "Int",
            "LONG_TEXT_AAAAAAAAAAAAAAAAAAAAAAAAA",
            "TEXT WITH ] CHAR",
            "TEXT WITH \\\" CHAR"
        })
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { $"\"{literal}\"" }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            _ = Assert.IsType<LiteralStringFormatTagParameter>(res.Parameters[0]);
            var literalParam = (LiteralStringFormatTagParameter)res.Parameters[0];
            Assert.Equal(literal, literalParam.Value);
            Assert.Null(res.PluralizationParameter);
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_LiteralMixed()
    {
        foreach (var literal in new[] {
            "",
            " ",
            "   ",
            "lkryweiuf rysiu",
            "Lorem Ipsum",
            "Int Plural Short",
            "LONG_TEXT_AAAAAABBBBBBBBBBBBBBBBBBBBBBB"
        })
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] {
                $"\"{literal}\"",
                "Int test"
            }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 2);
            _ = Assert.IsType<LiteralStringFormatTagParameter>(res.Parameters[0]);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[1]);
            var literalParam = (LiteralStringFormatTagParameter)res.Parameters[0];
            Assert.Equal(literal, literalParam.Value);
            Assert.Null(res.PluralizationParameter);
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_Multi()
    {
        var strings = new[] {
            "",
            " ",
            "   ",
            "lkryweiuf rysiu",
            "Lorem Ipsum",
            "Int Plural Short",
            "LONG_TEXT_AAAAAABBBBBBBBBBBBBBBBBBBBBBB"
        };
        foreach (var literal1 in strings)
        {
            foreach (var literal2 in strings)
            {
                var basicLocalizedItems = new ReswItem[0];
                var res = FormatTag.ParseParameters("test", new[] {
                $"\"{literal1}\"",
                $"\"{literal2}\"",
            }, basicLocalizedItems, "test", null);
                Assert.NotNull(res);
                Assert.True(res.Parameters.Count == 2);
                for (var i = 0; i < 2; ++i)
                {
                    _ = Assert.IsType<LiteralStringFormatTagParameter>(res.Parameters[i]);
                    var literalParam = (LiteralStringFormatTagParameter)res.Parameters[i];
                    Assert.Equal(i == 0 ? literal1 : literal2, literalParam.Value);
                    Assert.Null(res.PluralizationParameter);
                    Assert.Null(res.VariantParameter);
                }
            }
        }
    }
}
