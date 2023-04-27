using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace framer
{
    public class PathMorph<T> where T : notnull
    {
        private readonly int levensteindistance;
        private readonly int[,] resultMatrix;
        private readonly Tuple<int, int>[,] backtrackMatrix;
        private readonly T[] aInput;
        private readonly T[] bInput;
        //private readonly T[] aAligned;
        //private readonly T[] bAligned;

        //public T[] AALigner => aAligned;
        //public T[] BALigner => bAligned;

        public PathMorph(T[] a, T[] b)
        {
            aInput = a;
            bInput = b;
            var result = needlemanWunsch(a, b);
            
            levensteindistance = result.Item1;
            resultMatrix = result.Item2;
            backtrackMatrix = result.Item3;
            //calculate alignment
        }


        //left gap in a
        //up   gab in b
        //diagonal -> letters
        public static IEnumerable<Tuple<int, int>> Backtrack(int d, int h, int v, int min)
        {
            if (d == min)
                yield return Tuple.Create(-1, -1);
            if (h == min)
                yield return Tuple.Create(-1, 0);
            if (v == min)
                yield return Tuple.Create(0, -1);
        }
        public static Tinput Min<Tinput>(params Tinput[] input) where Tinput : notnull
        {
            Tinput result = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (Comparer<Tinput>.Default.Compare(input[i], result) < 0)
                    result = input[i];
            }
            return result;
        }
        public static int Delta<Tinput>(Tinput x, Tinput y) where Tinput : notnull
        {
            return x.Equals(y) ? 0 : 1;
        }
        public static Tuple<int, int[,], Tuple<int, int>[,]> needlemanWunsch<Tinput>(Tinput[] a, Tinput[] b) where Tinput : notnull
        {
            var alength = a.Length;
            var blength = b.Length;
            var mat = new int[alength + 1, blength + 1];
            var backtrackmat = new Tuple<int, int>[alength + 1, blength + 1];

            mat[0, 0] = 0;
            backtrackmat[0, 0] = Tuple.Create(0, 0);
            for (int i = 1; i < alength; i++)
            {
                mat[i, 0] = i;
                backtrackmat[i, 0] = Tuple.Create(-1, 0);
            }
            for (int j = 1; j < blength; j++)
            {
                mat[0, j] = j;
                backtrackmat[0, j] = Tuple.Create(0, -1);
            }

            for (int i = 1; i < alength; i++)
                for (int j = 1; j < blength; j++)
                {
                    var d = mat[i - 1, j - 1] + Delta(a[i], b[j]);
                    var h = mat[i, j - 1] + 1;
                    var v = mat[i - 1, j] + 1;
                    mat[i, j] = Min(d, h, v);
                    backtrackmat[i, j] = Backtrack(d, h, v, mat[i, j]).First();
                }

            return Tuple.Create(mat[alength, blength], mat, backtrackmat);
        }



    }
}
