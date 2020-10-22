// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using AdaptMark.Parsers.Core;
using AdaptMark.Parsers.Markdown.Helpers;

namespace AdaptMark.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Represents a span containing subscript text.
    /// </summary>
    public class SubscriptTextInline : MarkdownInline, IInlineContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptTextInline"/> class.
        /// </summary>
        public SubscriptTextInline()
            : base(MarkdownInlineType.Subscript)
        {
        }

        /// <summary>
        /// Gets or sets the contents of the inline.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; } = Array.Empty<MarkdownInline>();

        IReadOnlyList<MarkdownInline> IInlineContainer.Inlines => this.Inlines.AsReadonly();
        /// <summary>
        /// Attempts to parse a subscript text span.
        /// </summary>
        public new class Parser : InlineSourundParser<SubscriptTextInline>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> class.
            /// </summary>
            public Parser()
                : base("<sub>", "</sub>")
            {
            }

            /// <inheritdoc/>
            protected override SubscriptTextInline MakeInline(List<MarkdownInline> inlines) => new SubscriptTextInline
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

            return "<sub>" + string.Join(string.Empty, Inlines) + "</sub>";
        }
    }
}
