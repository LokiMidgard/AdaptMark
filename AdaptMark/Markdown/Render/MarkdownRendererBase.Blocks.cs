// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AdaptMark.Parsers.Markdown.Blocks;

namespace AdaptMark.Parsers.Markdown.Render
{
    /// <summary>
    /// Block Rendering Methods.
    /// </summary>
    public partial class MarkdownRendererBase
    {
        /// <summary>
        /// Renders a paragraph element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderParagraph(ParagraphBlock element, IRenderContext context);

        /// <summary>
        /// Renders a yaml header element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderYamlHeader(YamlHeaderBlock element, IRenderContext context);

        /// <summary>
        /// Renders a header element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderHeader(HeaderBlock element, IRenderContext context);

        /// <summary>
        /// Renders a list element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderListElement(ListBlock element, IRenderContext context);

        /// <summary>
        /// Renders a horizontal rule element.
        /// </summary>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderHorizontalRule(IRenderContext context);

        /// <summary>
        /// Renders a quote element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderQuote(QuoteBlock element, IRenderContext context);

        /// <summary>
        /// Renders a code element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderCode(CodeBlock element, IRenderContext context);

        /// <summary>
        /// Renders a table element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderTable(TableBlock element, IRenderContext context);

        /// <summary>
        /// Renders an element.
        /// </summary>
        /// <param name="element"> The parsed block element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected virtual void RenderOther(MarkdownBlock element, IRenderContext context)
        {
        }
    }
}