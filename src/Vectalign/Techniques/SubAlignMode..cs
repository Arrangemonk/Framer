using framer.Common;


namespace framer.VectAlign.Techniques
{
    public class SubAlignMode : RawAlignMode
    {
        public override List<List<PathCommand>> Align(List<PathCommand> from, List<PathCommand> to)
        {
            //Remove 'Z'
            var TransformFrom = PathNodeUtils.Transform(from, 0, true);
            var TransformTo = PathNodeUtils.Transform(to, 0, true);

            var fromSize = TransformFrom.Count;
            var toSize = TransformTo.Count;
            var min = Math.Min(fromSize, toSize);
            var numberOfSubAligns = (int)Math.Max(1, min / 2);
            var fromChunkSize = fromSize / numberOfSubAligns;
            var toChunkSize = toSize / numberOfSubAligns;

            var readonlyFrom = new List<PathCommand>();
            var readonlyTo = new List<PathCommand>();
            int startFromIndex, startToIndex, endFromIndex, endToIndex;
            for (var i = 0; i < numberOfSubAligns; i++)
            {
                if (i == numberOfSubAligns - 1)
                {
                    endFromIndex = fromSize;
                    endToIndex = toSize;
                }
                else
                {
                    endFromIndex = Math.Min(fromSize, (i + 1) * (fromChunkSize));
                    endToIndex = Math.Min(toSize, (i + 1) * (toChunkSize));
                }

                startFromIndex = i * fromChunkSize;
                startToIndex = i * toChunkSize;
                var subAlign = base.Align(TransformFrom.GetRange(startFromIndex, endFromIndex - startFromIndex), TransformTo.GetRange(startToIndex, endToIndex - startFromIndex));
                readonlyFrom.AddRange(subAlign[0]);
                readonlyTo.AddRange(subAlign[1]);
            }

            return new List<List<PathCommand>> { readonlyFrom, readonlyTo };
        }

    }
}