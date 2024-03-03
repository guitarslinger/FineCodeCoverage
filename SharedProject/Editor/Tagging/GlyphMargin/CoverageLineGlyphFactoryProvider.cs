﻿using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using OrderAttribute = Microsoft.VisualStudio.Utilities.OrderAttribute;
using FineCodeCoverage.Editor.Tagging.Base;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    [ExcludeFromCodeCoverage]
    [ContentType(SupportedContentTypeLanguages.CSharp)]
    [ContentType(SupportedContentTypeLanguages.VisualBasic)]
    [ContentType(SupportedContentTypeLanguages.CPP)]
    [TagType(typeof(CoverageLineGlyphTag))]
	[Order(Before = "VsTextMarker")]
	[Name(Vsix.GlyphFactoryProviderName)]
	[Export(typeof(IGlyphFactoryProvider))]
	internal class CoverageLineGlyphFactoryProvider: IGlyphFactoryProvider
	{
        public IGlyphFactory GetGlyphFactory(IWpfTextView textView, IWpfTextViewMargin textViewMargin) => new CoverageLineGlyphFactory();

    }
}