﻿using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class TrackedLines_Test
    {
        private IContainingCodeTrackerProcessResult GetProcessResult(List<SpanAndLineRange> unprocessedSpans,bool changed = true,bool isEmpty = false)
        {
            var mockContainingCodeTrackerProcessResult = new Mock<IContainingCodeTrackerProcessResult>();
            mockContainingCodeTrackerProcessResult.SetupGet(containingCodeTrackerProcessResult => containingCodeTrackerProcessResult.UnprocessedSpans).Returns(unprocessedSpans);
            mockContainingCodeTrackerProcessResult.SetupGet(containingCodeTrackerProcessResult => containingCodeTrackerProcessResult.Changed).Returns(changed);
            mockContainingCodeTrackerProcessResult.SetupGet(containingCodeTrackerProcessResult => containingCodeTrackerProcessResult.IsEmpty).Returns(isEmpty);
            return mockContainingCodeTrackerProcessResult.Object;
        }

        [Test]
        public void Should_Process_Changes_With_Unprocessed_Spans()
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            var newSpanChanges = new List<Span> { new Span(10, 10) };
            mockTextSnapshot.Setup(textSnapshot => textSnapshot.GetLineNumberFromPosition(10)).Returns(1);
            mockTextSnapshot.Setup(textSnapshot => textSnapshot.GetLineNumberFromPosition(20)).Returns(2);

            var changes = new List<SpanAndLineRange> { new SpanAndLineRange(new Span(15, 5), 1, 1) };
            var unprocessedSpans = new List<SpanAndLineRange> { new SpanAndLineRange(new Span(10, 5), 0, 1) };
            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker1.Setup(
                containingCodeTracker => containingCodeTracker.ProcessChanges(
                    mockTextSnapshot.Object,
                    new List<SpanAndLineRange> { new SpanAndLineRange(newSpanChanges[0], 1, 2) }))
                .Returns(GetProcessResult(unprocessedSpans));

            var mockContainingCodeTracker2 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker2.Setup(containingCodeTracker => containingCodeTracker.ProcessChanges(mockTextSnapshot.Object, unprocessedSpans))
                .Returns(GetProcessResult(unprocessedSpans));

            var trackedLines = new TrackedLines(
                new List<IContainingCodeTracker> { mockContainingCodeTracker1.Object, mockContainingCodeTracker2.Object }, null, null);
            trackedLines.Changed(mockTextSnapshot.Object, newSpanChanges);

            mockContainingCodeTracker1.VerifyAll();
            mockContainingCodeTracker2.VerifyAll();

        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Changed_If_ContainingCodeTracker_Changed(bool firstChanged)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();

            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker1.Setup(
                containingCodeTracker => containingCodeTracker.ProcessChanges(
                    mockTextSnapshot.Object,
                    It.IsAny<List<SpanAndLineRange>>()))
                .Returns(GetProcessResult(new List<SpanAndLineRange>(), firstChanged));

            var mockContainingCodeTracker2 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker2.Setup(containingCodeTracker => containingCodeTracker.ProcessChanges(mockTextSnapshot.Object, It.IsAny<List<SpanAndLineRange>>()))
                .Returns(GetProcessResult(new List<SpanAndLineRange>(), !firstChanged));

            var trackedLines = new TrackedLines(
                new List<IContainingCodeTracker> { mockContainingCodeTracker1.Object, mockContainingCodeTracker2.Object }, null, null);
            var changed = trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());

            Assert.That(changed, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Remove_ContainingCodeTracker_When_Empty(bool isEmpty)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();

            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker1.Setup(
                containingCodeTracker => containingCodeTracker.ProcessChanges(
                    mockTextSnapshot.Object,
                    It.IsAny<List<SpanAndLineRange>>()))
                .Returns(GetProcessResult(new List<SpanAndLineRange>(), false, isEmpty));

            var trackedLines = new TrackedLines(new List<IContainingCodeTracker> { mockContainingCodeTracker1.Object }, null, null);
            Assert.That(trackedLines.ContainingCodeTrackers, Is.EquivalentTo(new List<IContainingCodeTracker> { mockContainingCodeTracker1.Object }));
            trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());
            trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());

            var times = isEmpty ? Times.Once() : Times.Exactly(2);
            mockContainingCodeTracker1.Verify(
                containingCodeTracker => containingCodeTracker.ProcessChanges(mockTextSnapshot.Object, It.IsAny<List<SpanAndLineRange>>()), times);
            Assert.That(trackedLines.ContainingCodeTrackers, Has.Count.EqualTo(isEmpty ? 0 : 1));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Process_NewCodeTracker_Changes_After_ContainingCodeTrackers(bool newCodeChanged)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();

            var unprocessedSpans = new List<SpanAndLineRange> { new SpanAndLineRange(new Span(0, 10), 0, 1) };
            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker1.Setup(
                containingCodeTracker => containingCodeTracker.ProcessChanges(
                    mockTextSnapshot.Object,
                    It.IsAny<List<SpanAndLineRange>>()))
                .Returns(GetProcessResult(unprocessedSpans, false));

            var mockNewCodeTracker = new Mock<INewCodeTracker>();
            mockNewCodeTracker.Setup(newCodeTracker => newCodeTracker.ProcessChanges(mockTextSnapshot.Object, unprocessedSpans, null))
                .Returns(newCodeChanged);

            var trackedLines = new TrackedLines(
                new List<IContainingCodeTracker> { mockContainingCodeTracker1.Object }, 
                mockNewCodeTracker.Object,
                null);
            var changed = trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());

            Assert.That(changed, Is.EqualTo(newCodeChanged));

        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_GetState_Of_Not_Empty_ContainingCodeTrackers_When_NewCodeTracker_And_FileCodeSpanRangeService(bool isEmpty)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();

            var mockContainingCodeTracker = new Mock<IContainingCodeTracker>(MockBehavior.Strict);
            mockContainingCodeTracker.Setup(
                containingCodeTracker => containingCodeTracker.ProcessChanges(
                    mockTextSnapshot.Object,
                    It.IsAny<List<SpanAndLineRange>>()))
                .Returns(GetProcessResult(new List<SpanAndLineRange>(), false,isEmpty));

            if (!isEmpty)
            {
                mockContainingCodeTracker.Setup(
                    containingCodeTracker => containingCodeTracker.GetState()
                ).Returns(new ContainingCodeTrackerState(ContainingCodeTrackerType.OtherLines, CodeSpanRange.SingleLine(1), Enumerable.Empty<IDynamicLine>()));
            }
            

            var mockFileCodeSpanRangeService = new Mock<IFileCodeSpanRangeService>();
            mockFileCodeSpanRangeService.Setup(fileCodeSpanRangeService => fileCodeSpanRangeService.GetFileCodeSpanRanges(mockTextSnapshot.Object))
                .Returns(new List<CodeSpanRange>());
            var trackedLines = new TrackedLines(
                new List<IContainingCodeTracker> { mockContainingCodeTracker.Object },
                new Mock<INewCodeTracker>().Object,
                mockFileCodeSpanRangeService.Object);
            
            trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());

            mockContainingCodeTracker.VerifyAll();
        }

        [TestCaseSource(typeof(ApplyNewCodeCodeRangesTestData),nameof(ApplyNewCodeCodeRangesTestData.TestCases))]
        public void Should_ApplyNewCodeCodeRanges(
            List<CodeSpanRange> containingCodeTrackersCodeSpanRanges, 
            List<CodeSpanRange> fileCodeSpanRanges, 
            List<CodeSpanRange> expectedApplyNewCodeCodeRanges,
            bool containingCodeTrackersChanged,
            bool newCodeTrackerProcessChangesResult,
            bool expectedChanged
        )
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            var containingCodeTrackers = containingCodeTrackersCodeSpanRanges.Select(codeSpanRange =>
            {
                var mockContainingCodeTracker = new Mock<IContainingCodeTracker>();
                mockContainingCodeTracker.Setup(containingCodeTracker => containingCodeTracker.GetState())
                    .Returns(new ContainingCodeTrackerState(ContainingCodeTrackerType.OtherLines, codeSpanRange, Enumerable.Empty<IDynamicLine>()));
                mockContainingCodeTracker.Setup(containingCodeTracker => containingCodeTracker.ProcessChanges(
                    It.IsAny<ITextSnapshot>(),
                    It.IsAny<List<SpanAndLineRange>>())
                ).Returns(GetProcessResult(new List<SpanAndLineRange>(), containingCodeTrackersChanged));
                return mockContainingCodeTracker.Object;
            }).ToList();

            var mockFileCodeSpanRangeService = new Mock<IFileCodeSpanRangeService>();
            mockFileCodeSpanRangeService.Setup(fileCodeSpanRangeService => fileCodeSpanRangeService.GetFileCodeSpanRanges(mockTextSnapshot.Object))
                .Returns(fileCodeSpanRanges);

            var mockNewCodeTracker = new Mock<INewCodeTracker>();
            mockNewCodeTracker.Setup(newCodeTracker => newCodeTracker.ProcessChanges(
                mockTextSnapshot.Object, It.IsAny<List<SpanAndLineRange>>(), expectedApplyNewCodeCodeRanges)
            ).Returns(newCodeTrackerProcessChangesResult);

            var trackedLines = new TrackedLines(containingCodeTrackers, mockNewCodeTracker.Object, mockFileCodeSpanRangeService.Object);

            var changed = trackedLines.Changed(mockTextSnapshot.Object, new List<Span>());

            Assert.That(expectedChanged, Is.EqualTo(changed));
            mockNewCodeTracker.VerifyAll();
        }


        public class ApplyNewCodeCodeRangesTestData
        {
            public class ApplyNewCodeCodeRangesTestCase : TestCaseData
            {
                public ApplyNewCodeCodeRangesTestCase(
                    List<CodeSpanRange> containingCodeTrackersCodeSpanRanges,
                    List<CodeSpanRange> fileCodeSpanRanges,
                    List<CodeSpanRange> expectedApplyNewCodeCodeRanges,
                    bool containingCodeTrackersChanged = true,
                    bool newCodeTrackerProcessChangesResult = true,
                    bool expectedChanged = true,
                    string testName = null
                    ) : base(
                        containingCodeTrackersCodeSpanRanges, 
                        fileCodeSpanRanges, 
                        expectedApplyNewCodeCodeRanges,
                        containingCodeTrackersChanged,
                        newCodeTrackerProcessChangesResult,
                        expectedChanged
                        )
                {
                    if (testName != null)
                    {
                        this.SetName(testName);
                    }
                }
            }

            public static IEnumerable<ApplyNewCodeCodeRangesTestCase> TestCases
            {
                get
                {   // removes exact match
                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1), CodeSpanRange.SingleLine(2) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(2) }
                     );
                    
                    // new at the beginning
                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(2) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) }, 
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) }
                     );

                    //new at the end
                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(2) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(2) }
                     );

                    // removes intersecting
                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) },
                        new List<CodeSpanRange> { new CodeSpanRange(0,2)},
                        new List<CodeSpanRange> { }
                     );


                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1),},
                        new List<CodeSpanRange> { },
                        true,
                        false,
                        true
                     );

                    yield return new ApplyNewCodeCodeRangesTestCase(
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1) },
                        new List<CodeSpanRange> { CodeSpanRange.SingleLine(1), },
                        new List<CodeSpanRange> { },
                        false,
                        true,
                        true
                     );

                }
            }
        }


        private static IDynamicLine CreateDynamicLine(int lineNumber)
        {
            var mockDynamicLine = new Mock<IDynamicLine>();
            mockDynamicLine.SetupGet(x => x.Number).Returns(lineNumber);
            return mockDynamicLine.Object;
        }

        [Test]
        public void Should_Return_Lines_From_ContainingCodeTrackers()
        {
            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            var expectedLines = new List<IDynamicLine>
            {
                CreateDynamicLine(10),
                CreateDynamicLine(11),
                CreateDynamicLine(18),
                CreateDynamicLine(19),
                CreateDynamicLine(20),
            };
            mockContainingCodeTracker1.Setup(x => x.Lines).Returns(new List<IDynamicLine>
            {
                CreateDynamicLine(9),
                expectedLines[0],
                expectedLines[1]
            });
            var mockContainingCodeTracker2 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker2.Setup(x => x.Lines).Returns(new List<IDynamicLine>
            {
                expectedLines[2],
                expectedLines[3],
                expectedLines[4],
            });

            var trackedLines = new TrackedLines(new List<IContainingCodeTracker>
            {
                mockContainingCodeTracker1.Object,
                mockContainingCodeTracker2.Object
            }, null, null);

            var lines = trackedLines.GetLines(10, 20);
            Assert.That(lines, Is.EqualTo(expectedLines));
        }

        [Test]
        public void Should_Return_Lines_From_ContainingCodeTrackers_Exiting_Early()
        {
            var mockContainingCodeTracker1 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker1.Setup(x => x.Lines).Returns(new List<IDynamicLine>
             {
                 CreateDynamicLine(10),
             });
            var mockContainingCodeTracker2 = new Mock<IContainingCodeTracker>();
            mockContainingCodeTracker2.Setup(x => x.Lines).Returns(new List<IDynamicLine>
             {
                 CreateDynamicLine(21),
             });

            var notCalledMockContainingCodeTracker = new Mock<IContainingCodeTracker>(MockBehavior.Strict);

            var trackedLines = new TrackedLines(new List<IContainingCodeTracker>
             {
                 mockContainingCodeTracker1.Object,
                 mockContainingCodeTracker2.Object,
                 notCalledMockContainingCodeTracker.Object
             }, null, null);

            var lines = trackedLines.GetLines(10, 20).ToList();

            mockContainingCodeTracker1.VerifyAll();
            mockContainingCodeTracker2.VerifyAll();
        }

        [Test]
        public void Should_Return_Lines_From_NewCodeTracker_But_Not_If_Already_From_ContainingCodeTrackers()
        {
            var expectedLines = new List<IDynamicLine>
            {
                CreateDynamicLine(10),
                CreateDynamicLine(15),
            };
            var mockContainingCodeTracker = new Mock<IContainingCodeTracker>();

            mockContainingCodeTracker.Setup(x => x.Lines).Returns(new List<IDynamicLine>
             {
                 expectedLines[0]
             });

            var mockNewCodeTracker = new Mock<INewCodeTracker>();
            mockNewCodeTracker.SetupGet(newCodeTracker => newCodeTracker.Lines).Returns(new List<IDynamicLine>
            {
                 CreateDynamicLine(2),
                 CreateDynamicLine(10),
                 expectedLines[1],
                 CreateDynamicLine(50),
             });
            var trackedLines = new TrackedLines(new List<IContainingCodeTracker>
             {
                 mockContainingCodeTracker.Object,
             }, mockNewCodeTracker.Object, null);

            var lines = trackedLines.GetLines(10, 20).ToList();
            Assert.That(lines, Is.EqualTo(expectedLines));
        }
    }
}
