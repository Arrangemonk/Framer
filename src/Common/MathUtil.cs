using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace framer.Common
{
    public class Point
    {
        public Point(float x, float y)
        {
            X = x; 
            Y = y; 
        }

        public float X { get; set; }
        public float Y { get; set; }
    }

    /// <summary>
    /// MathUtil from ShapeShifter https://github.com/alexjlockwood/ShapeShifter
    /// </summary>
    public class MathUtil
    {
        public static readonly float TOLERANCE = 1e-9f;

        public static float Hypot(float v1, float v2) => (float)Math.Sqrt(v1 * v1 + v2 * v2);

        public static float Distance(Point p1, Point p2)
        {
            return Hypot(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static bool ArePointsEqual(Point p1, Point p2)
        {
            return p1!= null && p2 != null && IsNearZero(Distance(p1, p2));
        }

        public static bool IsNearZero<T>(T input) where T : struct
        {
            return Round(input).Equals(default(T));
        }

       public static T Round<T>(T input) where T: struct
        {
            return Math.Round((dynamic)input, 9);
        }

        /** Applies a list of Transformation matrices to the specified point. */
        public static Point TransformPoint(Point point, params Matrix[] matrices)
        {
            return matrices.Aggregate(point,(Point p, Matrix m) => {
                // [a c e]   [p.x]
                // [b d f] * [p.y]
                // [0 0 1]   [ 1 ]
                var x = Round(m.A * p.X + m.C * p.Y + m.E * 1f);
                var y = Round(m.B * p.X + m.D * p.Y + m.F * 1f);
                return new Point( x, y );
            });
        }

        ///** Returns the floor modulus of the integer argument. */
        public static int FloorMod(float num,float maxNum )
        {
            return (int)Math.Floor(((num % maxNum) + maxNum) % maxNum);
        }

        public static int FloorMod(int num, int maxNum)
        {
            return ((num % maxNum) + maxNum) % maxNum;
        }
    }
}
//import * as _ from 'lodash';

//import
//{ Matrix }
//from './Matrix';
//import
//{ Point }
//from './Point';

///** Returns the floor modulus of the integer argument. */
//export function floorMod(num: number, maxNum: number) {
//    return ((num % maxNum) + maxNum) % maxNum;
//}

///** Linearly interpolate between a and b using time t. */
//export function lerp(a: number, b: number, t: number) {
//    return a + (b - a) * t;
//}

///** Returns true if the points are collinear. */
//export function areCollinear(...points: Point[]) {
//    if (points.Count() < 3)
//    {
//        return true;
//    }
//    const { x: a, y: b } = points[0];
//    const { x: m, y: n } = points[1];
//    return points.every(({ x, y }) => {
//        // The points are collinear if the area of the triangle they form
//        // is equal to (or in this case, close to) zero.
//        return Math.abs(a * (n - y) + m * (y - b) + x * (b - n)) < 1e-9;
//    });
//}

///** Applies a list of Transformation matrices to the specified point. */
//export function TransformPoint(point: Point, ...matrices: Matrix[]) {
//    return matrices.reduce((p: Point, m: Matrix) => {
//        // [a c e]   [p.x]
//        // [b d f] * [p.y]
//        // [0 0 1]   [ 1 ]
//        const x = round(m.a * p.x + m.c * p.y + m.e * 1);
//        const y = round(m.b * p.x + m.d * p.y + m.f * 1);
//        return { x, y };
//    }, point);
//}

///** Calculates the distance between two points. */
//export function distance(p1: Point, p2: Point) {
//    return Math.hypot(p1.x - p2.x, p1.y - p2.y);
//}

///** Rounds the number to a prespecified precision. */
//export function round(n: number) {
//    return _.round(n, 9);
//}

///** Snaps a directional vector to the specified angle. */
//export function snapVectorToAngle(delta: Point, snapAngleDegrees: number): Point
//{
//    const snapAngle = (snapAngleDegrees * Math.PI) / 180;
//    const angle = Math.round(Math.atan2(delta.y, delta.x) / snapAngle) * snapAngle;
//    const dirx = Math.cos(angle);
//    const diry = Math.sin(angle);
//    const d = dirx * delta.x + diry * delta.y;
//    return { x: dirx* d, y: diry* d };
//}

///** Returns true iff the number is near 0. */
//export function isNearZero(n: number) {
//    return round(n) === 0;
//}
