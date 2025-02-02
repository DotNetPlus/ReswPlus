using ReswPlus.Core.ResourceParser;
using Xunit;

namespace ReswPlusUnitTests;

public class FormatTagMacros
{
    [Fact]
    public void TestParseParameters_OneLiteral()
    {
        foreach (var macro in FormatTag.MacrosAvailable)
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { macro.Key }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            _ = Assert.IsType<MacroFormatTagParameter>(res.Parameters[0]);
            var macroParam = (MacroFormatTagParameter)res.Parameters[0];
            Assert.Equal(macro.Value, macroParam.Id);
            Assert.Null(res.PluralizationParameter);
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_LiteralMixed()
    {
        foreach (var macro in FormatTag.MacrosAvailable)
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] {
                macro.Key,
                "Int test"
            }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 2);
            _ = Assert.IsType<MacroFormatTagParameter>(res.Parameters[0]);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[1]);
            var macroParam = (MacroFormatTagParameter)res.Parameters[0];
            Assert.Equal(macro.Value, macroParam.Id);
            Assert.Null(res.PluralizationParameter);
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_Multi()
    {
        foreach (var macro1 in FormatTag.MacrosAvailable)
        {
            foreach (var macro2 in FormatTag.MacrosAvailable)
            {
                var basicLocalizedItems = new ReswItem[0];
                var res = FormatTag.ParseParameters("test", new[] {
               macro1.Key,
               macro2.Key,
            }, basicLocalizedItems, "test", null);
                Assert.NotNull(res);
                Assert.True(res.Parameters.Count == 2);
                for (var i = 0; i < 2; ++i)
                {
                    _ = Assert.IsType<MacroFormatTagParameter>(res.Parameters[i]);
                    var macroParam = (MacroFormatTagParameter)res.Parameters[i];
                    Assert.Equal(i == 0 ? macro1.Value : macro2.Value, macroParam.Id);
                    Assert.Null(res.PluralizationParameter);
                    Assert.Null(res.VariantParameter);
                }
            }
        }
    }
}
