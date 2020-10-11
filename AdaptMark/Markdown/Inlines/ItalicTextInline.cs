// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using AdaptMark.Parsers.Core;
using AdaptMark.Parsers.Markdown.Helpers;

namespace AdaptMark.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Represents a span containing italic text.
    /// </summary>
    public class ItalicTextInline : MarkdownInline, IInlineContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItalicTextInline"/> class.
        /// </summary>
        public ItalicTextInline()
            : base(MarkdownInlineType.Italic)
        {
        }

        /// <summary>
        /// Gets or sets the contents of the inline.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; }

        /// <summary>
        /// Attempts to parse a bold text span.
        /// </summary>
        public class ParserAsterix : InlineSourundParser<ItalicTextInline>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParserAsterix"/> class.
            /// </summary>
            public ParserAsterix()
                : base("*")
            {
            }

            /// <inheritdoc/>
            protected override ItalicTextInline MakeInline(List<MarkdownInline> inlines) => new ItalicTextInline
            {
                Inlines = inlines,
            };
        }

        /// <summary>
        /// Attempts to parse a bold text span.
        /// </summary>
        public class ParserUnderscore : InlineSourundParser<ItalicTextInline>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParserUnderscore"/> class.
            /// </summary>
            public ParserUnderscore()
                : base("_")
            {
            }

            /// <inheritdoc/>
            protected override ItalicTextInline MakeInline(List<MarkdownInline> inlines) => new ItalicTextInline
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

            return "*" + string.Join(string.Empty, Inlines) + "*";
        }
    }
}