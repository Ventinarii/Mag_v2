using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    /// <summary>
    /// this is orginal implementation of matrix for use in physics engine
    /// 
    /// since this class fulfill 3 diffrent functions in code (rotate, scale and move(optional) it has MODE.
    /// MODE is READONLY and is set on creation. 
    /// call of function that is NOT compatible with MODE will result in Exception. This is intentinal and is meant to indidate that matrix is used in unusual way. 
    /// if you want to call those methods ANYWAY then use Translation. 
    /// </summary>
    public class FkMatrix2
    {
        //public final matrix values
        public readonly double 
            d00, d01,
            d10, d11;

        //to avoid mess each matrix can fulfill only ONE function at the time
        public enum Mode{ 
            custom = 1,//SOME matrix defined by user - can be used to transform vector
            rotation = 2,
            strech = 3
        }
        public readonly Mode UsedAs;
        //saved info about roatation (optional)
        public readonly double RotationDeg;
        //saved info about strech (optional)
        public readonly FkVector2 StrechValue;
        //cahe of reversed matrix
        private FkMatrix2 inversed;

        //=====================================================================================================================CONSTRUCTORS
       
        /// <summary>
        /// Create -=CUSTOM=- matrix
        /// </summary>
        /// <param name="d00">left top</param>
        /// <param name="d01">right top</param>
        /// <param name="d10">left bottom</param>
        /// <param name="d11">tight bottom</param>
        public FkMatrix2(double d00, double d01, double d10, double d11)
        {
            this.d00 = d00; this.d01 = d01;
            this.d10 = d10; this.d11 = d11;

            UsedAs = Mode.custom;
            this.RotationDeg = 0;
            StrechValue = new FkVector2(0, 0);
        }
        
        /// <summary>
        /// Create -=ROTATION=- matrix
        /// </summary>
        /// <param name="RotationDeg">rotation in degrees</param>
        public FkMatrix2(double RotationDeg)
        {
            double radians = FkMath.DegToRadians(RotationDeg);

            d00 = Math.Cos(radians); d01 = Math.Sin(radians);
            d10 = -d01; d11 = d00;
                      
            UsedAs = Mode.rotation;
            this.RotationDeg = RotationDeg;
            StrechValue = new FkVector2(0, 0);
        }
       
        /// <summary>
        /// Create -=STRECH=- matrix
        /// </summary>
        /// <param name="StrechValue">scalling in vector 2</param>
        public FkMatrix2(FkVector2 StrechValue)
        {
            d00 = StrechValue.X;d01 = 0;
            d10 = 0;d11 = StrechValue.Y;

            UsedAs = Mode.strech;
            this.RotationDeg = 0;
            this.StrechValue = StrechValue;            
        }
        
        /// <summary>
        /// DUPLICATE matrix AND it's MODE.
        /// Copies matrix (since evrything is readonly kind of useless)
        /// </summary>
        /// <param name="copyOF">matrix to copy</param>
        public FkMatrix2(FkMatrix2 copyOF)
        {
            this.d00 = copyOF.d00;
            this.d01 = copyOF.d01;
            this.d10 = copyOF.d10;
            this.d11 = copyOF.d11;
            this.RotationDeg = copyOF.RotationDeg;
            this.StrechValue = copyOF.StrechValue;
            this.UsedAs = copyOF.UsedAs;
        }
        
        /// <summary>
        /// private constructor without ANY constrains. use with care.
        /// </summary>
        /// <param name="d00">left top</param>
        /// <param name="d01">right top</param>
        /// <param name="d10">left bottom</param>
        /// <param name="d11">tight bottom</param>
        /// <param name="RotationDeg">rotation in degrees</param>
        /// <param name="StrechValue">scalling in vector 2</param>
        /// <param name="UsedAs">define mode of use</param>
        private FkMatrix2(double d00, double d01, double d10, double d11, double RotationDeg = 0, FkVector2 StrechValue = null, Mode UsedAs = Mode.custom)
        {
            this.d00 = d00; this.d01 = d01;
            this.d10 = d10; this.d11 = d11;
            this.RotationDeg = RotationDeg;
            this.StrechValue = StrechValue;
            this.UsedAs = UsedAs;
        }
        
        //=====================================================================================================================ROTATIONS
        
        /// <summary>
        /// For -=ROTATION=- matrix.
        /// </summary>
        /// <param name="v">vector to rotate</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if used in wrong mode</exception>
        public FkVector2 Rotate(FkVector2 v)
        {
            if (UsedAs != Mode.rotation)
                throw new InvalidOperationException("tried to rotate vector using "+UsedAs+" matrix");
            return Translate(v);
        }
        
        /// <summary>
        /// For -=ROTATION=- matrix.
        /// uses Inverse matrix.
        /// </summary>
        /// <param name="v">vector to rotate</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if used in wrong mode</exception>
        public FkVector2 UnRotate(FkVector2 v)
        {
            if (UsedAs != Mode.rotation)
                throw new InvalidOperationException("tried to rotate vector using " + UsedAs + " matrix");
            return UnTranslate(v);
        }

        //=====================================================================================================================STRECH
        
        /// <summary>
        /// For -=STRECH=- matrix.
        /// </summary>
        /// <param name="v">vector to scale(strech)</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if used in wrong mode</exception>
        public FkVector2 Strech(FkVector2 v)
        {
            if (UsedAs != Mode.strech)
                throw new InvalidOperationException("tried to strech vector using " + UsedAs + " matrix");
            return Translate(v);
        }
        
        /// <summary>
        /// For -=STRECH=- matrix.
        /// uses Inverse matrix.
        /// </summary>
        /// <param name="v">vector to scale(strech)</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if used in wrong mode</exception>
        public FkVector2 UnStrech(FkVector2 v)
        {
            if (UsedAs != Mode.strech)
                throw new InvalidOperationException("tried to strech vector using " + UsedAs + " matrix");
            return UnTranslate(v);
        }
        
        //=====================================================================================================================TECHNICAL
        
        /// <summary>
        /// For ANY matrix.
        /// </summary>
        /// <param name="v">vector to translate</param>
        /// <returns></returns>
        public FkVector2 Translate(FkVector2 v)
        {
            return new FkVector2(
                    (v.X * d00) + (v.Y * d01),
                    (v.X * d10) + (v.Y * d11)
            );
        }
       
        /// <summary>
        /// For ANY matrix.
        /// uses Inverse matrix.
        /// </summary>
        /// <param name="v">vector to translate</param>
        /// <returns></returns>
        public FkVector2 UnTranslate(FkVector2 v)
        {
            return getInversed().Translate(v);
        }
        
        /// <summary>
        /// Used to inverse matrix. 
        /// </summary>
        /// <returns></returns>
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
                inversed = new FkMatrix2(n00, n01, n10, n11, -RotationDeg, StrechValue.Multiply(-1), UsedAs);
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
                   UsedAs == matrix.UsedAs &&
                   RotationDeg == matrix.RotationDeg &&
                   EqualityComparer<FkVector2>.Default.Equals(StrechValue, matrix.StrechValue);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(d00, d01, d10, d11, UsedAs, RotationDeg, StrechValue);
        }
        public override string ToString()
        {
            return base.ToString()+$"d00: {this.d00},d01: {this.d01},d10: {this.d10},d11: {this.d11},UsedAs: {this.UsedAs},RotationDeg: {this.RotationDeg},StrechValue {this.StrechValue}";
        }
    }
}
