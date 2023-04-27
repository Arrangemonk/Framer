
using System.Text;
using framer.Common;
using static framer.Common.CommandTypeExtensions;

namespace framer.VectAlign
{
    public class PathNodeUtils
    {
        public static bool IsEquivalent(List<PathCommand> original, List<PathCommand> alternative)
        {
            var innerStart = 0;
            foreach (var o in original)
            {
                var found = false;
                for (var i = innerStart; i < alternative.Count && !found; i++)
                {
                    var n = alternative[i];
                    if ((o.Type != n.Type || !o.Params.Equals(n.Params)) &&
                        ((o.Type != CommandType.Z && o.Type != CommandType.z) || n.Type != CommandType.L /*TransformZ*/
                        ))
                        continue;
                    found = true;
                    innerStart = i + 1;
                }

                if (!found)
                    return false;
            }

            return true;
        }

        public static List<PathCommand> Transform(List<PathCommand> elements)
        {
            return Transform(elements, 0, true);
        }

        public static List<PathCommand> Transform(List<PathCommand> elements, int extraCopy, bool TransformZ)
        {
            if (elements == null)
                return null;

            var Transformed = new List<PathCommand>();
            for (var ni = 0; ni < elements.Count; ni++)
            {
                var node = elements[ni];
                var cmdArgs = CommandArguments(node.Type);
                var argsProvided = node.Params.Count;

                if (node.Type == CommandType.z)
                    node.Type = CommandType.Z;

                if (cmdArgs == -1)
                {
#if DEBUG
                    Logger.Error("Command not supported! " + node.Type);
#endif
                }
                else if (argsProvided < cmdArgs)
                {
#if DEBUG
                    Logger.Error("Command " + node.Type + " requires " + cmdArgs + " params! Passing only " +
                                 argsProvided);
#endif
                }
                else if (cmdArgs == node.Params.Count)
                {
                    //Normal command with the exact number of params
                    Transformed.Add(node);
                    if (extraCopy > 0 && (TransformZ || node.Type != CommandType.Z))
                    {
                        //never Add extra z/Z or dumb commands
                        var extraNodes = new PathCommand(node);
                        if (node.Type.IsRelative())
                        {
                            // this is a relative movement. If we want to create extra nodes, we need to create neutral relative commands
                            node.Params = Enumerable.Range(0, node.Params.Count).Select(i => 0.0f).ToList();
                        }

                        for (var j = 0; j < extraCopy; j++)
                            Transformed.Add(extraNodes);
                    }
                }
                else
                {
                    //Multiple groups of params, verify consistency
                    var mod = (argsProvided % cmdArgs);
                    if (mod != 0)
                    {
#if DEBUG
                        Logger.Error("Providing multiple groups of params for command " + node.Type +
                                     ", but in wrong number (missing " + mod + " args)");
#endif
                    }
                    else
                    {
                        //Create multiple nodes
                        var iter = argsProvided / cmdArgs;
                        for (var i = 0; i < iter; i++)
                        {
                            var newNode = new PathCommand(node.Type,
                                node.Params.Skip(i * cmdArgs).Take((i + 1) * cmdArgs).ToList());
                            Transformed.Add(newNode);

                            if (extraCopy > 0)
                            {
                                var extraNodes = new PathCommand(newNode);
                                if (newNode.Type.IsRelative())
                                {
                                    // this is a relative movement. If we want to create extra nodes, we need to create neutral relative commands
                                    extraNodes.Params = Enumerable.Range(0, node.Params.Count).Select(i => 0.0f)
                                        .ToList();
                                }

                                for (var j = 0; j < extraCopy; j++)
                                    Transformed.Add(extraNodes);
                            }
                        }
                    }
                }
            }


            if (TransformZ)
            {
                var penPos = PathNodeUtils.CalculatePenPosition(Transformed);
                for (var t = 0; t < Transformed.Count; t++)
                {
                    var transformedNode = Transformed[t];
                    if (transformedNode.Type != CommandType.Z)
                        continue;
                    transformedNode.Type = CommandType.L;
                    transformedNode.Params = penPos[t].ToList();
                }
            }

            return Transformed;
        }

        public static void Simplify(List<PathCommand> from, List<PathCommand> to)
        {
            if (from.Count != to.Count)
            {
                return;
            }

            var removeIndexes = new bool[from.Count];
            var last = from.Count - 1; //avoid last

            for (var i = 0; i < last; i++)
            {
                if (from[i].Equals(from[i + 1]) && to[i].Equals(to[i + 1]))
                {
                    removeIndexes[i] = true;
                }
            }

            from = from.Where((elem, i) => !removeIndexes[i]).ToList();
            to = to.Where((elem, i) => !removeIndexes[i]).ToList();
        }

        public static float[][] CalculatePenPosition(List<PathCommand> sequence)
        {
            var penPos = new float[sequence.Count][];
            for (var i = 0; i < penPos.Length; i++)
            {
                penPos[i] = new[] { 0.0f, 0.0f };
            }

            var lastStart = new float[] { 0, 0 };
            var currentX = 0.0f;
            var currentY = 0.0f;
            var saveNewStart = false;

            for (var i = 0; i < sequence.Count; i++)
            {
                var node = sequence[i];
                if (node.Type == CommandType.z || node.Type == CommandType.Z)
                {
                    //Close path and restart from last start
                    currentX = lastStart[0];
                    currentY = lastStart[1];
                    saveNewStart = true;
                }
                else
                {
                    var positionFroParams = GetPositionForParams(node);
                    if (!positionFroParams.IsNullOrEmpty())
                    {
                        if (node.Type.IsRelative())
                        {
                            //relative movement (its already correct in case of v or h
                            currentX += positionFroParams[0];
                            currentY += positionFroParams[1];
                        }
                        else
                        {
                            //absolute movement
                            if (i > 0 && node.Type == CommandType.V)
                            {
                                currentX = penPos[i - 1][0];
                                currentY = positionFroParams[1];
                            }
                            else if (i > 0 && node.Type == CommandType.H)
                            {
                                currentX = positionFroParams[0];
                                currentY = penPos[i - 1][1];
                            }
                            else
                            {
                                currentX = positionFroParams[0];
                                currentY = positionFroParams[1];
                            }
                        }

                        if (node.Type == CommandType.m || node.Type == CommandType.M)
                        {
                            saveNewStart = true;
                        }

                        if (saveNewStart)
                        {
                            lastStart = new float[] { currentX, currentY };
                            saveNewStart = false;
                        }
                    }
                }

                penPos[i][0] = currentX;
                penPos[i][1] = currentY;
            }

            return penPos;
        }

        static float[] GetPositionForParams(PathCommand node)
        {
            if (node.Params == null || node.Params.Count == 0)
                return Array.Empty<float>();

            var ris = new float[2];

            if (node.Type.ToRelative() == CommandType.v)
            {
                ris[1] = node.Params[0];
            }
            else if (node.Type.ToRelative() == CommandType.h)
            {
                ris[0] = node.Params[0];
            }
            else
            {
                ris[0] = node.Params[^2];
                ris[1] = node.Params[^1];
            }

            return ris;
        }

        public static string PathNodesToString(List<PathCommand> nodes, bool onlyCommands)
            => string.Join("", nodes.Select(n => $"{(onlyCommands ? n.Type : n)} "));



        public static string PathNodesToString(List<PathCommand> nodes)
            => PathNodesToString(nodes, false);




        /// <summary>
        /// Return the max X, Y values
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        public static float[] GetMaxValues(List<PathCommand> nodes)
        {
            if (nodes == null)
                return null;

            var penPos = CalculatePenPosition(nodes);

            var ris = new float[] { 0, 0 };
            foreach (var pen in penPos)
            {
                if (pen[0] > ris[0])
                    ris[0] = pen[0];

                if (pen[1] > ris[1])
                    ris[1] = pen[1];
            }

            return ris;
        }

    }
}