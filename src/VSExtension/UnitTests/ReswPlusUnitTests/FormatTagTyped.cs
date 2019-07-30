using ReswPlus.Core.ClassGenerator;
using ReswPlus.Core.ResourceParser;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReswPlusUnitTests
{
    public class FormatTagTyped
    {
        [Fact]
        public void TestGetParameterTypeExistingTypes()
        {
            foreach (var type in FormatTag.AcceptedTypes)
            {
                var typeRes = FormatTag.GetParameterType(type.Key, false);
                Assert.Equal(typeRes.type, type.Value.Type);
                Assert.Null(typeRes.typeToCast);
                Assert.False(typeRes.isVariantId);

                typeRes = FormatTag.GetParameterType(type.Key, true);
                Assert.Equal(typeRes.type, type.Value.Type);
                Assert.Equal(typeRes.typeToCast.HasValue, type.Value.CanBeQuantifier && type.Value.Type != ParameterType.Double);
                if (typeRes.typeToCast.HasValue)
                {
                    Assert.Equal(ParameterType.Double, typeRes.typeToCast.Value);
                }
            }

            var res = FormatTag.GetParameterType("", true);
            Assert.Equal(ParameterType.Double, res.type);
            Assert.Null(res.typeToCast);
        }

        [Fact]
        public void TestGetParameterTypeIncorrectTypes()
        {
            var res = FormatTag.GetParameterType("", false);
            Assert.Null(res.type);
            Assert.Null(res.typeToCast);

            foreach (var type in new string[]{
                "ant",
                "doble",
                "Int, Double",
                "UnsignedInt"
            })
            {
                res = FormatTag.GetParameterType(type, false);
                Assert.Null(res.type);
                Assert.Null(res.typeToCast);

                res = FormatTag.GetParameterType(type, true);
                Assert.Null(res.type);
                Assert.Null(res.typeToCast);
            }
        }

        [Fact]
        public void TestParseParameters_OneValidParameter()
        {
            foreach (var type in FormatTag.AcceptedTypes)
            {
                var basicLocalizedItems = new ReswItem[0];
                var res = FormatTag.ParseParameters("test", new[] { type.Key }, basicLocalizedItems, "test", null);
                Assert.NotNull(res);
                Assert.True(res.Parameters.Count == 1);
                Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
                Assert.Equal(((FunctionFormatTagParameter)res.Parameters[0]).Type, type.Value.Type);
                Assert.Null(res.PluralizationParameter);
                Assert.Null(res.VariantParameter);
            }
        }

        [Fact]
        public void TestParseParameters_OneValidNamedParameter()
        {
            foreach (var type in FormatTag.AcceptedTypes)
            {
                var basicLocalizedItems = new ReswItem[0];
                var res = FormatTag.ParseParameters("test", new[] { type.Key + " param" }, basicLocalizedItems, "test", null);
                Assert.NotNull(res);
                Assert.True(res.Parameters.Count == 1);
                Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
                Assert.Equal(((FunctionFormatTagParameter)res.Parameters[0]).Name, "param");
                Assert.Equal(((FunctionFormatTagParameter)res.Parameters[0]).Name, "param");
                Assert.False(((FunctionFormatTagParameter)res.Parameters[0]).IsVariantId);
                Assert.Null(res.PluralizationParameter);
                Assert.Null(res.VariantParameter);
            }
        }

        [Fact]
        public void TestParseParameters_MultiValidParameter()
        {
            foreach (var type1 in FormatTag.AcceptedTypes)
            {
                foreach (var type2 in FormatTag.AcceptedTypes)
                {
                    var basicLocalizedItems = new ReswItem[0];
                    var res = FormatTag.ParseParameters("test", new[] { type1.Key, type2.Key + " paramName" }, basicLocalizedItems, "test", null);
                    Assert.NotNull(res);
                    Assert.True(res.Parameters.Count == 2);
                    Assert.IsType<FunctionFormatTagParameter>(res.Parameters[0]);
                    Assert.Equal(((FunctionFormatTagParameter)res.Parameters[0]).Type, type1.Value.Type);
                    Assert.False(((FunctionFormatTagParameter)res.Parameters[0]).IsVariantId);
                    Assert.Null(res.PluralizationParameter);
                    Assert.Null(res.VariantParameter);
                    Assert.IsType<FunctionFormatTagParameter>(res.Parameters[1]);
                    Assert.Equal(((FunctionFormatTagParameter)res.Parameters[1]).Type, type2.Value.Type);
                    Assert.False(((FunctionFormatTagParameter)res.Parameters[1]).IsVariantId);
                    Assert.Equal("paramName", ((FunctionFormatTagParameter)res.Parameters[1]).Name);
                    Assert.Null(res.PluralizationParameter);
                    Assert.Null(res.VariantParameter);
                }
            }
        }

    }
}
