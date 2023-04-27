#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace framer.Common
{
    public enum CommandType
    {
        D, _, //dumb
        M, m,
        L, l, H, h, V, v,
        C, c, S, s, Q, q,
        T, t,
        A, a,
        Z, z
    }

    public static class CommandTypeExtensions
    {
        private static readonly List<CommandType> Relative = new()
        {
            CommandType.m,
            CommandType.l,
            CommandType.h,
            CommandType.v,
            CommandType.c,
            CommandType.s,
            CommandType.q,
            CommandType.a,
            CommandType.z
        };

        private static readonly List<CommandType> Absolute = new()
        {
            CommandType.M,
            CommandType.L,
            CommandType.H,
            CommandType.V,
            CommandType.C,
            CommandType.S,
            CommandType.Q,
            CommandType.A,
            CommandType.Z
        };

        private static readonly Dictionary<CommandType, CommandType> CommandMapping = new()
        {
            { CommandType._, CommandType._ },
            { CommandType.D, CommandType.D },
            { CommandType.M, CommandType.m },
            { CommandType.L, CommandType.l },
            { CommandType.H, CommandType.h },
            { CommandType.V, CommandType.v },
            { CommandType.C, CommandType.c },
            { CommandType.S, CommandType.s },
            { CommandType.Q, CommandType.q },
            { CommandType.A, CommandType.a },
            { CommandType.Z, CommandType.z }
        };

        public static CommandType ToAbsolute(this CommandType input) =>
            CommandMapping.First(m => m.Value == input || m.Key == input).Key;

        public static CommandType ToRelative(this CommandType input) =>
            CommandMapping.First(m => m.Value == input || m.Key == input).Value;

        public static int CommandArguments(CommandType type)
        {
            return type switch
            {
                CommandType._ => 0,
                CommandType.D => 0,
                CommandType.z => 0,
                CommandType.Z => 0,
                CommandType.m => 2,
                CommandType.M => 2,
                CommandType.l => 2,
                CommandType.L => 2,
                CommandType.t => 2,
                CommandType.T => 2,
                CommandType.h => 1,
                CommandType.H => 1,
                CommandType.v => 1,
                CommandType.V => 1,
                CommandType.c => 6,
                CommandType.C => 6,
                CommandType.s => 4,
                CommandType.S => 4,
                CommandType.q => 4,
                CommandType.Q => 4,
                CommandType.a => 7,
                CommandType.A => 7,
                _ => -1
            };
        }

        public static bool IsRelative(this CommandType type) => Relative.Contains(type);
        public static bool IsAbsolute(this CommandType type) => Absolute.Contains(type);
    }

    [DebuggerDisplay("{nameof(PathCommand)}: {DebuggerDisplay}")]
    public struct PathCommand
    {
        public PathCommand(PathCommand old, params float[] numbers)
        {
            Type = old.Type;
            Params = numbers.Any() ? numbers.ToList() : new List<float>(old.Params);
        }

        public PathCommand(CommandType type, params float[] numbers)
        {
            Type = type;
            Params = numbers.ToList();
        }

        public PathCommand(CommandType type, List<float> numbers)
        {
            Type = type;
            Params = numbers;
        }

        public CommandType Type { get; set; }
        public List<float> Params { get; set; }

        public PathCommand InterpolatePathDataNode(PathCommand nodeFrom, PathCommand nodeTo, float fraction)
        {
            return new PathCommand(nodeFrom.Type,
                nodeFrom.Params.Select((p, i) => p * (1 - fraction) + nodeTo.Params[i] * fraction).ToList());

        }

        public string DebuggerDisplay => ToString();

        public override string ToString()
        {
            switch (Type)
            {
                case CommandType.M:
                case CommandType.m:
                    return $"{Type}{Params[0]},{Params[1]}";
                case CommandType.L:
                case CommandType.l:
                    return $"{Type}{Params[0]},{Params[1]}";
                case CommandType.H:
                case CommandType.h:
                    return $"{Type}{Params[0]}";
                case CommandType.V:
                case CommandType.v:
                    return $"{Type}{Params[0]}";
                case CommandType.C:
                case CommandType.c:
                    return $"{Type}{Params[0]},{Params[1]} {Params[2]},{Params[3]} {Params[4]},{Params[5]}";
                case CommandType.S:
                case CommandType.s:
                    return $"{Type}{Params[0]},{Params[1]} {Params[2]},{Params[3]}";
                case CommandType.Q:
                case CommandType.q:
                    return $"{Type}{Params[0]},{Params[1]} {Params[2]},{Params[3]}";
                case CommandType.T:
                case CommandType.t:
                    return $"{Type}{Params[0]},{Params[1]}";
                case CommandType.A:
                case CommandType.a:
                    return $"{Type}{Params[0]},{Params[1]} {Params[2]} {Params[3]},{Params[4]} {Params[5]},{Params[6]}";
                case CommandType.Z:
                case CommandType.z:
                    return $"{Type}";
                case CommandType.D:
                    return "D";
                case CommandType._:
                    return "_";
                default:
                    throw new InvalidOperationException("Unknown path command type.");
            }
        }
    }

    public static class PathParser
    {
        private static readonly Regex Commandregex =
            new("([MmLlHhVvCcSsQqTtAaZzD_])((-?\\d+(\\.\\d+)?,-?\\d+(\\.\\d+)?)\\s?)*");

        private static readonly Regex Numberregex = new("-?\\d+(\\.\\d+)?");

        public static List<PathCommand> Parse(string source)
        {
            var result = new List<PathCommand>();
            foreach (Match match in Commandregex.Matches(source))
            {
                var type = Enum.Parse<CommandType>(match.Value[..1]);
                var numbers = Numberregex.Matches(match.Value[1..]).Select(m => float.Parse(m.Value)).ToList();
                result.Add(new PathCommand(type, numbers));
            }

            return result;
        }

        public static bool CanMorph(List<PathCommand> path1, List<PathCommand> path2)
        {
            return path1 != null && path2 != null
                                 && path1.Count == path2.Count
                                 && !path1.Any((t, i) =>
                                     t.Type != path2[i].Type || t.Params.Count != path2[i].Params.Count);
        }

        public static bool IsRelative(List<PathCommand> path)
        {
            return path.Skip(1).All(c => c.Type.IsRelative());
        }

        public static bool IsAbsolute(List<PathCommand> path)
        {
            return path.Skip(1).All(c => c.Type.IsAbsolute());
        }

        /// <summary>
        /// Converts Path Commands to relative, ported from SnapSvg http://snapsvg.io/
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<PathCommand> PathToRelative(List<PathCommand> path)
        {

            if (IsRelative(path))
            {
                return new List<PathCommand>(path);
            }

            var res = new List<PathCommand>();
            float x = 0.0f, y = 0.0f, mx = 0.0f, my = 0.0f;
            var start = 0;

            if (path[0].Type == CommandType.M)
            {
                x = path[0].Params[0];
                y = path[0].Params[1];
                mx = x;
                my = y;
                start++;
                res.Add(new PathCommand(CommandType.M, x, y));
            }

            for (var i = start; i < path.Count; i++)
            {
                var r = new PathCommand(CommandType._);
                var pa = path[i];

                if (pa.Type.ToRelative() != pa.Type)
                {
                    r.Type = pa.Type.ToRelative();

                    switch (r.Type)
                    {
                        case CommandType.a:
                            r.Params.Add(pa.Params[0]);
                            r.Params.Add(pa.Params[1]);
                            r.Params.Add(pa.Params[2]);
                            r.Params.Add(pa.Params[3]);
                            r.Params.Add(pa.Params[4]);
                            r.Params.Add(pa.Params[5] - x);
                            r.Params.Add(pa.Params[6] - y);
                            break;
                        case CommandType.v:
                            r.Params.Add(pa.Params[0] - y);
                            break;
                        case CommandType.m:
                            mx = pa.Params[0];
                            my = pa.Params[1];
                            break;
                        default:
                            for (var j = 0; j < pa.Params.Count; j++)
                            {
                                r.Params.Add(pa.Params[j] - (j % 2 != 0 ? x : y));
                            }

                            break;
                    }
                }
                else
                {
                    r = new PathCommand(pa.Type);

                    if (pa.Type == CommandType.m)
                    {
                        mx = pa.Params[0] + x;
                        my = pa.Params[1] + y;
                    }

                    r.Params = new List<float>(pa.Params);

                }

                switch (r.Type)
                {
                    case CommandType.z:
                        x = mx;
                        y = my;
                        break;
                    case CommandType.h:
                        x += r.Params[^1];
                        break;
                    case CommandType.v:
                        y += r.Params[^1];
                        break;
                    default:
                        x += r.Params[^2];
                        y += r.Params[^1];
                        break;
                }

                res.Add(r);
            }

            return res;
        }

        /// <summary>
        /// Converts Path Commands to absolute, ported from SnapSvg http://snapsvg.io/
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<PathCommand> PathToAbsolute(List<PathCommand> path)
        {
            var res = new List<PathCommand>();
            float x = 0, y = 0, mx = 0, my = 0;
            for (int i = 0, ii = path.Count; i < ii; i++)
            {
                var pp = path[i].Params;
                var r = new PathCommand(CommandType._);
                if (!path[i].Type.IsAbsolute())
                {
                    r.Type = path[i].Type.ToAbsolute();
                    switch (r.Type)
                    {
                        case CommandType.A:
                            r.Params.Add(pp[0]);
                            r.Params.Add(pp[1]);
                            r.Params.Add(pp[2]);
                            r.Params.Add(pp[3]);
                            r.Params.Add(pp[4]);
                            r.Params.Add(pp[5] + x);
                            r.Params.Add(pp[6] + y);
                            break;
                        case CommandType.V:
                            r.Params.Add(pp[0] + y);
                            break;
                        case CommandType.H:
                            r.Params.Add(pp[0] + x);
                            break;
                        case CommandType.M:
                            mx = pp[0] + x;
                            my = pp[1] + y;
                            break;
                        default:
                            for (int j = 0, jj = pp.Count; j < jj; j++)
                            {
                                r.Params.Add(pp[j] + (j % 2 == 0 ? x : y));
                            }
                            break;
                    }
                }else{
                    r.Params = new List<float>(pp);
                }
                switch (r.Type)
                {
                    case CommandType.Z:
                        x = +mx;
                        y = +my;
                        break;
                    case CommandType.H:
                        x = r.Params[0];
                        break;
                    case CommandType.V:
                        y = r.Params[0];
                        break;
                    case CommandType.M:
                        mx = r.Params[^2];
                        my = r.Params[^1];
                        x = r.Params[^2];
                        y = r.Params[^1];
                        break;
                    default:
                        x = r.Params[^2];
                        y = r.Params[^1];
                        break;
                }
                res.Add(r);
            }
            return res;
        }
    }
}
