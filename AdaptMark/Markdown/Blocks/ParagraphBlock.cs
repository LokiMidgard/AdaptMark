// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using AdaptMark.Parsers.Markdown.Helpers;
using AdaptMark.Parsers.Markdown.Inlines;

namespace AdaptMark.Parsers.Markdown.Blocks
{
    /// <summary>
    /// Represents a block of text that is displayed as a single paragraph.
    /// </summary>
    public class ParagraphBlock : MarkdownBlock, IInlineContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphBlock"/> class.
        /// </summary>
        public ParagraphBlock()
            : base(MarkdownBlockType.Paragraph)
        {
        }

        /// <summary>
        /// Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; } = Array.Empty<MarkdownInline>();
        IReadOnlyList<MarkdownInline> IInlineContainer.Inlines => this.Inlines.AsReadonly();

        /// <summary>
        /// Parses paragraph text.
        /// </summary>
        /// <param name="markdown">The markdown text. </param>
        /// <param name="document">The parsing Document.</param>
        /// <returns> A parsed paragraph. Or <c>null</c> if nothing was parsed.</returns>
        public static ParagraphBlock? Parse(LineBlock markdown, MarkdownDocument document)
        {
            var inlines = document.ParseInlineChildren(markdown, true, true);

            // If we didn't find inline elements we return no Paragraph
            if (inlines.Count == 0)
            {
                return null;
            }

            var result = new ParagraphBlock
            {
                Inlines = inlines,
            };
            return result;
        }

        /// <summary>
        /// Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        protected override string StringRepresentation()
        {
            if (Inlines == null)
            {
                return string.Empty;
            }

            return string.Join(string.Empty, Inlines);
        }
    }
}