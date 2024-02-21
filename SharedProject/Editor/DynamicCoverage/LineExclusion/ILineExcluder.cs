﻿using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ILineExcluder
    {
        bool ExcludeIfNotCode(string text, bool isCSharp);
    }
}
