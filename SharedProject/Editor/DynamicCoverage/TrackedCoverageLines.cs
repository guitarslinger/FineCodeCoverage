﻿using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    class TrackedCoverageLines : ITrackedCoverageLines
    {
        private readonly List<ICoverageLine> coverageLines;

        public IEnumerable<IDynamicLine> Lines => coverageLines.Select(coverageLine => coverageLine.Line);
        public TrackedCoverageLines(List<ICoverageLine> coverageLines)
        {
            this.coverageLines = coverageLines;
        }

        public bool Update(ITextSnapshot currentSnapshot)
        {
            var changed = false;
            var removals = new List<ICoverageLine>();
            foreach (var coverageLine in coverageLines)
            {
                var updateType = coverageLine.Update(currentSnapshot);
                if (updateType == CoverageLineUpdateType.Removal)
                {
                    changed = true;
                    removals.Add(coverageLine);
                }
                else if (updateType == CoverageLineUpdateType.LineNumberChange)
                {
                    changed = true;
                }
            }
            removals.ForEach(r => coverageLines.Remove(r));
            return changed;
        }
    }

}
