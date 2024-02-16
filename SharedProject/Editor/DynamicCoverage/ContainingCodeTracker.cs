﻿using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class ContainingCodeTracker : IContainingCodeTracker
    {
        private readonly ITrackingSpanRange trackingSpanRange;
        private readonly ITrackedCoverageLines trackedCoverageLines;
        private DirtyLine dirtyLine;

        public ContainingCodeTracker(ITrackedCoverageLines trackedCoverageLines, ITrackingSpanRange trackingSpanRange = null)
        {
            this.trackingSpanRange = trackingSpanRange;
            this.trackedCoverageLines = trackedCoverageLines;
        }

        private TrackingSpanRangeProcessResult ProcessTrackingSpanRangeChanges(ITextSnapshot currentSnapshot, List<SpanAndLineRange> newSpanChanges)
        {
            if (trackingSpanRange == null) return new TrackingSpanRangeProcessResult(newSpanChanges,false,false);

            return trackingSpanRange.Process(currentSnapshot, newSpanChanges);
        }

        private bool CreateDirtyLineIfRequired(
            List<SpanAndLineRange> newSpanChanges, 
            List<SpanAndLineRange> nonIntersecting,
            bool textChanged,
            ITextSnapshot currentSnapshot)
        {
            var createdDirtyLine = false;
            if (RequiresDirtyLine() && textChanged && Intersected(newSpanChanges,nonIntersecting))
            {
                CreateDirtyLine(currentSnapshot);
                createdDirtyLine = true;
            }
            return createdDirtyLine;
        }

        private void CreateDirtyLine(ITextSnapshot currentSnapshot)
        {
            var firstTrackingSpan = trackingSpanRange.GetFirstTrackingSpan();
            dirtyLine = new DirtyLine(firstTrackingSpan, currentSnapshot);
        }

        private bool RequiresDirtyLine()
        {
            return dirtyLine == null && trackedCoverageLines.Lines.Any();
        }

        private bool Intersected(
            List<SpanAndLineRange> newSpanChanges,
            List<SpanAndLineRange> nonIntersecting)
        {
            return nonIntersecting.Count < newSpanChanges.Count;
        }

        public IContainingCodeTrackerProcessResult ProcessChanges(ITextSnapshot currentSnapshot, List<SpanAndLineRange> newSpanChanges)
        {
            var trackingSpanRangeProcessResult = ProcessTrackingSpanRangeChanges(currentSnapshot, newSpanChanges);
            var nonIntersectingSpans = trackingSpanRangeProcessResult.NonIntersectingSpans;
            if (trackingSpanRangeProcessResult.IsEmpty)
            {
                return new ContainingCodeTrackerProcessResult(true, nonIntersectingSpans,true);
            }

            var createdDirtyLine = CreateDirtyLineIfRequired(newSpanChanges, nonIntersectingSpans,trackingSpanRangeProcessResult.TextChanged,currentSnapshot);
            var result = new ContainingCodeTrackerProcessResult(createdDirtyLine, nonIntersectingSpans, false);
            if (!createdDirtyLine)
            {
                var linesChanged = UpdateLines(currentSnapshot);
                result.Changed = result.Changed || linesChanged;
            }
            
            return result;
        }

        private bool UpdateLines(ITextSnapshot currentSnapshot)
        {
            if (dirtyLine != null)
            {
               return dirtyLine.Update(currentSnapshot);
            }
            else
            {
                return trackedCoverageLines.Update(currentSnapshot);
            }
        }

        public IEnumerable<IDynamicLine> Lines => dirtyLine != null ? new List<IDynamicLine> { dirtyLine.Line } :  trackedCoverageLines.Lines;
    }
    
}
