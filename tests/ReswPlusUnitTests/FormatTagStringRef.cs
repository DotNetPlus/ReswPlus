using System.Collections.Generic;
using ReswPlus.Core.ResourceParser;
using Xunit;

namespace ReswPlusUnitTests;

public class FormatTagStringRef
{
    [Fact]
    public void TestParseParameters_StringRef()
    {
        var localizedItems = new List<ReswItem>()
            {
                new("TestString", "Test"),
                new("Hello", "World"),
                new("1223", "Number"),
                new("Test_String", "TEST"),
            };

        foreach (var item in localizedItems)
        {
            var res = FormatTag.ParseParameters("test", new[] { $"{item.Key}()" }, localizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            _ = Assert.IsType<StringRefFormatTagParameter>(res.Parameters[0]);
            var stringRefParam = (StringRefFormatTagParameter)res.Parameters[0];
            Assert.Equal(item.Key, stringRefParam.Id);
            Assert.Null(res.PluralizationParameter);
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_WrongStringRef()
    {
        var localizedItems = new List<ReswItem>()
            {
                new("TestString", "Test"),
                new("Hello", "World"),
                new("1223", "Number"),
                new("Test_String", "TEST"),
            };

        var wrongRefItems = new List<string>()
        {
            "AA",
            "Test",
            "Number"
        };

        foreach (var item in wrongRefItems)
        {
            var res = FormatTag.ParseParameters("test", new[] { $"{item}()" }, localizedItems, "test", null);
            Assert.Null(res);
        }
    }

    [Fact]
    public void TestParseParameters_Multi()
    {
        var localizedItems = new List<ReswItem>()
            {
                new("TestString", "Test"),
                new("Hello", "World"),
                new("1223", "Number"),
                new("Test_String", "TEST"),
            };

        foreach (var stringRef1 in localizedItems)
        {
            foreach (var stringRef2 in localizedItems)
            {
                var res = FormatTag.ParseParameters("test", new[] {
              $"{stringRef1.Key}()",
              $"{stringRef2.Key}()",
            }, localizedItems, "test", null);
                Assert.NotNull(res);
                Assert.True(res.Parameters.Count == 2);
                for (var i = 0; i < 2; ++i)
                {
                    _ = Assert.IsType<StringRefFormatTagParameter>(res.Parameters[i]);
                    var macroParam = (StringRefFormatTagParameter)res.Parameters[i];
                    Assert.Equal(i == 0 ? stringRef1.Key : stringRef2.Key, macroParam.Id);
                    Assert.Null(res.PluralizationParameter);
                    Assert.Null(res.VariantParameter);
                }
            }
        }
    }
}
