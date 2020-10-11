// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptMark.Parsers.Markdown.Helpers;

namespace AdaptMark.Parsers.Markdown.Blocks
{
    /// <summary>
    /// Represents a block that is displayed using a quote style.  Quotes are used to indicate
    /// that the text originated elsewhere (e.g. a previous comment).
    /// </summary>
    public class QuoteBlock : MarkdownBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteBlock"/> class.
        /// </summary>
        public QuoteBlock()
            : base(MarkdownBlockType.Quote)
        {
        }

        /// <summary>
        /// Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownBlock> Blocks { get; set; }

        protected override string StringRepresentation()
        {
            var txt = MarkdownBlock.ToString(this.Blocks);

            var block = new LineBlock(txt.AsSpan());
            var builder = new StringBuilder();

            for (int i = 0; i < block.LineCount; i++)
            {
                var line = block[i];
                builder.Append("> ");
                builder.AppendLine(line.ToString());
            }


            return builder.ToString();
        }

        /// <summary>
        /// Parses QuoteBlock.
        /// </summary>
        public new class Parser : Parser<QuoteBlock>
        {
            /// <inheritdoc/>
            protected override void ConfigureDefaults(DefaultParserConfiguration configuration)
            {
                base.ConfigureDefaults(configuration);
                configuration.After<CodeBlock.ParserIndented>();
            }

            /// <inheritdoc/>
            protected override BlockParseResult<QuoteBlock> ParseInternal(in LineBlock markdown, int startLine, bool lineStartsNewParagraph, MarkdownDocument document)
            {
                if (markdown.LineCount == 0)
                {
                    return null;
                }

                var nonSpace = markdown[0].IndexOfNonWhiteSpace();
                if (nonSpace == -1 || markdown[startLine][nonSpace] != '>')
                {
                    return null;
                }

                bool lastDidNotContainedQuoteCharacter = false;

                var qutedBlock = markdown.SliceLines(startLine).RemoveFromLine((line, lineIndex) =>
                {
                    int startOfText;
                    var nonSpace = line.IndexOfNonWhiteSpace();
                    if (nonSpace == -1)
                    {
                        return (0, 0, true, true);
                    }
                    else if (line[nonSpace] != '>')
                    {
                        if (lastDidNotContainedQuoteCharacter)
                        {
                            return (0, 0, true, true);
                        }
                        else
                        {
                            lastDidNotContainedQuoteCharacter = true;
                            startOfText = nonSpace;
                        }
                    }
                    else
                    {
                        lastDidNotContainedQuoteCharacter = false;
                        startOfText = nonSpace + 1;
                    }

                    if (startOfText >= line.Length)
                    {
                        return (0, 0, false, false);
                    }

                    // ignore the first space aufter aqute character
                    if (line[startOfText] == ' ')
                    {
                        startOfText++;
                    }

                    if (startOfText >= line.Length)
                    {
                        return (0, 0, false, false);
                    }

                    return (startOfText, line.Length - startOfText, false, false);
                });

                var result = new QuoteBlock();

                if (qutedBlock.LineCount != 0)
                {
                    // Recursively call into the markdown block parser.
                    result.Blocks = document.ParseBlocks(qutedBlock);
                }
                else
                {
                    result.Blocks = Array.Empty<MarkdownBlock>();
                }

                return BlockParseResult.Create(result, startLine, qutedBlock.LineCount);
            }
        }
    }
}