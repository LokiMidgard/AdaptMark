// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AdaptMark.Parsers.Markdown;
using AdaptMark.Parsers.Markdown.Blocks;
using AdaptMark.Parsers.Markdown.Inlines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests.Markdown.Parse
{
    [TestClass]
    public class ListTests : ParseTestBase
    {
        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_SingleLine()
        {
            AssertEqual("- List",
                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Simple()
        {
            AssertEqual(CollapseWhitespace(@"
                before

                - List item 1
                * List item 2
                + List item 3

                after"),
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "before" }),
                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))),
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "after" }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_BlankLineIsOkay()
        {
            AssertEqual(CollapseWhitespace(@"
                * List item 1

                * List item 2"),
                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_WithBlocks()
        {
            // Not sure why it should not be a header?
            // But according to commonMark it is.
            AssertEqual(CollapseWhitespace(@"
                * #Header

                  ___
                *  #Not a header

                   ___"),
                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                    new ListItemBlock().AddChildren(
                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Header" }),
                        new HorizontalRuleBlock()),
                    new ListItemBlock().AddChildren(
                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Not a header" }),
                        new HorizontalRuleBlock())));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_Simple()
        {
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                    - Nested item
                + List item 2"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "Nested item" })))),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_Complex()
        {
            // This is super weird.
            // It now should expect same output as commonMark https://spec.commonmark.org/dingus/?text=-%20%23%20Level%201%0A-%20%23%20Level%201%0A%20%20%20%20-%20%23%20Level%202%0A%20%20%20%20%20%20%20%20-%20%23%20Level%203%0A%20%20%20%20%20%20%20%20%20%20%20%20-%20%23%20Level%204%20%0Alevel%204%2C%20line%202%0A%0A%20%20%20%20%20text
            AssertEqual(CollapseWhitespace(@"
                - #Level 1
                - #Level 1
                    - #Level 2
                        - #Level 3
                            - #Level 4  
                level 4, line 2

                     text"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 1" })),
                    new ListItemBlock().AddChildren(
                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 1" }),
                        new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                            new ListItemBlock().AddChildren(
                                new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 2" }),
                                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                                    new ListItemBlock().AddChildren(
                                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 3" }),
                                        new ListBlock { Style = ListStyle.Bulleted }.AddChildren(new ListItemBlock().AddChildren(new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 4" }))))))))),
                new ParagraphBlock().AddChildren(new TextRunInline { Text = "level 4, line 2" }),
                new CodeBlock { Text = " text" });

            AssertEqual(CollapseWhitespace(@"
                - #Level 1
                - #Level 1
                    - #Level 2
                        - #Level 3
                            - #Level 4  
                              level 4, line 2

                      text level 2"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 1" })),
                    new ListItemBlock().AddChildren(
                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 1" }),
                        new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                            new ListItemBlock().AddChildren(
                                new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 2" }),
                                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                                    new ListItemBlock().AddChildren(
                                        new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 3" }),
                                        new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                                            new ListItemBlock().AddChildren(
                                                new HeaderBlock { HeaderLevel = 1 }.AddChildren(new TextRunInline { Text = "Level 4" }),
                                                new ParagraphBlock().AddChildren(new TextRunInline { Text = "level 4, line 2" }))))),
                                new ParagraphBlock().AddChildren(new TextRunInline { Text = "text level 2" }))))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_MinSpace()
        {
            // the - must be at least indented as the previous item to be a sup item
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                  - Nested item"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "Nested item" }))))));

            // if it is less it is on the same level
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                 - Nested item"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "Nested item" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_MaxSpace()
        {
            // 7 spaces is the max relative indentation for two items.
            // 3 spaces more indention then the previous is maximum
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                     - Nested item"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "Nested item" }))))));

            // if it is more, its actual additiona text for the previous item.
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                      - Nested item"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1 - Nested item" }))));
        }

        // This is not how commonMark behaves
        [Ignore]
        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_SpaceDifference()
        {
            // This is weird.
            AssertEqual(CollapseWhitespace(@"
                 - List item 1
                - Nested item"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "Nested item" }))))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_Combo()
        {
            AssertEqual(CollapseWhitespace(@"
                   - List item 1
                - List item 2
                - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))));
            AssertEqual(CollapseWhitespace(@"
                   - List item 1
                - List item 2
                  - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                   - List item 1
                - List item 2
                   - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                   - List item 1
                - List item 2
                    - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                   - List item 1
                - List item 2
                     - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                 - List item 2
                    - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                - List item 1
                 - List item 2
                     - List item 3"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" })),
                        new ListItemBlock().AddChildren(
                            new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2" }),
                            new ListBlock().AddChildren(new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 3" }))))));
            AssertEqual(CollapseWhitespace(@"
                - 1
                 - 2
                - 3
                 - 4"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "1" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "2" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "3" })),
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "4" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Nested_Paragraph()
        {
            AssertEqual(CollapseWhitespace(@"
                * 1
                  * 2

                  3"),
            new ListBlock().AddChildren(
                new ListItemBlock().AddChildren(
                    new ParagraphBlock().AddChildren(new TextRunInline { Text = "1" }),
                    new ListBlock().AddChildren(
                        new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "2" }))),
                    new ParagraphBlock().AddChildren(new TextRunInline { Text = "3" }))));

            AssertEqual(CollapseWhitespace(@"
                * 1
                  * 2
                * 3

                     4"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "1" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "2" })))),
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "3" }),
                        new ParagraphBlock().AddChildren(new TextRunInline { Text = "4" }))));
        }

        // This test is ignored because it is written to pass the "reddit" quirks of markdown.
        // This parser doesn't conform to the reddit quirks, thus they shall not pass.
        [Ignore]
        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_NestedLists()
        {
            // No blank line means only the last line actually has a bullet.
            AssertEqual(CollapseWhitespace(@"
1. * Ordered list item 1
2. * Bullet 1 in list item 2
    * Bullet 2 in list item 2"),
                new ListBlock { Style = ListStyle.Numbered }.AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(
                            new TextRunInline { Text = "* Ordered list item 1" })),
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(
                            new TextRunInline { Text = "* Bullet 1 in list item 2" }),
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(
                                new ParagraphBlock().AddChildren(
                                    new TextRunInline { Text = "Bullet 2 in list item 2" }))))));

            // But if you put a blank line in there it works.
            AssertEqual(CollapseWhitespace(@"
1. * Ordered list item 1

2. * Bullet 1 in list item 2
    * Bullet 2 in list item 2"),
                new ListBlock { Style = ListStyle.Numbered }.AddChildren(
                    new ListItemBlock().AddChildren(
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(
                                new ParagraphBlock().AddChildren(
                                    new TextRunInline { Text = "Ordered list item 1" })))),
                    new ListItemBlock().AddChildren(
                        new ListBlock().AddChildren(
                            new ListItemBlock().AddChildren(
                                new ParagraphBlock().AddChildren(
                                    new TextRunInline { Text = "Bullet 1 in list item 2" })),
                            new ListItemBlock().AddChildren(
                                new ParagraphBlock().AddChildren(
                                    new TextRunInline { Text = "Bullet 2 in list item 2" }))))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Negative_SpaceRequired()
        {
            // The space is required.
            AssertEqual("-List",
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "-List" }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Negative_NewParagraph()
        {
            // Bulleted lists may start in same paragraph
            AssertEqual(CollapseWhitespace(@"
                before
                * List

                after"),
                new ParagraphBlock().AddChildren(new TextRunInline { Text = "before" }),
                new ListBlock { Style = ListStyle.Bulleted }.AddChildren(
                    new ListItemBlock().AddChildren(new ParagraphBlock().AddChildren(new TextRunInline { Text = "List" }))),
                new ParagraphBlock().AddChildren(new TextRunInline { Text = "after" }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void BulletedList_Negative_TooMuchSpaceToBeNested()
        {
            // 7 spaces is the maximum indentation for two items.
            AssertEqual(CollapseWhitespace(@"
                * a
                        * b"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(
                            new TextRunInline { Text = "a * b" }))));

            // 11 spaces is the maximum indentation for three items.
            AssertEqual(CollapseWhitespace(@"
                * a
                 * b
                            * c"),
                new ListBlock().AddChildren(
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(
                            new TextRunInline { Text = "a" })),
                    new ListItemBlock().AddChildren(
                        new ParagraphBlock().AddChildren(
                            new TextRunInline { Text = "b * c" }))));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void NumberedList_SingleLine()
        {
            AssertEqual("1. List",
                new ListBlock { Style = ListStyle.Numbered }.AddChildren(
                    new ListItemBlock { Blocks = new List<MarkdownBlock> { new ParagraphBlock().AddChildren(new TextRunInline { Text = "List" }) } }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void NumberedList_Numbering()
        {
            // The numbers are ignored, and they can be any length (as long as it is less then 10 digits).
            AssertEqual(CollapseWhitespace(@"
                7. List item 1
                502. List item 2
                502456456456456456456456456456456456. List item 3"),
                new ListBlock { Style = ListStyle.Numbered }.AddChildren(
                    new ListItemBlock { Blocks = new List<MarkdownBlock> { new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 1" }) } },
                    new ListItemBlock { Blocks = new List<MarkdownBlock> { new ParagraphBlock().AddChildren(new TextRunInline { Text = "List item 2 502456456456456456456456456456456456. List item 3" }) } }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void NumberedList_Negative_SpaceRequired()
        {
            // A space is required after the dot.
            AssertEqual("1.List", new ParagraphBlock().AddChildren(
                new TextRunInline { Text = "1.List" }));
        }

        [TestMethod]
        [TestCategory("Parse - block")]
        public void NumberedList_Negative_NoLetters()
        {
            // Only digits can make a numbered list.
            AssertEqual("a. List", new ParagraphBlock().AddChildren(
                new TextRunInline { Text = "a. List" }));
        }
    }
}
