using AdaptMark.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdaptMark.Markdown.Blocks
{
public interface IBlockContainer
    {
        IReadOnlyList<MarkdownBlock> Blocks { get; }
    }
}
