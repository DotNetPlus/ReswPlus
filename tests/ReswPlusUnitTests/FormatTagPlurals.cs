using System.Linq;
using ReswPlus.Core.ResourceParser;
using Xunit;

namespace ReswPlusUnitTests;

public class FormatTagPlurals
{

    [Fact]
    public void TestParseParameters_TypeLessPlural()
    {

        var basicLocalizedItems = new ReswItem[0];
        var res = FormatTag.ParseParameters("test", new[] { "Plural" }, basicLocalizedItems, "test", null);
        Assert.NotNull(res);
        Assert.True(res.Parameters.Count == 1);
        _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
        var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
        Assert.Equal(ParameterType.Double, functionParam.Type);
        Assert.Equal(res.PluralizationParameter, functionParam);
        Assert.Null(functionParam.TypeToCast);
        Assert.Null(res.VariantParameter);
    }

    [Fact]
    public void TestParseParameters_NamedTypeLessPlural()
    {

        var basicLocalizedItems = new ReswItem[0];
        var res = FormatTag.ParseParameters("test", new[] { "Plural testParam" }, basicLocalizedItems, "test", null);
        Assert.NotNull(res);
        Assert.True(res.Parameters.Count == 1);
        _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
        var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
        Assert.Equal(ParameterType.Double, functionParam.Type);
        Assert.Equal("testParam", functionParam.Name);
        Assert.Equal(res.PluralizationParameter, functionParam);
        Assert.Null(functionParam.TypeToCast);
        Assert.Null(res.VariantParameter);
    }

    [Fact]
    public void TestParseParameters_OneValidPluralParameter()
    {
        foreach (var type in FormatTag.AcceptedTypes.Where(t => t.Value.CanBeQuantifier))
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Plural " + type.Key }, basicLocalizedItems, "test", null);
            Assert.True(res.Parameters.Count == 1);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
            var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
            Assert.Equal(functionParam.Type, type.Value.Type);
            Assert.Equal(res.PluralizationParameter, functionParam);
            Assert.False(functionParam.IsVariantId);
            if (type.Value.Type == ParameterType.Double)
            {
                Assert.Null(functionParam.TypeToCast);
            }
            else
            {
                Assert.Equal(ParameterType.Double, functionParam.TypeToCast);
            }
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_MultiValidPluralParameter()
    {
        foreach (var type in FormatTag.AcceptedTypes.Where(t => t.Value.CanBeQuantifier))
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] {
                "Char test1",
                "Plural " + type.Key,
                "Int test2"
            }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 3);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[1]);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[2]);
            var testParam1 = (FunctionFormatTagParameter)res.Parameters[0];
            var testParam2 = (FunctionFormatTagParameter)res.Parameters[2];
            Assert.Equal(ParameterType.Char, testParam1.Type);
            Assert.Equal(ParameterType.Int, testParam2.Type);
            Assert.Equal("test1", testParam1.Name);
            Assert.Equal("test2", testParam2.Name);
            Assert.Null(testParam1.TypeToCast);
            Assert.Null(testParam2.TypeToCast);
            Assert.False(testParam1.IsVariantId);
            Assert.False(testParam2.IsVariantId);

            var functionParam = (FunctionFormatTagParameter)res.Parameters[1];
            Assert.Equal(functionParam.Type, type.Value.Type);
            Assert.False(functionParam.IsVariantId);
            Assert.Equal(res.PluralizationParameter, functionParam);
            if (type.Value.Type == ParameterType.Double)
            {
                Assert.Null(functionParam.TypeToCast);
            }
            else
            {
                Assert.Equal(ParameterType.Double, functionParam.TypeToCast);
            }
            Assert.Null(res.VariantParameter);

        }
    }

    [Fact]
    public void TestParseParameters_OneValidNamedPluralParameter()
    {
        foreach (var type in FormatTag.AcceptedTypes.Where(t => t.Value.CanBeQuantifier))
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Plural " + type.Key + " testParam" }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            _ = Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
            var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
            Assert.Equal(functionParam.Type, type.Value.Type);
            Assert.Equal("testParam", functionParam.Name);
            Assert.Equal(res.PluralizationParameter, functionParam);
            Assert.False(functionParam.IsVariantId);
            if (type.Value.Type == ParameterType.Double)
            {
                Assert.Null(functionParam.TypeToCast);
            }
            else
            {
                Assert.Equal(ParameterType.Double, functionParam.TypeToCast);
            }
            Assert.Null(res.VariantParameter);
        }
    }

    [Fact]
    public void TestParseParameters_WrongTypePluralParameter()
    {
        foreach (var type in FormatTag.AcceptedTypes.Where(t => !t.Value.CanBeQuantifier))
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { $"Plural {type}" }, basicLocalizedItems, "test", null);
            Assert.Null(res);
        }
    }
}
