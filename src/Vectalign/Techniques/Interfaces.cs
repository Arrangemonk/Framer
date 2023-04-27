using framer.Common;

namespace framer.VectAlign.Techniques
{
    public interface IAlignMode
    {
        List<List<PathCommand>> Align(List<PathCommand> from, List<PathCommand> to);
    }
    public interface IFillMode
    {
        void FillInjectedNodes(List<PathCommand> from, List<PathCommand> to);
    }
}
