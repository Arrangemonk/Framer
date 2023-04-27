using System.Text;
using framer.Common;

namespace framer.VectAlign.Techniques
{
    /**
 * Base fill mode which will fill the eventual holes using neutral nodes.
 */
    public class BaseFillMode : IFillMode
    {

        protected float[][] penPosFrom;
        protected float[][] penPosTo;
        protected float[][] penPosFromAfter;
        protected float[][] penPosToAfter;


        public virtual void FillInjectedNodes(List<PathCommand> from, List<PathCommand> to)
        {

            penPosFrom = PathNodeUtils.CalculatePenPosition(from);
            penPosTo = PathNodeUtils.CalculatePenPosition(to);

            for (var i = 0; i < from.Count; i++)
            {
                PathCommand nodePlaceholder, nodeMaster;
                float[][] penPosPlaceholder = null;
                float[][] penPosMaster = null;
                List<PathCommand> listMaster = null;
                List<PathCommand> listPlaceholder = null;

                if (from[i].Type == CommandType._)
                {
                    nodePlaceholder = from[i];
                    nodeMaster = to[i];
                    penPosPlaceholder = penPosFrom;
                    penPosMaster = penPosTo;
                    listMaster = to;
                    listPlaceholder = from;
                }
                else if (to[i].Type == CommandType._)
                {
                    nodePlaceholder = to[i];
                    nodeMaster = from[i];
                    penPosPlaceholder = penPosTo;
                    penPosMaster = penPosFrom;
                    listMaster = from;
                    listPlaceholder = to;
                }
                else
                    continue;

                nodePlaceholder.Type = nodeMaster.Type;
                nodePlaceholder.Params = new List<float>(nodeMaster.Params);
                float lastPlaceholderX, lastPlaceholderY, lastMasterX, lastMasterY;
                if (i > 0)
                {
                    lastPlaceholderX = penPosPlaceholder[i - 1][0];
                    lastPlaceholderY = penPosPlaceholder[i - 1][1];
                    lastMasterX = penPosMaster[i - 1][0];
                    lastMasterY = penPosMaster[i - 1][1];
                }
                else
                {
                    lastPlaceholderX = 0;
                    lastPlaceholderY = 0;
                    lastMasterX = 0;
                    lastMasterY = 0;
                }

                if (nodeMaster.Type.ToRelative() == CommandType.z)
                {
                    //Injecting a 'z' means we need to counterbalance the last path position with an extra 'm'
                    var extraMoveNodePlaceholder = new PathCommand(CommandType.M, lastPlaceholderX, lastPlaceholderY);
                    var extraMoveNodeMaster = new PathCommand(CommandType.M, lastMasterX, lastMasterY);

                    listMaster.Insert(i, extraMoveNodeMaster);
                    listPlaceholder.Insert(i, extraMoveNodePlaceholder);
                    i++; //next item is already filled, we just Added it

                    //Recalculate penpos arrays //TODO do this more efficiently
                    penPosFrom = PathNodeUtils.CalculatePenPosition(from);
                    penPosTo = PathNodeUtils.CalculatePenPosition(to);

                }
                else if (nodePlaceholder.Type.IsRelative())
                {
                    // this is a relative movement. If we want to create extra nodes, we need to create neutral relative commands
                    nodePlaceholder.Params = Enumerable.Repeat(0.0f, nodePlaceholder.Params.Count).ToList();
                }
                else
                {
                    if (nodePlaceholder.Type == CommandType.V)
                    {
                        nodePlaceholder.Params[0] = lastPlaceholderY;
                    }
                    else if (nodePlaceholder.Type == CommandType.H)
                    {
                        nodePlaceholder.Params[0] = lastPlaceholderX;
                    }
                    else
                    {
                        for (var j = 0; j < nodePlaceholder.Params.Count; j++)
                        {
                            nodePlaceholder.Params[j++] = lastPlaceholderX;
                            nodePlaceholder.Params[j] = lastPlaceholderY;
                        }
                    }
                }

                listPlaceholder[i] = nodePlaceholder;
            }

            penPosFromAfter = PathNodeUtils.CalculatePenPosition(from);
            penPosToAfter = PathNodeUtils.CalculatePenPosition(to);
            CheckPenPosValidity();
        }


        protected bool CheckPenPosValidity()
        {
            return (Math.Abs(penPosFromAfter[^1][0] - penPosFrom[^1][0]) < MathUtil.TOLERANCE)
                   && (Math.Abs(penPosFromAfter[^1][1] - penPosFrom[^1][1]) < MathUtil.TOLERANCE)
                   && (Math.Abs(penPosToAfter[^1][0] - penPosTo[^1][0]) < MathUtil.TOLERANCE)
                   && (Math.Abs(penPosToAfter[^1][1] - penPosTo[^1][1]) < MathUtil.TOLERANCE);
        }
    }
}