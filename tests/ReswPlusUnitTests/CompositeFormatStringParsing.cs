using System;
using System.Collections.Generic;
using ReswPlus.SourceGenerator.Analysis;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests for the parser used to decide whether the value of a formatted resource is a usable composite format
/// string, and which arguments it references.
/// </summary>
public class CompositeFormatStringParsing
{
    [Theory]
    [InlineData("", new int[0])]
    [InlineData("no placeholder at all", new int[0])]
    [InlineData("{0}", new[] { 0 })]
    [InlineData("{1} {0}", new[] { 0, 1 })]
    [InlineData("{0} and {0} again", new[] { 0 })]
    [InlineData("{0,10}", new[] { 0 })]
    [InlineData("{0,-10}", new[] { 0 })]
    [InlineData("{0:C2}", new[] { 0 })]
    [InlineData("{2,-10:yyyy-MM-dd}", new[] { 2 })]
    [InlineData("{{0}}", new int[0])]
    [InlineData("{{{0}}}", new[] { 0 })]
    [InlineData("100% of {0}", new[] { 0 })]
    public void ValidFormatStringsAreParsed(string value, int[] expectedIndexes)
    {
        Assert.True(CompositeFormatString.TryGetArgumentIndexes(value, out var indexes));
        Assert.Equal(expectedIndexes, indexes);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{0")]
    [InlineData("0}")]
    [InlineData("{}")]
    [InlineData("{name}")]
    [InlineData("{0,}")]
    [InlineData("{0 0}")]
    [InlineData("{{0}")]
    [InlineData("{0:{}")]
    public void MalformedFormatStringsAreRejected(string value)
    {
        Assert.False(CompositeFormatString.TryGetArgumentIndexes(value, out _));
    }

    [Fact]
    public void TheParserAgreesWithStringFormat()
    {
        // Accepting a value the runtime rejects means missing a guaranteed crash, and rejecting a value the
        // runtime accepts means reporting a perfectly good resource. Both are checked exhaustively over the
        // alphabet that makes up a format item.
        var alphabet = new[] { '{', '}', '0', '1', 'a', ',', ':', '-', ' ' };
        var values = new object[1000];
        var buffer = new char[4];
        var mismatches = new List<string>();

        for (var i = 0; i < values.Length; i++)
        {
            // A string argument ignores the format specifier, so only the shape of the value is under test.
            values[i] = "x";
        }

        void Walk(int depth, int length)
        {
            if (depth == length)
            {
                var candidate = new string(buffer, 0, length);

                bool isAcceptedByTheRuntime;

                try
                {
                    _ = string.Format(candidate, values);

                    isAcceptedByTheRuntime = true;
                }
                catch (FormatException)
                {
                    isAcceptedByTheRuntime = false;
                }

                if (CompositeFormatString.TryGetArgumentIndexes(candidate, out _) != isAcceptedByTheRuntime)
                {
                    mismatches.Add(candidate);
                }

                return;
            }

            foreach (var character in alphabet)
            {
                buffer[depth] = character;

                Walk(depth + 1, length);
            }
        }

        for (var length = 0; length <= buffer.Length; length++)
        {
            Walk(0, length);
        }

        Assert.Empty(mismatches);
    }
}
