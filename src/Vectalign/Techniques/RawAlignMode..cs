using framer.Common;
using framer.VectAlign;
using framer.VectAlign;
using framer.VectAlign.Techniques;

namespace framer.VectAlign.Techniques
{
    public class RawAlignMode : IAlignMode
    {
        public virtual List<List<PathCommand>> Align(List<PathCommand> from, List<PathCommand> to)
        {
            var equivalent = false;
            var extraCloneNodes = 0;
            List<PathCommand> alignedFrom = null;
            List<PathCommand> alignedTo = null;

            for (var i = 0; i < VectAlign.MaxAlignIterations && !equivalent; i++)
            {
                var nw = new NeedlemanWunschAlignment(PathNodeUtils.Transform(from, extraCloneNodes, true),
                    PathNodeUtils.Transform(to, extraCloneNodes, true));
                alignedFrom = nw.AlignedFrom;
                alignedTo = nw.AlignedTo;
                equivalent = PathNodeUtils.IsEquivalent(nw.OriginalFrom, nw.AlignedFrom) &&
                             PathNodeUtils.IsEquivalent(nw.OriginalTo, nw.AlignedTo);

                extraCloneNodes++;
            }

            return equivalent ? new List<List<PathCommand>> { alignedFrom, alignedTo } : null;
        }
    }
}
