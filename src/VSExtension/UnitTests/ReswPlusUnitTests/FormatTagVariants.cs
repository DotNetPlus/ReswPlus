using ReswPlus.Core.ResourceParser;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ReswPlusUnitTests
{
    public class FormatTagVariants
    {
        [Fact]
        public void TestParseParameters_OneValidNamelessVariantParameter()
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Variant" }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
            var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
            Assert.Equal(ParameterType.Long, functionParam.Type);
            Assert.Null(res.PluralizationParameter);
            Assert.True(functionParam.IsVariantId);
            Assert.Equal(res.VariantParameter, functionParam);
        }

        [Fact]
        public void TestParseParameters_ValidNamedVariantParameter()
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Variant var" }, basicLocalizedItems, "test", null);
            Assert.NotNull(res);
            Assert.True(res.Parameters.Count == 1);
            Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
            var functionParam = (FunctionFormatTagParameter)res.Parameters[0];
            Assert.Equal(ParameterType.Long, functionParam.Type);
            Assert.Null(res.PluralizationParameter);
            Assert.True(functionParam.IsVariantId);
            Assert.Equal("var", functionParam.Name);
            Assert.Equal(res.VariantParameter, functionParam);
        }

        [Fact]
        public void TestParseParameters_OnlyOneVariantAccepted()
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Variant var1", "Variant var2" }, basicLocalizedItems, "test", null);
            Assert.Null(res);
        }

        [Fact]
        public void TestParseParameters_TypedVariantNotAccepted()
        {
            var basicLocalizedItems = new ReswItem[0];
            var res = FormatTag.ParseParameters("test", new[] { "Variant Int var1", "Variant var2" }, basicLocalizedItems, "test", null);
            Assert.Null(res);
        }
    }
}
