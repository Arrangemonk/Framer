using System.Text;
using System.Text.RegularExpressions;

namespace framer.Test
{
    public class UnitTest1
    {

        private static Regex commandregex = new Regex("([MmLlHhVvCcSsQqTtAaZz])((-?\\d+(\\.\\d+)?,-?\\d+(\\.\\d+)?)\\s?)*");
        private static string source = "M1375.85,1406.31C1367.72,1402.12 1380.15,1329.51 1380.15,1322.27C1380.15,1311.08 1389.23,1302 1400.42,1302C1411.61,1302 1420.7,1311.08 1420.7,1322.27C1420.7,1339.56 1411.7,1376.49 1402.66,1390.84C1411.09,1390.84 1451.07,1434.58 1451.07,1445.08C1451.07,1468.68 1451.07,1479.03 1444.86,1494.01C1441.99,1486.57 1439.34,1477.97 1440.15,1468.68C1438.19,1476.64 1436.82,1484.36 1432.11,1491.86C1429.06,1485.05 1428.53,1477.33 1429.17,1468.68C1426.35,1475.94 1421.63,1482.71 1416.51,1489.04C1416.51,1463.26 1421.37,1452.31 1424.94,1441.29C1416.51,1441.29 1402.29,1419.95 1375.85,1406.31Z";
        private static string target = "M2143.67,1551.05C2169,1527.55 2202.55,1542.78 2204.87,1576.94C2208.07,1624.15 2171.83,1642.7 2150.98,1656.63C2181.91,1659.13 2167.79,1697.29 2171.03,1712.04C2173,1721.02 2186.81,1737.98 2179.58,1743.23C2170.84,1749.58 2149.36,1749.69 2117.51,1749.86C2111.73,1752.63 2101.55,1748.77 2094.13,1745.56C2098.76,1741.25 2102.04,1739.81 2111.29,1739.07C2103.62,1739.34 2098.84,1735.47 2092.48,1731.08C2098.56,1725.92 2104.88,1728.1 2113.69,1729.14C2107.38,1726.67 2101.22,1722.18 2095.15,1719.23C2101.54,1718.14 2111.32,1716.29 2116.29,1720.14C2116.29,1720.14 2142.34,1716.86 2149.12,1717.59C2139.44,1682.72 2099.96,1671.61 2099.46,1646.65C2098.59,1604.12 2119.25,1573.71 2143.67,1551.05Z";
        [Fact]
        public void TestMorphTests()
        {
            StringBuilder CommandListSource = new StringBuilder();
            StringBuilder CommandListTarget = new StringBuilder();
            foreach (Match match in commandregex.Matches(source))
            {
                CommandListSource.Append(match.Value.Substring(0, 1));
            }
            foreach (Match match in commandregex.Matches(target))
            {
                CommandListTarget.Append(match.Value.Substring(0, 1));
            }

            var sourcestring = CommandListSource.ToString();
            var targetstring = CommandListTarget.ToString();

            var morph = new PathMorph<char>(sourcestring.ToArray(), targetstring.ToArray());

        }
    }
}