using System.Diagnostics;
using framer.Common;

namespace framer.VectAlign.Techniques
{
    [DebuggerDisplay("{PrintMatrix}")]
    public class NeedlemanWunschAlignment
    {
        private readonly List<PathCommand> _listA;
        private readonly List<PathCommand> _listB;
        private readonly List<PathCommand> _originalListA;
        private readonly List<PathCommand> _originalListB;
        private List<PathCommand> _alignedListA;
        private List<PathCommand> _alignedListB;
        private int[][] _scoreMatrix;

        private const int Match = 1;
        public const int Missmatch = -1;
        private const int Indel = 0;

        public NeedlemanWunschAlignment(List<PathCommand> from, List<PathCommand> to)
        {
            _originalListA = from;
            _originalListB = to;

            _listA = new List<PathCommand>();
            _listA.AddRange(_originalListA);

            _listB = new List<PathCommand>();
            _listB.AddRange(_originalListB);

            //Add dumb command at start
            PathCommand dumbNode = new PathCommand(CommandType.D);
            _listA.Insert(0, dumbNode);
            _listB.Insert(0, dumbNode);
            Align();
        }

        private void InitMatrixD()
        {
            _scoreMatrix = new int[_listA.Count + 1][];
            for  (var i = 0; i < _scoreMatrix.Length;i++)
            {
                _scoreMatrix[i] = new int[_listB.Count + 1];
            }
            for (var i = 0; i <= _listA.Count; i++)
            {
                for (var j = 0; j <= _listB.Count; j++)
                {
                    if (i == 0)
                        _scoreMatrix[i][j] = -j;
                    else if (j == 0)
                        _scoreMatrix[i][j] = -i;
                    else
                        _scoreMatrix[i][j] = 0;
                }
            }
        }

        private void Align()
        {
            InitMatrixD();
            _alignedListA = new List<PathCommand>();
            _alignedListB = new List<PathCommand>();

            //process matrix D
            for (var a = 1; a <= _listA.Count; a++)
            {
                for (var b = 1; b <= _listB.Count; b++)
                {
                    var scoreDiag = _scoreMatrix[a - 1][b - 1] + GetScore(a, b);
                    var scoreLeft = _scoreMatrix[a][b - 1] + Indel;
                    var scoreUp = _scoreMatrix[a - 1][b] + Indel;
                    _scoreMatrix[a][b] = Math.Max(Math.Max(scoreDiag, scoreLeft), scoreUp);
                }
            }

            //backtracking
            var i = _listA.Count;
            var j = _listB.Count;
            var arrayType = new float[] { };
            while (i > 0 && j > 0)
            {

                PathCommand newNodeA;
                PathCommand newNodeB;
                if (_scoreMatrix[i][j] == _scoreMatrix[i - 1][j - 1] + GetScore(i, j))
                {
                    newNodeA = _listA[i - 1];
                    newNodeB = _listB[j - 1];
                    i--;
                    j--;
                }
                else if (_scoreMatrix[i][j] == _scoreMatrix[i][j - 1] + Indel)
                {
                    newNodeA = new PathCommand(CommandType._, arrayType);
                    newNodeB = _listB[j - 1];
                    j--;
                }
                else
                {
                    newNodeA = _listA[i - 1];
                    newNodeB = new PathCommand(CommandType._, arrayType);
                    i--;
                }

                //insert new nodes in reverse order
                _alignedListA.Insert(0, newNodeA);
                _alignedListB.Insert(0, newNodeB);
            }

            //Remove dumb nodes
            _alignedListA = _alignedListA.Omit(node=> node.Type == CommandType.D).ToList();
            _alignedListB = _alignedListB.Omit(node => node.Type == CommandType.D).ToList();
        }

        private int GetScore(int i, int j)
        {
            return _listA[i - 1].Type == _listB[j - 1].Type ? Match : //Arbitrary positive score
                Missmatch; //Arbitrary negative score
        }

        public int OverallScore => _scoreMatrix.Sum(t => t.Sum());
        

        public List<PathCommand> AlignedFrom => _alignedListA;
        public List<PathCommand> AlignedTo => _alignedListB;
        public List<PathCommand> OriginalFrom => _originalListA;
        public List<PathCommand> OriginalTo => _originalListB;
        

        private string PrintMatrix => string.Join(Environment.NewLine, _scoreMatrix.Select(row => string.Join(",", row.Select(f => f.ToString()))));
        
    }
}