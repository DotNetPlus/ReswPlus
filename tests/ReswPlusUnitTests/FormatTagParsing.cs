using System;
using System.Collections.Generic;
using System.Linq;
using ReswPlus.Core.ResourceParser;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

public class FormatTagParsing
{
    [Fact]
    public void TestTagType()
    {
        foreach (var parameters in new string[]{
            "Double",
            "Int, Double",
            "UInt test, Char character",
            "Plural",
            "String test, Plural, String test2, UInt test3",
            "Variant v",
            "String test, Variant v",
            "String test, Variant v, Plural Int",
            "String test, Variant v, Plural Int test",
        })
        {
            var res = ReswClassGenerator.ParseTag($"#Format[{parameters}]");
            Assert.False(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
            res = ReswClassGenerator.ParseTag($"#FormatNet[{parameters}]");
            Assert.True(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
        }
    }

    [Fact]
    public void TestTagLiteral()
    {
        foreach (var parameters in new string[]{
            "\"Hello\"",
            "\"Hello world\", Double",
            "UInt test, \"World is small\"",
            "Plural, \"Test\"",
            "String test, Plural, String test2, \"Hello\"",
            "Variant v, \"AAAA\"",
            "String test, \"BBB\", Variant v",
            "String test, \"Hello\", \"Hello\", Plural Int",
            "\"Hello\", \"Hello\", Plural Int test",
            "\"Hello ] World\"", //literal with ']'
            "\"Hello \\\" World\"", //literal with '"'
            "\"Hello , World\"", //literal with ','
            "\"Hello ( World\"", //literal with ')'
            "\"Hello ) World\"", //literal with '('
            "\"Hello () World\"", //literal with '()'

        })
        {
            var res = ReswClassGenerator.ParseTag($"#Format[{parameters}]");
            Assert.False(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
            Assert.Equal(res.format, parameters);
            res = ReswClassGenerator.ParseTag($"#FormatNet[{parameters}]");
            Assert.True(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
        }
    }

    [Fact]
    public void TestTagStringRef()
    {
        foreach (var parameters in new string[]{
            "Test()",
            "Int, String()",
            "Hello(), Char character",
            "Plural, Welcome_Message()",
            "String test, Hello(), String test2, UInt test3",
            "Variant v, Test(), Test()",
            "hello(), String test, World(), Variant v",
            "String test, Variant v, Hello(), Plural Int",
            "String test, Hello(), Variant v, Plural Int test",
        })
        {
            var res = ReswClassGenerator.ParseTag($"#Format[{parameters}]");
            Assert.False(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
            res = ReswClassGenerator.ParseTag($"#FormatNet[{parameters}]");
            Assert.True(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
        }
    }

    [Fact]
    public void TestTagMacros()
    {
        foreach (var parameters in new string[]{
            "SHORT_WEEKDAY",
            "LOCALE_NAME, Double",
            "UInt test, Char character",
            "TIME",
            "String test, Plural, VERSION_X, UInt test3",
            "Variant v",
            "APP_NAME, Variant v",
            "String test, VERSION_XY, Plural Int",
            "String test, Variant v, TIME",
        })
        {
            var res = ReswClassGenerator.ParseTag($"#Format[{parameters}]");
            Assert.False(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
            res = ReswClassGenerator.ParseTag($"#FormatNet[{parameters}]");
            Assert.True(res.isDotNetFormatting);
            Assert.True(res.format != null && res.format.Any());
        }
    }

    [Fact]
    public void TestTagParsing()
    {
        foreach (var type in FormatTag.AcceptedTypes)
        {
            var res = ReswClassGenerator.ParseTag($"#Format[{type.Key}]");
            Assert.True(res.format == type.Key);

            //Text before the tag
            var parameters = $"Int, {type.Key}, String test";
            res = ReswClassGenerator.ParseTag($"This is a comment #Format[{parameters}]");
            Assert.True(res.format == parameters);

            //Text after the tag
            res = ReswClassGenerator.ParseTag($"#Format[{parameters}] This is a text");
            Assert.True(res.format == parameters);

            //Text before and after the tag
            res = ReswClassGenerator.ParseTag($"Test #FormatNet[{parameters}] This is a text");
            Assert.True(res.format == parameters);

            //Remove spaces
            res = ReswClassGenerator.ParseTag($"Hello world #FormatNet[  {parameters}  ] This is a text!");
            Assert.True(res.format == parameters);
        }
    }

    [Fact]
    public void TestTagIncorrectParsing()
    {
        var incorrectTags = new string[]
        {
            "",
            "   ",
            " #Format  ",
            "hello  #Format[ world",
            "#Firmat[Int]",
            "Hello Format[Int] World",
            "Format[Int]",
            "#Format(Int)",
        };

        foreach (var tag in incorrectTags)
        {
            var res = ReswClassGenerator.ParseTag(tag);
            Assert.Null(res.format);
        }
    }

    [Fact]
    public void TestSplittingParameters()
    {
        var parametersList = new List<string[]>(){
            new string[]
        {
            "hi",
            "\"test\"",
            "hello",
            "world"
        },
        new string[]
        {
            "test",
            "\"hello, world\"", // the comma in the string would cause the test to fail if we used .Split(',')
            "HI"
        },
        new string[]
        {
            "test",
            "\"hello\\\" world\"",
            "HI"
        },
        new string[]
        {
            "  test ",
            " \"hello[] world\" ",
            "HI   "
        },
          new string[]
        {"Hello" }
    };

        foreach (var parameters in parametersList)
        {
            var parametersStr = parameters.Aggregate((a, b) => a + "," + b);
            var resultParameters = FormatTag.SplitParameters(parametersStr).ToList();
            Assert.True(parameters.Select(s => s.Trim()).SequenceEqual(resultParameters));
        }
    }
}
