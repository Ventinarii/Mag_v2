using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    public class FkMatrix2
    {
        //public final matrix values
        public readonly double 
            d00, d01,
            d10, d11;
        //saved info about roatation (optional)
        public readonly double RotationDeg;
        //cahe of reversed matrix
        private FkMatrix2 inversed;    
        /// <summary>
        /// Create -=CUSTOM=- matrix
        /// this can be used to create streach, shear and other matrixes. Since it uses det to inverse matrix it can UNTRANSLATE whatever this will do
        /// </summary>
        /// <param name="d00">left top</param>
        /// <param name="d01">right top</param>
        /// <param name="d10">left bottom</param>
        /// <param name="d11">tight bottom</param>
        public FkMatrix2(double d00, double d01, double d10, double d11)
        {
            this.d00 = d00; this.d01 = d01;
            this.d10 = d10; this.d11 = d11;
            this.RotationDeg = 0;
        }
        /// <summary>
        /// Create matrix that will rotate vetors by angle RotationDeg
        /// </summary>
        /// <param name="RotationDeg">rotation in degrees</param>
        public FkMatrix2(double RotationDeg)
        {
            double radians = FkMath.DegToRadians(RotationDeg);

            d00 = Math.Cos(radians); d01 = Math.Sin(radians);
            d10 = -d01; d11 = d00;
            this.RotationDeg = RotationDeg;
        }       
        /// <summary>
        /// private constructor without ANY constrains.
        /// </summary>
        /// <param name="d00">left top</param>
        /// <param name="d01">right top</param>
        /// <param name="d10">left bottom</param>
        /// <param name="d11">tight bottom</param>
        /// <param name="RotationDeg">rotation in degrees</param>
        /// <param name="StrechValue">scalling in vector 2</param>
        /// <param name="UsedAs">define mode of use</param>
        private FkMatrix2(double d00, double d01, double d10, double d11, double RotationDeg = 0)
        {
            this.d00 = d00; this.d01 = d01;
            this.d10 = d10; this.d11 = d11;

            this.RotationDeg = RotationDeg;
        }
        /// <summary>
        /// translates
        /// named this way to make code easier to read
        /// </summary>
        /// <param name="v">vector to translate</param>
        /// <returns>new vector with result</returns>
        public FkVector2 Rotate(FkVector2 v)
        {
            return new FkVector2(
                    (v.X * d00) + (v.Y * d01),
                    (v.X * d10) + (v.Y * d11)
            );
        }
        /// <summary>
        /// translates given vecotr by inverse matrix
        /// named this way to make code easier to read
        /// </summary>
        /// <param name="v">vector to UnTranslate</param>
        /// <returns>new vector with result</returns>
        public FkVector2 UnRotate(FkVector2 v)
        {
            return getInversed().Rotate(v);
        }
        /// <summary>
        /// Used to inverse matrix. 
        /// </summary>
        /// <returns>new inverted matrix</returns>
        public FkMatrix2 getInversed()
        {
            if (inversed == null)
            {
                double det = (d00 * d11) - (d10 * d01);
                double n00 = d11, n01 = -d01,
                       n10 = -d10, n11 = d00;

                n00 /= det; 
                n01 /= det;

                n10 /= det; 
                n11 /= det;
                inversed = new FkMatrix2(n00, n01, n10, n11, -RotationDeg);
            }
            return inversed;
        }

        public override bool Equals(object obj)
        {
            return obj is FkMatrix2 matrix &&
                   d00 == matrix.d00 &&
                   d01 == matrix.d01 &&
                   d10 == matrix.d10 &&
                   d11 == matrix.d11 &&
                   RotationDeg == matrix.RotationDeg;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(d00, d01, d10, d11, RotationDeg);
        }
        public override string ToString()
        {
            return base.ToString()+$"d00: {this.d00},d01: {this.d01},d10: {this.d10},d11: {this.d11},RotationDeg: {this.RotationDeg}";
        }
    }
}
