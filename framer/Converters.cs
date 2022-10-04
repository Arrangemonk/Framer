using framer;
using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Framer
{
    internal class Program
    {
        const string idName = "id";
        const string pathName = "path";
        const string groupName = "g";
        private static XNamespace nameSpace = null;



        static void Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Console.WriteLine("svg animator");
            XDocument first = null;
            var framepaths = new Dictionary<string, List<string>>();
            //var transforms = new Dictionary<string, List<Transform>>();
            foreach (var path in args)
            {
                var svg = XDocument.Load(path);
                if (first == null)
                {
                    first = svg;
                    nameSpace = first.Root.GetDefaultNamespace();
                }
                //convertTransforms(ref transforms, svg.Root);
                extractPaths(ref framepaths, svg.Root);
                //HandleElements(svg.Root, groupName, (group, id) =>
                //{
                //    extractPaths(ref framepaths, group, id);
                //});

            }
            HandleElements(first.Root, pathName, (path, id) => animatePath(path, ref framepaths, id));

            //HandleElements(first.Root, groupName, (group, id) =>
            //{
            //    foreach (var path in animpaths(group, id))
            //    {
            //        animatePath(path, ref framepaths,id);
            //    }
            //});
            //animateTransforms(ref transforms, first.Root);
            first.Save(args[0].Substring(0, args[0].Length - 3) + "output.svg");
        }

        private static void convertTransforms(ref Dictionary<string, List<Transform>> transforms, XElement parent)
        {
            var descendants = parent.Descendants().Where(elem => TransformAttributeFromElement(elem) != null).ToArray();
            for (int i = 0; i < descendants.Length; i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.ToString();
                if (id == null)
                    continue;
                var attribute = TransformAttributeFromElement(descendants[i]);
                if (attribute == null)
                    continue;

                var decomposed = new Transform(attribute.Value.Replace("matrix(", "").Replace(")", "").Split(",").Select(part => double.Parse(part)).ToArray());
                //attribute.Value = decomposed.ToString();
                if (!transforms.ContainsKey(id))
                    transforms.Add(id, new List<Transform>());
                transforms[id].Add(decomposed);
            }
        }

        private static void animateTransforms(ref Dictionary<string, List<Transform>> transformDict, XElement parent)
        {
            var descendants = parent.Descendants().Where(elem => TransformAttributeFromElement(elem) != null).ToArray();
            for (int i = 0; i < descendants.Length; i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.ToString();
                if (id == null)
                    continue;
                var attribute = TransformAttributeFromElement(descendants[i]);
                if (attribute == null)
                    continue;

                if (!transformDict.ContainsKey(id))
                {
                    continue;
                }
                var transforms = transformDict[id];

                foreach (var transform in Transform.possibleTransforms(transforms))
                {
                    descendants[i].Add(Transform.animateTransform(transform.Type, attribute.Name.LocalName, "0s", 3, transform.Values, transform.Additive));
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
            for (int i = 0; i < descendants.Length; i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.ToString();
                if (id == null)
                    continue;
                action(descendants[i], id);
            }
        }

        private static void animatePath(XElement path, ref Dictionary<string, List<string>> framepaths, string index,bool loop = false)
        {
            var anims = framepaths[index];
            if (anims == null || anims.Count < 2 || anims.All(anim => anim.Equals(anims[0])))
                return;
            if(loop)
                anims.Add(anims[0]);// hin und her

            path.Add(FPath.AnimateAttribute("d", "0s", 3, anims.ToArray()));
        }


        private static void extractPaths(ref Dictionary<string, List<string>> framepaths, XElement parent)
        {
            var descendants = parent.Descendants(nameSpace + pathName).ToArray();
            for (int i = 0; i < descendants.Length; i++)
            {
                var id = descendants[i].Attribute(idName)?.Value;// ?? i.ToString();
                if (id == null)
                    continue;
                if (!framepaths.ContainsKey(id))
                    framepaths.Add(id, new List<string>());

                framepaths[id].Add(descendants[i].Attribute("d").Value);
            }
        }

        private static void extractPaths(ref Dictionary<string, List<string>> framepaths, XElement parent, string parentID)
        {
            var descendants = parent.Descendants(nameSpace + pathName).ToArray();

            for (int i = 0; i < descendants.Length; i++)
            {
                var id = descendants[i].Attribute(idName)?.Value ?? parentID + i;

                if (!framepaths.ContainsKey(id))
                    framepaths.Add(id, new List<string>());
                var d = descendants[i].Attribute("d").Value;
                if (!framepaths[id].Contains(d))
                    framepaths[id].Add(d);
            }
        }

        private static IEnumerable<XElement> animpaths(XElement svg, string parentID)
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

