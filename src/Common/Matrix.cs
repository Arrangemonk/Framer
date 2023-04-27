using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace framer.Common
{
    /// <summary>
    /// Matrix math from ShapeShifter https://github.com/alexjlockwood/ShapeShifter
    /// </summary>
    public class Matrix
    {
        public float A { get; }
        public float B { get; }
        public float C { get; }
        public float D { get; }
        public float E { get; }
        public float F { get; }

        public static Matrix Identity => new(1, 0, 0, 1, 0, 0);

        /// <summary>
        /// Flattens the matrices into a single matrix by performing matrix multiplication
        /// on each in left to right order.
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public static Matrix Flatten(List<Matrix> matrices)
        {
            return matrices.Aggregate(Identity, (prev, curr) => prev.Dot(curr));
        }

        /// <summary>
        /// Creates a scaling Transformation matrix.
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <returns></returns>
        public static Matrix Scaling(float sx, float sy)
        {
            return new Matrix(sx, 0, 0, sy, 0, 0);
        }

        /// <summary>
        /// Creates a counter clockwise rotation Transformation matrix.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Matrix Rotation(float degrees)
        {
            var cosr = (float)Math.Cos((degrees * Math.PI) / 180);
            var sinr = (float)Math.Sin((degrees * Math.PI) / 180);
            return new Matrix(cosr, sinr, -sinr, cosr, 0, 0);
        }

        /// <summary>
        /// Creates a translation Transformation matrix.
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static Matrix Translation(float tx, float ty)
        {
            return new Matrix(1, 0, 0, 1, tx, ty);
        }

        public Matrix(
       float a,
       float b,
       float c,
       float d,
       float e,
       float f
      )
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
        }


        /// <summary>
        /// Returns the dot product of this 2D Transformation matrices with m.
        /// [a c e]   [a' c' e']
        /// [b d f] * [b' d' f']
        /// [0 0 1]   [0  0  1 ]
        /// </summary>
        /// <returns></returns>
        public Matrix Dot(Matrix m)
        {
            return new Matrix(
              MathUtil.Round(A * m.A + C * m.B),
              MathUtil.Round(B * m.A + D * m.B),
              MathUtil.Round(A * m.C + C * m.D),
              MathUtil.Round(B * m.C + D * m.D),
              MathUtil.Round(A * m.E + C * m.F + this.E),
              MathUtil.Round(B * m.E + D * m.F + this.F));
        }


        /// <summary>
        /// Returns the inverse of this Transformation matrix or undefined if the
        /// matrix is not invertible.
        /// </summary>
        /// <returns></returns>
        public Matrix? Invert()
        {
            var det = MathUtil.Round(A * D - B * C);
            if (MathUtil.IsNearZero(det))
            {
                return null;
            }
            det = 1 / det;
            return new Matrix(
              MathUtil.Round(D * det),
              MathUtil.Round(-B * det),
              MathUtil.Round(-C * det),
              MathUtil.Round(A * det),
              MathUtil.Round((C * F - D * E) * det),
              MathUtil.Round((B * E - A * F) * det));
        }

        /// <summary>
        ///  Extracts the x/y scaling from the Transformation matrix.
        /// </summary>
        /// <returns></returns>
        public (float, float) GetScaling()
        {
            float sx = (A >= 0 ? 1 : -1) * MathUtil.Hypot(A, C);
            float sy = (D >= 0 ? 1 : -1) * MathUtil.Hypot(B, D);
            return (MathUtil.Round(sx), MathUtil.Round(sy));
        }

        /// <summary>
        ///  Extracts the rotation in degrees from the Transformation matrix.
        /// </summary>
        /// <returns></returns>
        public float GetRotation()
        {
            return (float)MathUtil.Round((180 / Math.PI) * Math.Atan2(-C, A));
        }

        /// <summary>
        /// Extracts the x/y translation from the Transformation matrix.
        /// </summary>
        /// <returns></returns>
        public (float, float) GetTranslation()
        {
            return (MathUtil.Round(E), MathUtil.Round(F));
        }

        /// <summary>
        /// Returns a single scale factor (to use for scaling a path's stroke width, etc.).
        /// Given unit vectors u0 = (0, 1) and v0 = (1, 0).
        ///
        /// After matrix mapping, we get u1 and v1. Let Θ be the angle between u1 and v1.
        /// Then the final scale we want is:
        ///
        /// Math.Min(|u1|sin(Θ),|v1|sin(Θ)) = |u1||v1|sin(Θ) / Math.Max(|u1|,|v1|)
        ///
        /// If Math.Max(|u1|,|v1|) = 0, that means either x or y has a scale of 0.
        ///
        /// For the non-skew case, which is most of the cases, matrix scale is
        /// computing exactly the scale on x and y axis, and take the minimal of these two.
        ///
        /// For the skew case, an unit square will mapped to a parallelogram,
        /// and this function will return the minimal height of the 2 bases.
        /// </summary>
        /// <returns></returns>
        public float GetScaleFactor()
        {
            var m = new Matrix(A, B, C, D, 0, 0);
            var u0 = new Point(0f, 1f);
            var v0 = new Point(1f, 0f);
            var u1 = MathUtil.TransformPoint(u0, m);
            var v1 = MathUtil.TransformPoint(v0, m);
            var sx = MathUtil.Hypot(u1.X, u1.X);
            var sy = MathUtil.Hypot(v1.Y, v1.Y);
            var dotProduct = u1.Y * v1.X - u1.X * v1.Y;
            var maxScale = Math.Max(sx, sy);
            return maxScale > 0 ? Math.Abs(dotProduct) / maxScale : 0;
        }

        /// <summary>
        /// Returns true if the matrix is approximately equal to this matrix.
        /// </summary>
        public bool Equals(Matrix m)
        {
            return (
              Math.Abs(this.A - m.A) < 1e-9 &&
              Math.Abs(this.B - m.B) < 1e-9 &&
              Math.Abs(this.C - m.C) < 1e-9 &&
              Math.Abs(this.D - m.D) < 1e-9 &&
              Math.Abs(this.E - m.E) < 1e-9 &&
              Math.Abs(this.F - m.F) < 1e-9
            );
        }
    }
}
