// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptMark.Parsers.Core;
using AdaptMark.Parsers.Markdown.Helpers;

namespace AdaptMark.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Represents a span containing strikethrough text.
    /// </summary>
    public class StrikethroughTextInline : MarkdownInline, IInlineContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrikethroughTextInline"/> class.
        /// </summary>
        public StrikethroughTextInline()
            : base(MarkdownInlineType.Strikethrough)
        {
        }

        /// <summary>
        /// Gets or sets The contents of the inline.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; } = Array.Empty<MarkdownInline>();

        /// <summary>
        /// Attempts to parse a strikethrough text span.
        /// </summary>
        public new class Parser : InlineSourundParser<StrikethroughTextInline>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> class.
            /// </summary>
            public Parser()
                : base("~~")
            {
            }

            /// <inheritdoc/>
            protected override StrikethroughTextInline MakeInline(List<MarkdownInline> inlines) => new StrikethroughTextInline
            {
                Inlines = inlines,
            };
        }

        protected override string StringRepresentation()
        {
            if (Inlines == null)
            {
                return string.Empty;
            }

            return "~~" + string.Join(string.Empty, Inlines) + "~~";
        }
    }
}