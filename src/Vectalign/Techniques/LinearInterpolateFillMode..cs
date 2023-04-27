using framer.Common;

namespace framer.VectAlign.Techniques
{

    /**
 * Fill mode which interpolates linearly over consecutive sequences of injected nodes of the same type (when possible).
 */
    public class LinearInterpolateFillMode : BaseFillMode
    {

        public override void FillInjectedNodes(List<PathCommand> from, List<PathCommand> to)
        {
            //Execute a base injection first
            base.FillInjectedNodes(from, to);

            //Interpolates
            InterpolateResultList(from);
            InterpolateResultList(to);

            //Check validity again
            penPosFromAfter = PathNodeUtils.CalculatePenPosition(from);
            penPosToAfter = PathNodeUtils.CalculatePenPosition(to);
            CheckPenPosValidity();
        }

        /**
     * Apply interpolation on the result list (where possible)
     * @param list
     */
        private void InterpolateResultList(List<PathCommand> list)
        {
            if (list == null || list.Count <= 2)
                return;

            float[][] listPenPos = PathNodeUtils.CalculatePenPosition(list);

            //find subsequence of interpolatable commands
            var subList = new List<PathCommand>();

            var size = list.Count;
            var i = 0;
            while (i < size - 1)
            {
                //TODO O(n^2), improve this
                var currentNode = list[i];

                if (!IsInterpolatableCommand(currentNode.Type))
                {
                    i++;
                    continue;
                }

                var validSequence = true;
                var k = i;
                for (var j = i; j < size && validSequence; j++)
                {
                    if (currentNode.Type == list[j].Type)
                    {
                        k = j;
                        if (!currentNode.Params.Equals(list[j].Params))
                            break;
                    }
                    //TODO else if there's another compatible command (a sequence of L can interpolate with a V or an H
                    else
                        validSequence = false;
                }

                if (k - i > 2)
                {
                    InterpolateSubList(list.Skip(i).Take(k + 1).ToList());
                }

                i++;
            }
        }

        /**
     * Check if a command is interpolatable
     * @param command
     * @return
     */
        public bool IsInterpolatableCommand(CommandType command)
        {
            return command is CommandType.L or CommandType.V or CommandType.H;
        }


        /**
     * Interpolate a list of commands using two delimiter nodes
     * @param list
     */
        private void InterpolateSubList(List<PathCommand> list)
        {
            if (list == null || list.Count <= 2)
                return;

            var size = list.Count;
            PathCommand nodeFrom = list[0];
            PathCommand nodeTo = list[^1];

            var step = 1.0f / (size - 1);
            var fraction = 0.0f;
            foreach (var current in list){
                current.InterpolatePathDataNode(nodeFrom, nodeTo, fraction);
                fraction += step;
            }
        }
    }
}
