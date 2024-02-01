﻿using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Impl
{
    [ContentType("code")]
    [TagType(typeof(IClassificationTag))]
    [Name("FCC.CoverageLineClassificationTaggerProvider")]
    [Export(typeof(ITaggerProvider))]
    internal class CoverageLineClassificationTaggerProvider : ITaggerProvider, ILineSpanTagger<IClassificationTag>
    {
        private readonly ICoverageTypeService coverageTypeService;
        private readonly ICoverageTaggerProvider<IClassificationTag> coverageTaggerProvider;

        [ImportingConstructor]
        public CoverageLineClassificationTaggerProvider(
            ICoverageTypeService coverageTypeService,
             ICoverageTaggerProviderFactory coverageTaggerProviderFactory
        )
        {
            this.coverageTypeService = coverageTypeService;
            this.coverageTaggerProvider =  coverageTaggerProviderFactory.Create<IClassificationTag, CoverageClassificationFilter>(this);
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return coverageTaggerProvider.CreateTagger(buffer) as ITagger<T>;
        }

        public TagSpan<IClassificationTag> GetTagSpan(ILineSpan lineSpan)
        {
            var ct = coverageTypeService.GetClassificationType(lineSpan.Line.CoverageType);
            return new TagSpan<IClassificationTag>(lineSpan.Span, new ClassificationTag(ct));
        }
    }
}
