using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    /// <summary>
    /// this is orginal implementation of vector for use in physics engine
    /// </summary>
    public class FkVector2
    {
        public readonly double X;
        public readonly double Y;        
        /// <summary>
        /// create vector [X, Y]
        /// </summary>
        public FkVector2(double x, double y) {
            X = x;
            Y = y;
        }        
        /// <summary>
        /// Add 2 vectors X+x & Y+y
        /// </summary>
        /// <returns>new vector with result</returns>
        public FkVector2 Add(FkVector2 v) { 
            return new FkVector2(X + v.X, Y + v.Y);
        }
        /// <summary>
        /// Substract 2 vectros X-x & Y-y
        /// </summary>
        /// <returns>new vector with result</returns>
        public FkVector2 Subtract(FkVector2 v)
        {
            return new FkVector2(X - v.X, Y - v.Y);
        }
        /// <summary>
        /// Returns vector with all values positive
        /// </summary>
        /// <returns>new vector with result</returns>
        public FkVector2 Absoulte() { 
            return new FkVector2(FkMath.Abs(X), FkMath.Abs(Y));
        }
        /// <summary>
        /// multiplies vector by scalar
        /// </summary>
        /// <returns>new vector with result</returns>
        public FkVector2 Multiply(double a){
            return new FkVector2(X * a, Y * a);
        }
        /// <summary>
        /// return direction vector of length 1
        /// </summary>
        /// <returns>new vector with result</returns>
        public FkVector2 Normalize() {
            return this.Multiply(1 / this.Length());
        }
        /// <summary>
        /// check if 2 vectrs are SIMILAR with error margin
        /// </summary>
        public bool EqualWIthError(FkVector2 other, double error = 0.01)
        {
            return FkMath.EqualWIthError(X, other.X, error) && FkMath.EqualWIthError(Y, other.Y, error);
        }        
        /// <summary>
        /// length of vector squared (cheaper than with Sqrt)
        /// </summary>
        public double LengthSquared(){
            return X * X + Y * Y;
        }
        /// <summary>
        /// length of vector - if possible use LengthSquared.
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }
        /// <summary>
        /// returns dot product of 2 vectors
        /// </summary>
        /// <returns>for normalized vector: 1 if aligned, 0 is perpendiciular, -1 if oposite</returns>
        public double DotProdcut(FkVector2 other) {
            return (this.X * other.X) + (this.Y * other.Y);
        }

        
        public override bool Equals(object obj)
        {
            return obj is FkVector2 vector &&
                   X == vector.X &&
                   Y == vector.Y;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        public override string ToString()
        {
            return base.ToString()+$" X: {this.X},Y: {this.Y}";
        }
    }
}
