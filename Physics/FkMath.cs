using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    public class FkMath
    {
        /// <summary>
        /// this function i s used to ensure that val is in between from and to regardless of that is greater
        /// </summary>
        /// <param name="from">border A</param>
        /// <param name="val">value</param>
        /// <param name="to">border B</param>
        /// <returns>value within constains</returns>
        public static double Clamp(double from, double val, double to) {
            var from2 = from;
            var to2 = to;
            if (from > to) {
                from2 = to;
                to2 = from;
            }
            return (val < from2) ? (from2) : ((to2 < val) ? (to2) : (val));
        }
        
        /// <summary>
        /// this function i s used to check IF val is in between from and to regardless of that is greater
        /// </summary>
        /// <param name="from">border A</param>
        /// <param name="val">value</param>
        /// <param name="to">border B</param>
        /// <returns>true if value is within constrains</returns>
        public static bool InRange(double from, double val, double to) {
            return (from < to) ? (from <= val && val <= to) : (to <= val && val <= from);
        }
        
        /// <summary>
        /// absolute value of double d
        /// </summary>
        /// <param name="d"></param>
        /// <returns>absolute value</returns>
        public static double Abs(double d) {
            return (d < 0) ? (-d) : (d);
        }
        
        /// <summary>
        /// check if 2 numbers are equal with error margin (numeric instability)
        /// return (Abs(value1 - value2) <= Abs(error));
        /// </summary>
        /// <param name="value1">value to compare A</param>
        /// <param name="value2">value to compare A</param>
        /// <param name="error">error margin</param>
        /// <returns></returns>
        public static bool EqualWIthError(double value1, double value2, double error = 0.0001) {
            return Abs(value1 - value2) <= Abs(error);
        }
        
        /// <summary>
        /// this function is used to convert value in degrees (even multiplication of 360% or negative values)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>value in radians</returns>
        public static double DegToRadians(double angle)
        {
            double radians = (angle % 360);//stay in 0-360
            radians *= Math.PI / 180;//add PI end result =>  ( (360+ (rotationDeg%360) )%360 )*(2*Math.PI/360); // but cheaper (avoided another '%')
            return radians;
        }

        public static double Pow2 (double a)
        {
            return a * a;
        }
    }
}
