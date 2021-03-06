﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AdaptMark.Parsers.Markdown
{
    /// <summary>
    /// Callback provides new start and length for the line.
    /// </summary>
    /// <param name="line">The original line.</param>
    /// <param name="lineNumber">The index of the Line.</param>
    /// <returns>A tuple containing start and length of the line.</returns>
    public delegate (int start, int length, bool skipLine, bool lastLine) RemoveLineCallback(ReadOnlySpan<char> line, int lineNumber);

    /// <summary>
    /// Filters parts of a string.
    /// </summary>
    public readonly ref struct LineBlock
    {
        private readonly ReadOnlySpan<(int start, int length)> lines;

        private readonly ReadOnlySpan<char> text;

        private readonly int start;
        private readonly int fromEnd;

        /// <summary>
        /// Gets the number of lines in this Block.
        /// </summary>
        public int LineCount => this.lines.Length;

        /// <summary>
        /// Gets a single line.
        /// </summary>
        public ReadOnlySpan<char> this[int line]
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get
            {
                var result = this.text.Slice(this.lines[line].start, this.lines[line].length);
                if (line == 0)
                {
                    result = result.Slice(this.start, result.Length - this.start);
                }

                if (line == this.LineCount - 1)
                {
                    result = result.Slice(0, result.Length - this.fromEnd);
                }

                return result;
            }
        }

        /// <summary>
        /// Returns the character in the current line and clumn.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        /// <returns>The char.</returns>
        public char this[int line, int column] => this[line][column];

        /// <summary>
        /// Returns the character in the current line and clumn.
        /// </summary>
        /// <returns>The char.</returns>
        public char this[LineBlockPosition pos] => this[pos.Line][pos.Column];

        /// <summary>
        /// Gets the number of characters of this text. (Without counting linbreaks).
        /// </summary>
        public int TextLength
        {
            get
            {
                int length = 0;
                for (int i = 0; i < this.lines.Length; i++)
                {
                    length += this.lines[i].length;
                    if (i != 0)
                    {
                        length += 1;
                    }
                }

                return length - this.start - this.fromEnd;
            }
        }

        /// <summary>
        /// Concatinates the lines and uses \n as an line seperator.
        /// </summary>
        /// <returns>The concatinated StringBuilder.</returns>
        public StringBuilder ToStringBuilder()
        {
            var builderSize = this.TextLength;

            var builder = new StringBuilder(builderSize);

            PopulateBuilder(builder);

            return builder;
        }

        internal void PopulateBuilder(StringBuilder builder)
        {
            for (int i = 0; i < this.lines.Length; i++)
            {
                var from = this[i];

                builder.Append(from);
                if (i < this.LineCount - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        /// <summary>
        /// Concatinates the lines and uses \n as an line seperator.
        /// </summary>
        /// <returns>The concatinated string.</returns>
        public override string ToString()
        {
            var bufferSize = this.TextLength + ((this.LineCount - 1) * (Environment.NewLine.Length - 1));
            char[]? arrayBuffer;
            if (bufferSize <= SpanExtensions.MAX_STACK_BUFFER_SIZE)
            {
                arrayBuffer = null;
            }
            else
            {
                arrayBuffer = ArrayPool<char>.Shared.Rent(bufferSize);
            }

            Span<char> buffer = arrayBuffer != null
                ? arrayBuffer.AsSpan(0, bufferSize)
                : stackalloc char[bufferSize];

            var index = 0;
            for (int i = 0; i < this.lines.Length; i++)
            {
                var from = this[i];
                from.CopyTo(buffer.Slice(index));
                index += from.Length;
                if (index < buffer.Length)
                {
                    Environment.NewLine.AsSpan().CopyTo(buffer.Slice(index));
                    index += Environment.NewLine.Length;
                }
            }

            var result = buffer.ToString();

            if (arrayBuffer != null)
            {
                ArrayPool<char>.Shared.Return(arrayBuffer, false);
            }

            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineBlock"/> struct.
        /// </summary>
        /// <param name="data">The text that is the base for this LineBlock.</param>
        public LineBlock(ReadOnlySpan<char> data)
        {
            this.text = data;
            var current = data;
            int lineStart = 0;
            this.start = 0;
            this.fromEnd = 0;
            Span<(int start, int length)> tempSpan = stackalloc (int start, int length)[4];
            var tempSize = 0;
            while (true)
            {
                var indexCurent = current.IndexOfAny('\r', '\n');

                if (indexCurent == -1)
                {
                    if (tempSize >= tempSpan.Length)
                    {
                        var tt = tempSpan;
                        var newSize = tempSpan.Length * 2;
                        tempSpan = stackalloc (int, int)[newSize];
                        tt.CopyTo(tempSpan);
                    }

                    tempSpan[tempSize] = (lineStart, data.Length - lineStart);
                    tempSize++;

                    break;
                }

                var length = indexCurent;

                var nextLine = lineStart + indexCurent + 1;

                if (current[indexCurent] == '\r' && indexCurent + 1 < current.Length && current[indexCurent + 1] == '\n')
                {
                    nextLine += 1;
                }

                if (tempSize >= tempSpan.Length)
                {
                    var tt = tempSpan;
                    var newSize = tempSpan.Length * 2;
                    tempSpan = stackalloc (int, int)[newSize];
                    tt.CopyTo(tempSpan);
                }

                tempSpan[tempSize] = (lineStart, length);
                tempSize++;

                if (nextLine >= data.Length)
                {
                    break;
                }

                current = data.Slice(nextLine);
                lineStart = nextLine;
            }

            this.lines = new ReadOnlySpan<(int start, int length)>(tempSpan.Slice(0, tempSize).ToArray());
            System.Diagnostics.Debug.Assert(this.TextLength >= 0, "TextLength is negative");
        }

        private LineBlock(ReadOnlySpan<(int start, int length)> lines, ReadOnlySpan<char> text, int start, int fromEnd)
        {
            this.lines = lines;
            this.text = text;
            this.start = start;
            this.fromEnd = fromEnd;
            System.Diagnostics.Debug.Assert(this.TextLength >= 0, "TextLength is negative");
        }

        /// <summary>
        /// Slices the Lines.
        /// </summary>
        /// <param name="start">The startindex.</param>
        /// <param name="length">The number of lines.</param>
        /// <returns>A new LineBlock that will only have the lines specified.</returns>
        public LineBlock SliceLines(int start, int length)
        {
            int startModification;
            int endModification;
            if (start == 0)
            {
                startModification = this.start;
            }
            else
            {
                startModification = 0;
            }

            if (length == this.LineCount - start)
            {
                endModification = this.fromEnd;
            }
            else
            {
                endModification = 0;
            }

            var slicedLines = this.lines.Slice(start, length);
            if (slicedLines.Length == 0)
            {
                startModification = 0;
                endModification = 0;
            }

            var lineBlock = new LineBlock(slicedLines, this.text, startModification, endModification);

            System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");
            return lineBlock;
        }

        /// <summary>
        /// Slices the Lines.
        /// </summary>
        /// <param name="start">The startindex.</param>
        /// <returns>A new LineBlock that will only have the lines specified.</returns>
        public LineBlock SliceLines(int start)
        {
            return this.SliceLines(start, this.LineCount - start);
        }

        /// <summary>
        /// Will remove a specific number of characters from the start of each line.
        /// If a line has less charaters then removed the line will have 0 characers.
        /// </summary>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>A new Instance of LineBlock with the lines modified.</returns>
        public LineBlock RemoveFromLineStart(int length)
        {
            var temp = new (int start, int length)[this.lines.Length];

            for (int i = 0; i < temp.Length; i++)
            {
                ref var toSet = ref temp[i];
                toSet = this.lines[i];
                toSet.length -= length;

                if (i == 0)
                {
                    toSet.length -= this.start;
                    toSet.start += this.start;
                }

                if (i == temp.Length - 1)
                {
                    toSet.length -= this.fromEnd;
                }

                if (toSet.length < 0)
                {
                    toSet.length = 0;
                    toSet.start = 0;
                }
                else
                {
                    toSet.start += length;
                }
            }

            var lineBlock = new LineBlock(temp.AsSpan(), this.text, 0, 0);

            System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");
            return lineBlock;
        }

        /// <summary>
        /// Will remove a specific number of characters from the end of each line.
        /// If a line has less charaters then removed the line will have 0 characers.
        /// </summary>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>A new Instance of LineBlock with the lines modified.</returns>
        public LineBlock RemoveFromLineEnd(int length)
        {
            var temp = new (int start, int length)[this.lines.Length];

            for (int i = 0; i < temp.Length; i++)
            {
                ref var toSet = ref temp[i];
                toSet = this.lines[i];
                toSet.length -= length;

                if (i == 0)
                {
                    toSet.length -= this.start;
                    toSet.start += this.start;
                }

                if (i == temp.Length - 1)
                {
                    toSet.length -= this.fromEnd;
                }

                if (toSet.length < 0)
                {
                    toSet.length = 0;
                    toSet.start = 0;
                }
            }

            var lineBlock = new LineBlock(temp.AsSpan(), this.text, 0, 0);
            System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");

            return lineBlock;
        }

        /// <summary>
        /// Will remove a specific number of characters from each line.
        /// A callback is called for each line and returns the new start and length.
        /// </summary>
        /// <param name="callback">The callback that will be called for each line.</param>
        /// <returns>A new Instance of LineBlock with the lines modified.</returns>
        public LineBlock RemoveFromLine(RemoveLineCallback callback)
        {
            Span<(int start, int length)> temp = stackalloc (int start, int length)[this.lines.Length];
            var skipedLines = 0;
            var linesTaken = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                ref readonly var from = ref this.lines[i];
                ref var toSet = ref temp[i - skipedLines];

                var (newStart, newLength, skip, isLastLine) = callback(this[i], i);

                if (skip)
                {
                    skipedLines++;
                }
                else
                {
                    linesTaken++;

                    if (newStart < 0 || newStart > from.length || newStart + newLength > from.length)
                    {
                        throw new ArgumentOutOfRangeException($"The supplied argument were out of range. New string must <={from.length}");
                    }

                    toSet.start = newStart + from.start;
                    toSet.length = newLength;
                    if (i == 0)
                    {
                        toSet.start += this.start;
                        toSet.length -= this.start;
                    }
                }

                if (isLastLine)
                {
                    break;
                }
            }

            var lineBlock = new LineBlock(temp.Slice(0, linesTaken).ToArray(), this.text, 0, 0);
            System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");

            return lineBlock;
        }

        /// <summary>
        /// Removes A specific amouts of charactesr. Empty lines will be removed.
        /// </summary>
        /// <param name="start">The position from where characters will be kept.</param>
        /// <returns>The modified Block.</returns>
        public LineBlock SliceText(int start) => this.SliceText(start, -1);

        /// <summary>
        /// Removes A specific amouts of charactesr. Empty lines will be removed.
        /// </summary>
        /// <param name="start">The position from where characters will be kept.</param>
        /// <returns>The modified Block.</returns>
        public LineBlock SliceText(LineBlockPosition start) => this.SliceText(start, -1);

        /// <summary>
        /// Removes A specific amouts of charactesr. Empty lines will be removed.
        /// </summary>
        /// <param name="start">The position from where characters will be kept.</param>
        /// <param name="length">The number of characters taken.</param>
        /// <returns>The modified Block.</returns>
        public LineBlock SliceText(LineBlockPosition start, int length)
        {
            // it is more prformant to remove the lines first.
            var slicedLines = this.SliceLines(start.Line);
            var slicedStart = slicedLines.SliceText(start.Column);
            var slicedEnd = slicedStart.SliceText(0, length);
            return slicedEnd;
        }

        /// <summary>
        /// Removes A specific amouts of charactesr. Empty lines will be removed.
        /// </summary>
        /// <param name="start">The position from where characters will be kept.</param>
        /// <param name="length">The number of characters taken.</param>
        /// <returns>The modified Block.</returns>
        public LineBlock SliceText(int start, int length)
        {
            if (length == -1 && start > this.TextLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (start + length > this.TextLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            int restStart = start;
            int restLength = length;

            var removedLines = 0;
            var newStart = this.start;
            var newEnd = this.fromEnd;
            LineBlock temp;

            if (restStart != 0)
            {
                for (int i = 0; i < this.LineCount; i++)
                {
                    var currentLine = this[i];

                    if (restStart <= currentLine.Length)
                    {
                        if (i == 0)
                        {
                            newStart += restStart;
                        }
                        else
                        {
                            newStart = restStart;
                        }

                        break;
                    }
                    else
                    {
                        restStart -= currentLine.Length;
                        if (restStart > 0)
                        {
                            removedLines++;
                            restStart--;
                        }
                    }
                }

                var slicedLines = this.lines.Slice(removedLines);
                temp = new LineBlock(slicedLines, this.text, newStart, newEnd);
                System.Diagnostics.Debug.Assert(temp.TextLength <= this.TextLength, "TextLength must be less then or equals this");
            }
            else
            {
                temp = this;
            }

            if (restLength == -1)
            {
                return temp;
            }

            removedLines = temp.LineCount;
            for (int i = 0; i < temp.LineCount; i++)
            {
                var currentLine = temp[i];

                if (restLength <= currentLine.Length)
                {
                    if (i == temp.LineCount - 1)
                    {
                        newEnd += currentLine.Length - restLength;
                    }
                    else
                    {
                        newEnd = currentLine.Length - restLength;
                    }

                    removedLines--;
                    break;
                }
                else
                {
                    restLength -= currentLine.Length;
                    if (restLength > 0)
                    {
                        removedLines--;
                        restLength--;
                    }
                }
            }

            var newLines = temp.lines.Slice(0, temp.LineCount - removedLines);
            newStart = temp.start;
            if (newLines.Length == 0)
            {
                newStart = 0;
                newEnd = 0;
            }

            var lineBlock = new LineBlock(newLines, temp.text, newStart, newEnd);

            System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");
            return lineBlock;
        }

        /// <summary>
        /// Removes Whitespace and empty lines from the end.
        /// </summary>
        /// <returns>The modefied LineBlock.</returns>
        public LineBlock TrimEnd()
        {
            for (int i = this.LineCount - 1; i >= 0; i--)
            {
                var currentLine = this[i];

                // find the last line that has text.
                if (!currentLine.IsWhiteSpace())
                {
                    var lastLine = this[i];
                    var trimed = lastLine.TrimEnd();
                    var lastLineEntry = this.lines[i];
                    var newFromEnd = lastLineEntry.length - trimed.Length;
                    if (i == 0)
                    {
                        newFromEnd -= this.start;
                    }

                    var lineBlock = new LineBlock(this.lines.Slice(0, i + 1), this.text, this.start, newFromEnd);

                    System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");
                    return lineBlock;
                }
            }

            return default;
        }

        /// <summary>
        /// Removes Whitespace and empty lines from the start.
        /// </summary>
        /// <returns>The modefied LineBlock.</returns>
        public LineBlock TrimStart()
        {
            for (int i = 0; i < this.LineCount; i++)
            {
                var currentLine = this[i];

                // find the last line that has text.
                if (!currentLine.IsWhiteSpace())
                {
                    var lastLine = this[i];
                    var trimed = lastLine.TrimStart();
                    var lastLineEntry = this.lines[i];
                    var newStart = lastLineEntry.length - trimed.Length;

                    // when this is the last line we may not forget fromEnd
                    if (i == this.LineCount - 1)
                    {
                        newStart -= this.fromEnd;
                    }

                    var lineBlock = new LineBlock(this.lines.Slice(i), this.text, newStart, this.fromEnd);

                    System.Diagnostics.Debug.Assert(lineBlock.TextLength <= this.TextLength, "TextLength must be less then or equals this");
                    return lineBlock;
                }
            }

            return default;
        }

        /// <summary>
        /// Returns the position of the string.
        /// </summary>
        /// <param name="value">The text to search.</param>
        /// <returns>The line and index in the line.</returns>
        public LineBlockPosition IndexOf(ReadOnlySpan<char> value)
        {
            return this.IndexOf(value, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns the position of the string.
        /// </summary>
        /// <param name="value">The text to search.</param>
        /// <param name="comparisonType">Defines the comparision.</param>
        /// <returns>The line and index in the line.</returns>
        public LineBlockPosition IndexOf(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            int lengthOfPreviouseLies = 0;
            for (int i = 0; i < this.LineCount; i++)
            {
                var index = this[i].IndexOf(value, comparisonType);
                if (index >= 0)
                {
                    return new LineBlockPosition(i, index, index + lengthOfPreviouseLies);
                }

                lengthOfPreviouseLies += this[i].Length + 1;
            }

            return LineBlockPosition.NotFound;
        }

        /// <summary>
        /// Helps caching some information for mor performant search.
        /// </summary>
        public readonly struct IndexOfAnyInput : IEquatable<IndexOfAnyInput>
        {
            /// <summary>
            /// The Values that will be searched.
            /// </summary>
            public readonly ReadOnlyMemory<char> Values;

            /// <summary>
            /// Caches if any of the character is a letter or digit.
            /// </summary>
            public readonly bool HasLettersOrDigits;

            /// <summary>
            /// Caches if any of the Characters is a white space.
            /// </summary>
            public readonly bool HasWhiteSpace;

            /// <summary>
            /// Initializes a new instance of the <see cref="IndexOfAnyInput"/> struct.
            /// </summary>
            public IndexOfAnyInput(ReadOnlyMemory<char> values)
            {
                this.Values = values;
                HasLettersOrDigits = false;
                HasWhiteSpace = false;

                var span = values.Span;

                // we want to prevent iterating over every char if it is
                // not needed. This method is primaryly called to find
                // trip chars. almost non trip char is a letter or digit.
                // But almost every character in a text is a letter.
                // So checking for letter will prevent iterating.
                for (int i = 0; i < span.Length; i++)
                {
                    if (char.IsLetterOrDigit(span[i]))
                    {
                        this.HasLettersOrDigits = true;
                        break;
                    }
                }

                for (int i = 0; i < span.Length; i++)
                {
                    if (char.IsWhiteSpace(span[i]))
                    {
                        this.HasWhiteSpace = true;
                        break;
                    }
                }
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return obj is IndexOfAnyInput input && this.Equals(input);
            }

            /// <inheritdoc/>
            public bool Equals(IndexOfAnyInput other)
            {
                return EqualityComparer<ReadOnlyMemory<char>>.Default.Equals(this.Values, other.Values);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return 1291433875 + this.Values.GetHashCode();
            }

            /// <summary>
            /// Compares the Values.
            /// </summary>
            public static bool operator ==(IndexOfAnyInput left, IndexOfAnyInput right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Compares the values for inequality.
            /// </summary>
            public static bool operator !=(IndexOfAnyInput left, IndexOfAnyInput right)
            {
                return !(left == right);
            }
        }

        /// <summary>
        /// Returns the position of any supled chars.
        /// </summary>
        /// <param name="input">The characters to search.</param>
        /// <param name="fromPosition">The position from where to start the search.</param>
        /// <returns>The line and index in the line.</returns>
        public LineBlockPosition IndexOfAny(IndexOfAnyInput input, LineBlockPosition fromPosition)
        {
            if (this.lines.Length == 0)
            {
                return LineBlockPosition.NotFound;
            }

            var values = input.Values.Span;
            if (values.Length == 1)
            {
                var toSearch = values[0];
                var to = this.lines[this.lines.Length - 1].start + this.lines[this.lines.Length - 1].length - fromEnd;
                var line = fromPosition.Line;
                var currentLineStart = this.lines[line].start;
                if (line == 0)
                {
                    currentLineStart += this.start;
                }

                var nextLineStart = int.MaxValue;
                var thisLineEnd = this.lines[line].start + this.lines[line].length;
                var numberOfLineBrackeCharacters = 0;
                if (line + 1 < this.lines.Length)
                {
                    nextLineStart = this.lines[line + 1].start;
                }

                for (int i = this.lines[0].start + fromPosition.FromStart + this.start; i < to; i++)
                {
                    if (i > thisLineEnd)
                    {
                        numberOfLineBrackeCharacters++;
                    }

                    if (i >= nextLineStart)
                    {
                        currentLineStart = nextLineStart;
                        line++;
                        thisLineEnd = this.lines[line].start + this.lines[line].length;
                        if (line + 1 < this.lines.Length)
                        {
                            nextLineStart = this.lines[line + 1].start;
                        }
                        else
                        {
                            nextLineStart = int.MaxValue;
                        }
                    }

                    if (this.text[i] == toSearch)
                    {
                        var positionFromStart = i;

                        // remove the line breakCharacter
                        positionFromStart -= numberOfLineBrackeCharacters;

                        // But add one for each actuall linebreak
                        positionFromStart += line - fromPosition.Line;

                        // remove the start offset
                        positionFromStart -= this.start + this.lines[0].start;

                        return new LineBlockPosition(line, i - currentLineStart, positionFromStart);
                    }
                }

                return LineBlockPosition.NotFound;
            }
            else
            {
                var to = this.lines[this.lines.Length - 1].start + this.lines[this.lines.Length - 1].length - fromEnd;
                var line = fromPosition.Line;
                var currentLineStart = this.lines[line].start;
                if (line == 0)
                {
                    currentLineStart += this.start;
                }

                var nextLineStart = int.MaxValue;
                var thisLineEnd = this.lines[line].start + this.lines[line].length;
                var numberOfLineBrackeCharacters = 0;
                if (line + 1 < this.lines.Length)
                {
                    nextLineStart = this.lines[line + 1].start;
                }

                for (int i = this.lines[0].start + fromPosition.FromStart + this.start; i < to; i++)
                {



                    var currentChar = this.text[i];
                    if (i > thisLineEnd)
                    {
                        numberOfLineBrackeCharacters++;
                    }

                    if (i >= nextLineStart)
                    {
                        currentLineStart = nextLineStart;
                        line++;
                        thisLineEnd = this.lines[line].start + this.lines[line].length;
                        if (line + 1 < this.lines.Length)
                        {
                            nextLineStart = this.lines[line + 1].start;
                        }
                        else
                        {
                            nextLineStart = int.MaxValue;
                        }
                    }

                    // && (!nonIsDigit | !char.IsDigit(currentChar))
                    // && (!nonIsLetter | !char.IsLetter(currentChar)))
                    // if (((currentChar & oneFilter) == oneFilter)
                    //    && ((currentChar | zeroFilter) == zeroFilter))
                    if ((input.HasLettersOrDigits || !char.IsLetterOrDigit(currentChar))
                        && (input.HasWhiteSpace || !char.IsWhiteSpace(currentChar)))
                    {
                        bool found = false;
                        for (int j = 0; j < values.Length; j++)
                        {
                            if (currentChar == values[j])
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            var positionFromStart = i;

                            // remove the line breakCharacter
                            positionFromStart -= numberOfLineBrackeCharacters;

                            // But add one for each actuall linebreak
                            positionFromStart += line - fromPosition.Line;

                            // remove the start offset
                            positionFromStart -= this.start + this.lines[0].start;

                            return new LineBlockPosition(line, i - currentLineStart, positionFromStart);
                        }
                    }
                }

                return LineBlockPosition.NotFound;
            }
        }

        /// <summary>
        /// Returns the position of the string.
        /// </summary>
        /// <param name="value">The text to search.</param>
        /// <returns>The line and index in the line.</returns>
        public LineBlockPosition IndexOf(char value)
        {
            ReadOnlySpan<char> toSearch = stackalloc char[]
            {
                value,
            };

            return this.IndexOf(toSearch);
        }
    }
}