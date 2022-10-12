using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace framer
{
    public class Converters
    {



        public static string ConvertRectangles(string x, string y, string width, string height)
        {
            var x1 = float.Parse(x);
            var y1 = float.Parse(y);
            var w = float.Parse(width);
            var h = float.Parse(height);

            if (x1 < 0 || y1 < 0 || w < 0 || h < 0)
            {
                return "";
            }
            var x2 = (x1 + w).ToString();
            var y2 = (y1 + h).ToString();

            return $"M{x},{y}L{x2},{y} {x2},{y2} {x},{y2}z";
        }

        public static string convertLine(string x1, string y1, string x2, string y2)
        {
            if (float.Parse(x1) < 0 || float.Parse(y1) < 0 || float.Parse(x2) < 0 || float.Parse(y2) < 0)
            {
                return "";
            }

            return $"M{x1},{y1}L{x2},{y2}";
        }

        //public static string convertPoly(string points,string types)
        //{
        //    types = types ?? "polyline";

        //    var pointsArr = points
        //        /** clear redundant characters */
        //        .Split("     ").Join("")
        //        .trim()
        //        .split("/\s +|,/");
        //    var x0 = pointsArr.shift();
        //    var y0 = pointsArr.shift();

        //    var output = 'M' + x0 + ',' + y0 + 'L' + pointsArr.join(' ');

        //    return types === 'polygon' ? output + 'z' : output;
        //}

        //function convertCE(cx, cy)
        //{
        //    function calcOuput(cx, cy, rx, ry)
        //    {
        //        if (cx < 0 || cy < 0 || rx <= 0 || ry <= 0)
        //        {
        //            return '';
        //        }

        //        var output = 'M' + (cx - rx).toString() + ',' + cy.toString();
        //        output += 'a' + rx.toString() + ',' + ry.toString() + ' 0 1,0 ' + (2 * rx).toString() + ',0';
        //        output += 'a' + rx.toString() + ',' + ry.toString() + ' 0 1,0' + (-2 * rx).toString() + ',0';

        //        return output;
        //    }

        //    switch (arguments.length)
        //    {
        //        case 3:
        //            return calcOuput(parseFloat(cx, 10), parseFloat(cy, 10), parseFloat(arguments[2], 10), parseFloat(arguments[2], 10));
        //        case 4:
        //            return calcOuput(parseFloat(cx, 10), parseFloat(cy, 10), parseFloat(arguments[2], 10), parseFloat(arguments[3], 10));
        //            break;
        //        default:
        //            return '';
        //    }
        //}

    }

    public static class Interpolation
    {
        public const string Linear = "0 0 1 1";
        public const string EaseIn = "0.5 0 1 1";
        public const string EaseOut = "0 0 0.5 1";
        public const string EaseInEaseOut = "0.5 0 0.5 1";
    }

    public class FPath
    {

        public static XElement AnimateAttribute(string attributeName, string begin, double frameduration, string[] values) => XElement.Parse($@"
<svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:space=""preserve"" >
<animate 
    attributeName=""{attributeName}"" 
    begin=""{begin}"" 
    dur=""{frameduration * values.Length}s"" 
    values=""{string.Join(";", values)}"" 
    repeatCount=""indefinite"" 
    calcMode=""spline""
    keySplines=""{string.Join(";", Enumerable.Repeat(Interpolation.Linear, values.Length - 1))}""
    fill=""freeze""/>
</svg>").Descendants().First(x => x.Name.LocalName == "animate");

    }
    public class TransformAnim
    {

        public TransformAnim(string type, string[] values, bool additive)
        {
            Type = type;
            Values = values;
            Additive = additive;

         }

        public bool Additive { get; set; }
        public string Type { get; set; }
        public string[] Values { get; set; }
    }
    public class Transform
    {
        public static XElement animateTransform(string type, string attributename, string begin, double frameduration, string[] values, bool additive) => XElement.Parse($@"
<svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:space=""preserve"" >
    <animateTransform
      attributeName =""{attributename}""
      attributeType=""XML""
      begin=""{begin}""
      dur=""{frameduration * values.Length}s"" 
      repeatCount=""indefinite""
      {(additive ? "additive=\"sum\"" : "")}
      type=""{type}""
      values=""{string.Join(";", values)}"" />
      calcMode=""spline""
      keySplines=""{string.Join(";", Enumerable.Repeat(Interpolation.Linear, values.Length - 1))}""
</svg>").Descendants().First(x => x.Name.LocalName == "animateTransform");

        const string format = "0.#################";

        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double SkewX { get; set; }
        public double SkewY { get; set; }

        public const string TranslateName = "translate";
        public string Translate => $"{X.ToString(format)},{Y.ToString(format)}";

        public const string RotateName = "rotate";
        public string Rotate => $"{Angle.ToString(format)}";

        public const string SkewXName = "skewX";
        public string SkewXAnim => $"{SkewX.ToString(format)}";

        public const string SkewYName = "skewY";
        public string SkewYAnim => $"{SkewY.ToString(format)}";

        public const string ScaleName = "scale";
        public string Scale => $"{ScaleX.ToString(format)},{ ScaleY.ToString(format)}";

        //https://stackoverflow.com/questions/5107134/find-the-rotation-and-skew-of-a-matrix-transformation
        // var a = [a, b, c, d, e, f]
        public Transform(params double[] a)
        {
            var angle = normalizeAngle(Math.Atan2(a[1], a[0]));
            var denom = Math.Pow(a[0], 2) + Math.Pow(a[1], 2);
            var scaleX = Math.Sqrt(denom);
            var scaleY = (a[0] * a[3] - a[2] * a[1]) / scaleX;
            var skewX = normalizeAngle(Math.Atan2(a[0] * a[2] + a[1] * a[3], denom));

            Angle = clampAngle(angle / (Math.PI / 180));    // this is rotation angle in degrees
            ScaleX = scaleX;                                    // scaleX factor  
            ScaleY = scaleY;                                    // scaleY factor
            SkewX = clampAngle(skewX / (Math.PI / 180));    // skewX angle degrees
            SkewY = 0;                                          // skewY angle degrees
            X = a[4];                                     // translation point  x
            Y = a[5];                                        // translation point  y
        }

        private double normalizeAngle(double angle)
        {
            const double tau = 2.0 * Math.PI;
            return (angle % tau);
        }

        private double clampAngle(double angle)
        {
            angle += 360;
            return angle % 360;
        }

        public override string ToString()
        {
            List<string> result = new List<string>();
            if (X != 0 && Y != 0)
                result.Add($"{TranslateName}({X.ToString(format)},{Y.ToString(format)})");
            if (Angle != 0)
                result.Add($"{RotateName}({Angle.ToString(format)})");
            if (ScaleX != 1 && ScaleY != 1)
                result.Add($"{ScaleName}({ScaleX.ToString(format)},{ ScaleY.ToString(format)})");
            if (SkewX != 0)
                result.Add($"{SkewXName}({SkewX.ToString(format)})");
            if (SkewY != 0)
                result.Add($"{SkewYName}({SkewY.ToString(format)})");

            return string.Join(" ", result);
        }

        public static IEnumerable<TransformAnim> possibleTransforms(IEnumerable<Transform> transforms)
        {
            var translatelist = new List<string>();
            var rotatelist = new List<string>();
            var scalelist = new List<string>();
            var skewXlist = new List<string>();
            var skewYlist = new List<string>();

            foreach (var transform in transforms)
            {
                translatelist.Add(transform.Translate);
                rotatelist.Add(transform.Rotate);
                scalelist.Add(transform.Scale);
                skewXlist.Add(transform.SkewXAnim);
                skewYlist.Add(transform.SkewYAnim);
            }
            if (translatelist.Any(translate => !translatelist.First().Equals(translate)))
                yield return new TransformAnim(TranslateName, translatelist.ToArray(), false);
            if (rotatelist.Any(rotate => !rotatelist.First().Equals(rotate)))
                yield return new TransformAnim(RotateName, rotatelist.ToArray(), true);
            if (scalelist.Any(scale => !scalelist.First().Equals(scale)))
                yield return new TransformAnim(ScaleName, scalelist.ToArray(), true);
            if (skewXlist.Any(skewX => !skewXlist.First().Equals(skewX)))
                yield return new TransformAnim(SkewXName, skewXlist.ToArray(), true);
            if (skewYlist.Any(skewY => !skewYlist.First().Equals(skewY)))
                yield return new TransformAnim(SkewYName, skewYlist.ToArray(), true);

            yield break;
        }
    }
}
