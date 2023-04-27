using framer.Common;
using framer.VectAlign.Techniques;

namespace framer.VectAlign
{
    public enum AlignMode
    {
        Base = 0,// Inject necessary elements by repeating existing ones
        Linear = 4,// Inject necessary elements and interpolates coordinates where possible
        SubBase = 1,// Use sub-Aligning and inject necessary elements by repeating existing ones
        SubLinear = 5//Use sub-Aligning and inject necessary elements and interpolates coordinates where possible

        //TODO more technique
    }
    public class VectAlign
    {

        public static readonly int MaxAlignIterations = 5;

        public static List<string> AlignSequence(List<string> paths, AlignMode alignMode)
        {
            var result = new List<string>();
            var old = paths[0];
            for (var i = 1; i < paths.Count; i++)
            {
                result.AddRange(Align(old, paths[i], alignMode));
                old = paths[i]; 
            }

            return result;
        }

        public static List<string> Align(string from, string to, AlignMode alignMode)
        {
            var fromList = PathParser.Parse(from);
            var toList = PathParser.Parse(to);

            if (PathParser.CanMorph(fromList, toList))
                return new List<string> { from, to };

            List<List<PathCommand>> aligns = alignMode switch
            {
                AlignMode.Base => new RawAlignMode().Align(fromList, toList),
                AlignMode.Linear => new RawAlignMode().Align(fromList, toList),
                AlignMode.SubBase => new SubAlignMode().Align(fromList, toList),
                AlignMode.SubLinear => new SubAlignMode().Align(fromList, toList),
                //TODO handle more case here if needed
                _ => throw new ArgumentOutOfRangeException(nameof(alignMode), alignMode, null)
            };

            if (aligns.IsNullOrEmpty())
                return new List<string>();

            IFillMode fillMode = alignMode switch
            {
                AlignMode.Base => new BaseFillMode(),
                AlignMode.SubBase => new BaseFillMode(),
                AlignMode.Linear => new LinearInterpolateFillMode(),
                AlignMode.SubLinear => new LinearInterpolateFillMode(),
                //TODO handle more case here if needed
                _ => throw new ArgumentOutOfRangeException(nameof(alignMode), alignMode, null)
            };

            fillMode.FillInjectedNodes(aligns[0], aligns[1]);

            PathNodeUtils.Simplify(aligns[0], aligns[1]);

            return new List<string> { PathNodeUtils.PathNodesToString(aligns[0]), PathNodeUtils.PathNodesToString(aligns[1]) };

        }

        static string PrintPathData(List<PathCommand> data)
        {
            return string.Join(Environment.NewLine, data.Select((node, i) => $"{i}.\t{node.ToString()}"));
        }
    }
}