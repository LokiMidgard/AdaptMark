// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using AdaptMark.Parsers.Markdown;
using AdaptMark.Parsers.Markdown.Blocks;

namespace UnitTests.Markdown.Parse
{
    /// <summary>
    /// The base class for our display unit tests.
    /// </summary>
    public abstract class ParseTestBase : TestBase
    {
        internal void AssertEqual(string markdown, params MarkdownBlock[] expectedAst)
        {
            var expected = new StringBuilder();
            expected.AppendLine();
            foreach (var block in expectedAst)
            {
                SerializeElement(expected, block, indentLevel: 0);
            }
            
            expected.AppendLine();

            var parser = new MarkdownDocument();
            parser.Parse(markdown);

            var actual = new StringBuilder();
            actual.AppendLine();
            foreach (var block in parser.Blocks)
            {
                SerializeElement(actual, block, indentLevel: 0);
            }

            actual.AppendLine();

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        internal void AssertEqual(MarkdownDocument document, params MarkdownBlock[] expectedAst)
        {
            var expected = new StringBuilder();
            expected.AppendLine();
            foreach (var block in expectedAst)
            {
                SerializeElement(expected, block, indentLevel: 0);
            }

            expected.AppendLine();

            var actual = new StringBuilder();
            actual.AppendLine();
            foreach (var block in document.Blocks)
            {
                SerializeElement(actual, block, indentLevel: 0);
            }
            
            actual.AppendLine();

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }
    }
}