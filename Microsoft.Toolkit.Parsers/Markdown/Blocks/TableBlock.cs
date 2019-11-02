// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Toolkit.Parsers.Core;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;

namespace Microsoft.Toolkit.Parsers.Markdown.Blocks
{
    /// <summary>
    /// Represents a block which contains tabular data.
    /// </summary>
    public class TableBlock : MarkdownBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableBlock"/> class.
        /// </summary>
        public TableBlock()
            : base(MarkdownBlockType.Table)
        {
        }

        /// <summary>
        /// Gets or sets the table rows.
        /// </summary>
        public IList<TableRow> Rows { get; set; }

        /// <summary>
        /// Gets or sets describes the columns in the table.  Rows can have more or less cells than the number
        /// of columns.  Rows with fewer cells should be padded with empty cells.  For rows with
        /// more cells, the extra cells should be hidden.
        /// </summary>
        public IList<TableColumnDefinition> ColumnDefinitions { get; set; }

        /// <summary>
        /// Describes a column in the markdown table.
        /// </summary>
        public class TableColumnDefinition
        {
            /// <summary>
            /// Gets or sets the alignment of content in a table column.
            /// </summary>
            public ColumnAlignment Alignment { get; set; }
        }

        /// <summary>
        /// Represents a single row in the table.
        /// </summary>
        public class TableRow
        {
            /// <summary>
            /// Gets or sets the table cells.
            /// </summary>
            public IList<TableCell> Cells { get; set; }

            /// <summary>
            /// Parses the contents of the row, ignoring whitespace at the beginning and end of each cell.
            /// </summary>
            /// <param name="markdown"> The markdown text. </param>
            /// <param name="startingPos"> The position of the start of the row. </param>
            /// <param name="maxEndingPos"> The maximum position of the end of the row </param>
            /// <param name="requireVerticalBar"> Indicates whether the line must contain a vertical bar. </param>
            /// <param name="contentParser"> Called for each cell. </param>
            /// <returns> The position of the start of the next line. </returns>
            internal static int ParseContents(string markdown, int startingPos, int maxEndingPos, bool requireVerticalBar, Action<int, int> contentParser)
            {
                // Skip quote characters.
                int pos = startingPos;

                // If the line starts with a '|' character, skip it.
                bool lineHasVerticalBar = false;
                if (pos < maxEndingPos && markdown[pos] == '|')
                {
                    lineHasVerticalBar = true;
                    pos++;
                }

                while (pos < maxEndingPos)
                {
                    // Ignore any whitespace at the start of the cell (except for a newline character).
                    while (pos < maxEndingPos && ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]) && markdown[pos] != '\n' && markdown[pos] != '\r')
                    {
                        pos++;
                    }

                    int startOfCellContent = pos;

                    // Find the end of the cell.
                    bool endOfLineFound = true;
                    while (pos < maxEndingPos)
                    {
                        char c = markdown[pos];
                        if (c == '|' && (pos == 0 || markdown[pos - 1] != '\\'))
                        {
                            lineHasVerticalBar = true;
                            endOfLineFound = false;
                            break;
                        }

                        if (c == '\n')
                        {
                            break;
                        }

                        if (c == '\r')
                        {
                            if (pos < maxEndingPos && markdown[pos + 1] == '\n')
                            {
                                pos++; // Swallow the complete linefeed.
                            }

                            break;
                        }

                        pos++;
                    }

                    int endOfCell = pos;

                    // If a vertical bar is required, and none was found, then exit early.
                    if (endOfLineFound && !lineHasVerticalBar && requireVerticalBar)
                    {
                        return startingPos;
                    }

                    // Ignore any whitespace at the end of the cell.
                    if (endOfCell > startOfCellContent)
                    {
                        while (ParseHelpers.IsMarkdownWhiteSpace(markdown[pos - 1]))
                        {
                            pos--;
                        }
                    }

                    int endOfCellContent = pos;

                    if (endOfLineFound == false || endOfCellContent > startOfCellContent)
                    {
                        // Parse the contents of the cell.
                        contentParser(startOfCellContent, endOfCellContent);
                    }

                    // End of input?
                    if (pos == maxEndingPos)
                    {
                        break;
                    }

                    // Move to the next cell, or the next line.
                    pos = endOfCell + 1;

                    // End of the line?
                    if (endOfLineFound)
                    {
                        break;
                    }
                }

                return pos;
            }

            /// <summary>
            /// Called when this block type should parse out the goods. Given the markdown, a starting point, and a max ending point
            /// the block should find the start of the block, find the end and parse out the middle. The end most of the time will not be
            /// the max ending pos, but it sometimes can be. The function will return where it ended parsing the block in the markdown.
            /// </summary>
            /// <returns>the postiion parsed to</returns>
            internal int Parse(string markdown, int startingPos, int maxEndingPos, MarkdownDocument document)
            {
                Cells = new List<TableCell>();
                return ParseContents(
                    markdown,
                    startingPos,
                    maxEndingPos,
                    requireVerticalBar: true,
                    contentParser: (startingPos2, maxEndingPos2) =>
                    {
                        var cell = new TableCell();
                        cell.Inlines = document.ParseInlineChildren(markdown, startingPos2, maxEndingPos2, Array.Empty<Type>());
                        Cells.Add(cell);
                    });
            }
        }

        /// <summary>
        /// Represents a cell in the table.
        /// </summary>
        public class TableCell
        {
            /// <summary>
            /// Gets or sets the cell contents.
            /// </summary>
            public IList<MarkdownInline> Inlines { get; set; }
        }

        /// <summary>
        /// Parses Tables
        /// </summary>
        public new class Parser : Parser<TableBlock>
        {
            /// <inheritdoc/>
            protected override TableBlock ParseInternal(string markdown, int startOfLine, int firstNonSpace, int realStartOfLine, int endOfFirstLine, int maxEnd, out int actualEnd, StringBuilder paragraphText, bool lineStartsNewParagraph, MarkdownDocument document)
            {
                // A table is a line of text, with at least one vertical bar (|), followed by a line of
                // of text that consists of alternating dashes (-) and vertical bars (|) and optionally
                // vertical bars at the start and end.  The second line must have at least as many
                // interior vertical bars as there are interior vertical bars on the first line.
                actualEnd = realStartOfLine;

                if (!lineStartsNewParagraph)
                {
                    return null;
                }

                // First thing to do is to check if there is a vertical bar on the line.
                var barSections = markdown.Substring((int)realStartOfLine, (int)(endOfFirstLine - realStartOfLine)).Split('|');

                var allBarsEscaped = true;

                // we can skip the last section, because there is no bar at the end of it
                for (var i = 0; i < barSections.Length - 1; i++)
                {
                    var barSection = barSections[i];
                    if (!barSection.EndsWith("\\"))
                    {
                        allBarsEscaped = false;
                        break;
                    }
                }

                if (allBarsEscaped)
                {
                    return null;
                }

                var rows = new List<TableRow>();

                // Parse the first row.
                var firstRow = new TableRow();
                realStartOfLine = firstRow.Parse(markdown, (int)realStartOfLine, maxEnd, document);
                rows.Add(firstRow);

                // Parse the contents of the second row.
                var secondRowContents = new List<string>();
                realStartOfLine = TableRow.ParseContents(
                    markdown,
                    (int)realStartOfLine,
                    maxEnd,
                    requireVerticalBar: false,
                    contentParser: (start2, end2) => secondRowContents.Add(markdown.Substring(start2, end2 - start2)));

                // There must be at least as many columns in the second row as in the first row.
                if (secondRowContents.Count < firstRow.Cells.Count)
                {
                    return null;
                }

                // Check each column definition.
                // Note: excess columns past firstRowColumnCount are ignored and can contain anything.
                var columnDefinitions = new List<TableColumnDefinition>(firstRow.Cells.Count);
                for (int i = 0; i < firstRow.Cells.Count; i++)
                {
                    var cellContent = secondRowContents[i];
                    if (cellContent.Length == 0)
                    {
                        return null;
                    }

                    // The first and last characters can be '-' or ':'.
                    if (cellContent[0] != ':' && cellContent[0] != '-')
                    {
                        return null;
                    }

                    if (cellContent[cellContent.Length - 1] != ':' && cellContent[cellContent.Length - 1] != '-')
                    {
                        return null;
                    }

                    // Every other character must be '-'.
                    for (int j = 1; j < cellContent.Length - 1; j++)
                    {
                        if (cellContent[j] != '-')
                        {
                            return null;
                        }
                    }

                    // Record the alignment.
                    var columnDefinition = new TableColumnDefinition();
                    if (cellContent.Length > 1 && cellContent[0] == ':' && cellContent[cellContent.Length - 1] == ':')
                    {
                        columnDefinition.Alignment = ColumnAlignment.Center;
                    }
                    else if (cellContent[0] == ':')
                    {
                        columnDefinition.Alignment = ColumnAlignment.Left;
                    }
                    else if (cellContent[cellContent.Length - 1] == ':')
                    {
                        columnDefinition.Alignment = ColumnAlignment.Right;
                    }

                    columnDefinitions.Add(columnDefinition);
                }

                // Parse additional rows.
                while (realStartOfLine < maxEnd)
                {
                    var row = new TableRow();
                    realStartOfLine = row.Parse(markdown, (int)realStartOfLine, maxEnd, document);
                    if (row.Cells.Count == 0)
                    {
                        break;
                    }

                    rows.Add(row);
                }

                actualEnd = realStartOfLine;
                return new TableBlock { ColumnDefinitions = columnDefinitions, Rows = rows };
            }
        }
    }
}