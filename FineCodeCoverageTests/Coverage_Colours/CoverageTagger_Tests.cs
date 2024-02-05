﻿using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests
{
    public class CoverageTagger_Tests
    {
        [Test]
        public void Should_Listen_For_Changes()
        {
            var autoMoqer = new AutoMoqer();
            var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(coverageTagger, null));
        }

        [Test]
        public void Should_Unlisten_For_Changes_On_Dispose()
        {
            var autoMoqer = new AutoMoqer();
            var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();

            coverageTagger.Dispose();
            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.RemoveListener(coverageTagger));
        }

        [Test]
        public void Should_Raise_Tags_Changed_For_CurrentSnapshot_Range_When_NewCoverageLinesMessage()
        {
            //var autoMoqer = new AutoMoqer();
            //var mockTextBufferAndFile = autoMoqer.GetMock<ITextBufferWithFilePath>();
            //var mockTextSnapshot = new Mock<ITextSnapshot>();
            //mockTextSnapshot.SetupGet(currentSnapshot => currentSnapshot.Length).Returns(10);
            //mockTextBufferAndFile.SetupGet(textBufferAndFile => textBufferAndFile.TextBuffer.CurrentSnapshot).Returns(mockTextSnapshot.Object);

            //var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();
            //SnapshotSpan? snapshotSpan = null;
            //coverageTagger.TagsChanged += (sender, args) =>
            //{
            //    snapshotSpan = args.Span;
            //};
            //coverageTagger.Handle(new NewCoverageLinesMessage());

            //Assert.Multiple(() =>
            //{
            //    Assert.That(snapshotSpan.Value.Snapshot, Is.SameAs(mockTextSnapshot.Object));
            //    Assert.That(snapshotSpan.Value.Start.Position, Is.EqualTo(0));
            //    Assert.That(snapshotSpan.Value.End.Position, Is.EqualTo(10));
            //});
            throw new NotImplementedException();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_HasCoverage_When_Has(bool hasCoverage)
        {
            //var coverageTagger = new CoverageTagger<DummyTag>(
            //    new Mock<ITextBufferWithFilePath>().Object,
            //    hasCoverage ? new Mock<IFileLineCoverage>().Object : null,
            //    new Mock<ICoverageTypeFilter>().Object,
            //    new Mock<IEventAggregator>().Object,
            //    new Mock<ILineSpanLogic>(MockBehavior.Strict).Object,
            //    new Mock<ILineSpanTagger<DummyTag>>().Object
            //);

            //Assert.That(coverageTagger.HasCoverage, Is.EqualTo(hasCoverage));
            throw new NotImplementedException();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Raise_TagsChanged_For_CoverageTypeFilterChangedMessage_With_The_Same_TypeIdentifier_If_Has_Coverage(bool same)
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.SetInstance<ICoverageTypeFilter>(new DummyCoverageTypeFilter());
            var mockTextBufferAndFile = autoMoqer.GetMock<ITextBufferWithFilePath>();

            mockTextBufferAndFile.SetupGet(textBufferAndFile => textBufferAndFile.TextBuffer.CurrentSnapshot.Length).Returns(10);

            var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();
            var tagsChanged = false;
            coverageTagger.TagsChanged += (sender, args) =>
            {
                tagsChanged = true;
            };

            var filter = same ? new DummyCoverageTypeFilter() as ICoverageTypeFilter : new OtherCoverageTypeFilter();
            var coverageTypeFilterChangedMessage = new CoverageTypeFilterChangedMessage(filter);
            coverageTagger.Handle(coverageTypeFilterChangedMessage);

            Assert.That(tagsChanged, Is.EqualTo(same));
        }

        [Test]
        public void Should_Not_Raise_TagsChanged_For_CoverageTypeFilterChangedMessage_If_No_Coverage()
        {
            var coverageTagger = new CoverageTagger<DummyTag>(
                new Mock<ITextBufferWithFilePath>().Object,
                null,
                new DummyCoverageTypeFilter(),
                new Mock<IEventAggregator>().Object,
                new Mock<ILineSpanLogic>(MockBehavior.Strict).Object,
                new Mock<ILineSpanTagger<DummyTag>>().Object
            );
            
            var tagsChanged = false;
            coverageTagger.TagsChanged += (sender, args) =>
            {
                tagsChanged = true;
            };

            var coverageTypeFilterChangedMessage = new CoverageTypeFilterChangedMessage(new DummyCoverageTypeFilter());
            coverageTagger.Handle(coverageTypeFilterChangedMessage);

            Assert.That(tagsChanged, Is.False);
        }

        [Test]
        public void Should_Return_No_Tags_If_No_Coverage_Lines()
        {
            var coverageTagger = new CoverageTagger<DummyTag>(
                new Mock<ITextBufferWithFilePath>().Object,
                null,
                new Mock<ICoverageTypeFilter>().Object,
                new Mock<IEventAggregator>().Object,
                new Mock<ILineSpanLogic>(MockBehavior.Strict).Object,
                new Mock<ILineSpanTagger<DummyTag>>().Object
            );

            var tags = coverageTagger.GetTags(new NormalizedSnapshotSpanCollection());

            Assert.That(tags, Is.Empty);
        }



        [Test]
        public void Should_Return_No_Tags_If_ICoverageTypeFilter_Is_Disabled()
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.SetInstance<ICoverageTypeFilter>(new DummyCoverageTypeFilter { Disabled = true });
            autoMoqer.SetInstance(new Mock<ILineSpanLogic>(MockBehavior.Strict).Object);

            var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();

            var tags = coverageTagger.GetTags(new NormalizedSnapshotSpanCollection());

            Assert.That(tags, Is.Empty);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_GetLineSpans_From_LineSpanLogic_For_The_FilePath_And_Spans_When_Coverage_And_Coverage_Filter_Enabled(bool newCoverage)
        {
            throw new NotImplementedException();
            //var autoMoqer = new AutoMoqer();
            //var fileLineCoverage = autoMoqer.GetMock<IFileLineCoverage>().Object;

            //var mockTextBufferAndFile = autoMoqer.GetMock<ITextBufferWithFilePath>();
            //var mockTextSnapshot = new Mock<ITextSnapshot>();
            //mockTextSnapshot.SetupGet(currentSnapshot => currentSnapshot.Length).Returns(10);
            //mockTextBufferAndFile.SetupGet(textBufferAndFile => textBufferAndFile.TextBuffer.CurrentSnapshot).Returns(mockTextSnapshot.Object);
            //mockTextBufferAndFile.SetupGet(textBufferWithFilePath => textBufferWithFilePath.FilePath).Returns("filepath");

            //var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();
            //var spans = new NormalizedSnapshotSpanCollection();

            //var expectedFileLineCoverageForLogic = fileLineCoverage;
            //if (newCoverage)
            //{
            //    expectedFileLineCoverageForLogic = new Mock<IFileLineCoverage>().Object;
            //    coverageTagger.Handle(new NewCoverageLinesMessage { CoverageLines = expectedFileLineCoverageForLogic });
            //}

            //coverageTagger.GetTags(spans);

            //autoMoqer.Verify<ILineSpanLogic>(lineSpanLogic => lineSpanLogic.GetLineSpans(
            //   expectedFileLineCoverageForLogic, "filepath", spans));
        }

        [Test]
        public void Should_GetTagsSpans_For_Filtered_LineSpans()
        {
            throw new NotImplementedException();
            //var autoMoqer = new AutoMoqer();
            //var mockCoverageTypeFilter = autoMoqer.GetMock<ICoverageTypeFilter>();
            //mockCoverageTypeFilter.Setup(coverageTypeFilter => coverageTypeFilter.Show(CoverageType.Covered)).Returns(false);
            //mockCoverageTypeFilter.Setup(coverageTypeFilter => coverageTypeFilter.Show(CoverageType.NotCovered)).Returns(false);
            //mockCoverageTypeFilter.Setup(coverageTypeFilter => coverageTypeFilter.Show(CoverageType.Partial)).Returns(true);

            //var lineSpans = new List<ILineSpan>
            //{
            //    new LineSpan{  Line = CreateLine(CoverageType.Covered),Span = SnapshotSpanFactory.Create(1)},
            //    new LineSpan{  Line = CreateLine(CoverageType.NotCovered), Span = SnapshotSpanFactory.Create(2)},
            //    new LineSpan{  Line = CreateLine(CoverageType.Partial), Span = SnapshotSpanFactory.Create(3)},
            //};
            //var expectedLineSpan = lineSpans[2];

            //var mockLineSpanTagger = autoMoqer.GetMock<ILineSpanTagger<DummyTag>>();
            //var tagSpan = new TagSpan<DummyTag>(expectedLineSpan.Span, new DummyTag());
            //mockLineSpanTagger.Setup(lineSpanTagger => lineSpanTagger.GetTagSpan(expectedLineSpan)).Returns(tagSpan);

            //autoMoqer.Setup<ILineSpanLogic, IEnumerable<ILineSpan>>(
            //    lineSpanLogic => lineSpanLogic.GetLineSpans(
            //        It.IsAny<IFileLineCoverage>(),
            //        It.IsAny<string>(),
            //        It.IsAny<NormalizedSnapshotSpanCollection>()
            //        )
            //    )
            //    .Returns(lineSpans);

            //var coverageTagger = autoMoqer.Create<CoverageTagger<DummyTag>>();

            //var tags = coverageTagger.GetTags(new NormalizedSnapshotSpanCollection());

            //Assert.That(tags, Is.EqualTo(new[] { tagSpan }));
            //mockCoverageTypeFilter.VerifyAll();

            //ILine CreateLine(CoverageType coverageType)
            //{
            //    var mockLine = new Mock<ILine>();
            //    mockLine.SetupGet(line => line.CoverageType).Returns(coverageType);
            //    return mockLine.Object;
            //}
        }
    }
}