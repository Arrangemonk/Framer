using framer;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using framer.Common;
using framer.VectAlign;

namespace Framer
{
    internal class Program
    {
        const string idName = "id";
        const string pathName = "path";
        const string groupName = "g";
        private static XNamespace nameSpace = null;

        public static string PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Console.WriteLine("svg animator");
            XDocument first = null;
            var framepaths = new Dictionary<string, List<string>>();
            var Transforms = new Dictionary<string, List<Transform>>();
            var ViewBoxes = new List<List<float>>();
            foreach (var path in args.OrderBy(path => PadNumbers(Path.GetFileNameWithoutExtension(path))))
            {
                var svg = XDocument.Load(path);
                if (first == null)
                {
                    first = svg;
                    nameSpace = first.Root.GetDefaultNamespace();
                }
                ConvertTransforms(ref Transforms, svg.Root);
                ExtractPaths(ref framepaths, svg.Root);
                ExtractViewBoxe(ref ViewBoxes, svg.Root);
                //HandleElements(svg.Root, groupName, (group, id) =>
                //{
                //    ExtractPaths(ref framepaths, group, id);
                //});

            }

            HandleViewBoxes(first.Root, ViewBoxes);

            HandleElements(first.Root, pathName, (path, id) => AnimatePath(path, ref framepaths, id,true));

            //HandleElements(first.Root, groupName, (group, id) =>
            //{
            //    foreach (var path in AnimPaths(group, id))
            //    {
            //        AnimatePath(path, ref framepaths,id);
            //    }
            //});
            AnimateTransforms(ref Transforms, first.Root);
            first.Save(args[0].Substring(0, args[0].Count() - 3) + "output.svg");
        }

        private static void ExtractViewBoxe(ref List<List<float>> ViewBoxes, XElement svgRoot)
        {
            var viewbox = svgRoot.Attribute("viewBox")?.Value;
            if (viewbox != null)
            {
                var parts = viewbox.Split(' ');

                ViewBoxes.Add(parts.Select(part => float.Parse(part)).ToList());
            }
        }

        private static void HandleViewBoxes(XElement firstRoot, List<List<float>> ViewBoxes)
        {
            var result = new List<float>{0,0,0,0};

            foreach (var current in ViewBoxes)
            {
                for (var i = 0; i < current.Count; i++)
                {
                    // start points of viewboxes can be smaller than 0
                    if(current[i] < 0 ? current[i] < result[i] : result[i] < current[i])
                        result[i] = current[i];
                }
            }

            var resultstring = string.Join(" ", result);
            var viewbox = firstRoot.Attribute("viewBox");
            if (viewbox == null)
                firstRoot.Add(new XAttribute("viewBox", resultstring));
            else
                viewbox.Value = resultstring;
        }

        private static void ConvertTransforms(ref Dictionary<string, List<Transform>> Transforms, XElement parent)
        {
            var descendants = parent.Descendants().Where(elem => TransformAttributeFromElement(elem) != null).ToArray();
            for (int i = 0; i < descendants.Count(); i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.[i]();
                if (id == null)
                    continue;
                var attribute = TransformAttributeFromElement(descendants[i]);
                if (attribute == null)
                    continue;

                var decomposed = new Transform(attribute.Value.Replace("matrix(", "").Replace(")", "").Split(",").Select(part => double.Parse(part)).ToArray());
                //attribute.Value = decomposed.[i]();
                if (!Transforms.ContainsKey(id))
                    Transforms.Add(id, new List<Transform>());
                Transforms[id].Add(decomposed);
            }
        }

        private static void AnimateTransforms(ref Dictionary<string, List<Transform>> TransformDict, XElement parent)
        {
            var descendants = parent.Descendants().Where(elem => TransformAttributeFromElement(elem) != null).ToArray();
            for (int i = 0; i < descendants.Count(); i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.[i]();
                if (id == null)
                    continue;
                var attribute = TransformAttributeFromElement(descendants[i]);
                if (attribute == null)
                    continue;

                if (!TransformDict.ContainsKey(id))
                {
                    continue;
                }
                var Transforms = TransformDict[id];

                foreach (var t in Transform.PossibleTransforms(Transforms))
                {
                    descendants[i].Add(Transform.AnimateTransform(t.Type, attribute.Name.LocalName, "0s", 1, t.Values, t.Additive));
                }
                //attribute.Remove();
            }
        }

        private static XAttribute? TransformAttributeFromElement(XElement elem)
        {
            return elem.Attribute("gradientTransform") ??
                   elem.Attribute("transform");
        }

        private static void HandleElements(XElement parent, string elementName, Action<XElement, string> action)
        {

            var descendants = parent.Descendants(nameSpace + elementName).ToArray();
            for (int i = 0; i < descendants.Count(); i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.[i]();
                if (id == null)
                    continue;
                action(descendants[i], id);
            }
        }

        private static void AnimatePath(XElement path, ref Dictionary<string, List<string>> framepaths, string index, bool loop = false)
        {
            var anims = framepaths[index];
            if (anims == null || anims.Count < 2 || anims.All(anim => anim.Equals(anims[0])))
                return;
            if (loop)
                anims.Add(anims[0]);// hin und her

            var aligned = VectAlign.AlignSequence(anims.ToList(),AlignMode.SubLinear);
            path.Attribute("d").Value = aligned[0];

            path.Add(FPath.AnimatePath( path.Attribute(idName).Value, 1, aligned));
        }


        private static void ExtractPaths(ref Dictionary<string, List<string>> framepaths, XElement parent)
        {
            var descendants = parent.Descendants(nameSpace + pathName).ToArray();
            for (int i = 0; i < descendants.Count(); i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.[i]();
                if (id == null)
                    continue;
                if (!framepaths.ContainsKey(id))
                    framepaths.Add(id, new List<string>());

                framepaths[id].Add(descendants[i].Attribute("d").Value);
            }
        }

        private static void ExtractPaths(ref Dictionary<string, List<string>> framepaths, XElement parent, string parentID)
        {
            var descendants = parent.Descendants(nameSpace + pathName).ToArray();

            for (int i = 0; i < descendants.Count(); i++)
            {
                var id = descendants[i].Attribute(idName)?.Value ?? parentID + i;

                if (!framepaths.ContainsKey(id))
                    framepaths.Add(id, new List<string>());
                var d = descendants[i].Attribute("d").Value;
                if (!framepaths[id].Contains(d))
                    framepaths[id].Add(d);
            }
        }

        private static IEnumerable<XElement> AnimPaths(XElement svg, string parentID)
        {
            var result = new List<XElement>();
            int index = 0;
            foreach (var p in svg.Elements().Where(x => x.Name.LocalName == pathName))
            {
                var id = p.Attributes().FirstOrDefault(a => a.Name.LocalName == idName)?.Value;
                if (id == null)
                {
                    id = parentID + index;
                    p.Add(new XAttribute(idName, id));
                }
                result.Add(p);
                index++;
            }
            return result;
        }
    }
}

