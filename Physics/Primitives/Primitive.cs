using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics.Primitives
{
    public class Primitive
    {
        //these variables are responsible for positioning of object in THIS simulation frame
        public FkVector2 Position { get; set; }
        public double Rotation { get; set; }
        //these variables are responsible for storing MOVMENT of object in between THIS frame and NEXT frame
        //(i.e after update they sould be already up to date)
        public FkVector2 Velocity { get; set; }
        public double Angular { get; set; }
        //these variables are responsible for DESCRIPTION of object (what is it, what shape it has, does it move)
        public readonly FkVector2 Scale;//IF IsBox==false then we ONLY use X from vector as circle RADIUS
        public readonly FkMatrix2 ScaleMatrix;
        public readonly bool IsBox;//in simulation we only got boxes(that can be streched using Scale) and circles
        public readonly bool IsStatic;//is the obect static (used as map boudary or obstacle)

        /// <summary>
        /// this is single constructor for objects of this class. it can go without any arguments - in such a case it will: 
        /// generate Static Circle of radius 1 at X:0 Y:0 R:0 with dX:0 dY:0 dR:0
        /// Pay attention as calling Box functions on circle and vice versa will throw exception.
        /// </summary>
        /// <param name="Position">Position of center of mass at given moment</param>
        /// <param name="Rotation">Rotation (reference point center of mass) at given moment in degrees</param>
        /// <param name="Velocity">Velocity (in U/sec) at given moment</param>
        /// <param name="Angular">Angular velocity(rotation) (in deg/sec) at given moment</param>
        /// <param name="Scale">Scale of objeect. can't be changed, IF primitive is NOT box ony X is used as RADIUS</param>
        /// <param name="IsBox">Type of object. can't be changed</param>
        /// <param name="IsStatic">Is movable. can't be changed</param>
        public Primitive(
            FkVector2 Position = null,
            double Rotation = 0,
            FkVector2 Velocity = null,
            double Angular = 0,
            FkVector2 Scale = null,
            bool IsBox = false,
            bool IsStatic = false) {

            if (Position == null)
                Position = new FkVector2(0, 0);
            this.Position = Position;

            this.Rotation = Rotation;

            if (Velocity == null)
                Velocity = new FkVector2(0, 0);
            this.Velocity = Velocity;

            this.Angular = Angular;

            if (Scale == null)
                Scale = new FkVector2(1, 1);
            Scale = Scale.Absoulte();
            if (Scale.X < 0.1)
                Scale = new FkVector2(.1, Scale.Y);
            if (Scale.Y < 0.1)
                Scale = new FkVector2(Scale.X, .1);
            this.Scale = Scale;

            this.IsBox = IsBox;

            this.IsStatic = IsStatic;
        }

        //========================================================================================================================circle functionality

        /// <summary>
        /// checks if circle contains given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool PointInCircle(FkVector2 point)
        {
            if (this.IsBox)
                throw new InvalidOperationException("this is box, not circle");

            point = point.Subtract(this.Position);//now circle is point of reference
            return (point.LengthSquared()) <= (this.Scale.X * this.Scale.X);
        }

        /// <summary>
        /// check if line intersect circle
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool LineInCircle(FkVector2 lineStart, FkVector2 lineEnd) {
            if (this.IsBox)
                throw new InvalidOperationException("this is box, not circle");
            lineStart = lineStart.Subtract(this.Position);
            lineEnd = lineEnd.Subtract(this.Position);

            var radius2 = Scale.X * this.Scale.X;
            if ((lineStart.LengthSquared() <= radius2) || (lineEnd.LengthSquared() <= radius2))
                return true;

            var ab = lineEnd.Subtract(lineStart);
            var t = lineStart.Multiply(-1).DotProdcut(ab) / ab.DotProdcut(ab);

            if (!FkMath.InRange(0, t, 1))
                return false;

            var randevu = lineStart.Add(ab.Multiply(t));

            return randevu.LengthSquared() <= radius2;
        }

        //========================================================================================================================box functionality

        /// <summary>
        /// -returns all (4) corners of the box in array ([4]) based of scale. every box is size of (1,1) so scale IS box size.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public FkVector2[] BoxGetVerticesRelative() {
            if (!this.IsBox)
                throw new InvalidOperationException();

            FkVector2[] corners = new FkVector2[4];
            corners[0] = this.Scale;
            corners[1] = new FkVector2(Scale.X, -Scale.Y);
            corners[2] = corners[0].Multiply(-1);
            corners[3] = corners[1].Multiply(-1);
            return corners;
        }

        /// <summary>
        /// -returns all (4) corners of the box in array ([4]) based of scale. every box is size of (1,1) so scale IS box size.
        /// -adds this.Position to result
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public FkVector2[] BoxGetVertices()
        {
            if (!this.IsBox)
                throw new InvalidOperationException();

            FkVector2[] corners = BoxGetVerticesRelative();
            for (int i = 0; i < 4; i++)
                corners[i] = corners[i].Add(this.Position);
            return corners;
        }
        //========================================

        /// <summary>
        /// -returns all (4) corners of the box in array ([4]) based of scale. every box is size of (1,1) so scale IS box size.
        /// -rotates all points (as vectors) from center of mass by angle this.Rotation
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public FkVector2[] BoxGetVerticesRelativeRotated()
        {
            if (!this.IsBox)
                throw new InvalidOperationException();

            FkVector2[] corners = new FkVector2[4];
            corners[0] = this.Scale;
            corners[1] = new FkVector2(this.Scale.X, -this.Scale.Y);
            corners[2] = corners[0].Multiply(-1);
            corners[3] = corners[1].Multiply(-1);

            var rotationMatrix = new FkMatrix2(this.Rotation);

            for (int i = 0; i < 4; i++)
                corners[i] = rotationMatrix.Rotate(corners[i]);

            return corners;
        }

        /// <summary>
        /// -returns all (4) corners of the box in array ([4]) based of scale. every box is size of (1,1) so scale IS box size.
        /// -rotates all points (as vectors) from center of mass by angle this.Rotation
        /// -adds this.Position to result
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public FkVector2[] BoxGetVerticesRotated()
        {
            if (!this.IsBox)
                throw new InvalidOperationException();

            FkVector2[] corners = BoxGetVerticesRelativeRotated();
            for (int i = 0; i < 4; i++)
                corners[i] = corners[i].Add(this.Position);
            return corners;
        }

        /// <summary>
        /// checks if box contains given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool PointInBox(FkVector2 point) {
            if (!this.IsBox)
                throw new InvalidOperationException("this is circle, not box");

            point = point.Subtract(this.Position);//now box center is point of reference
            point = new FkMatrix2(-this.Rotation).Rotate(point);//now box is aligned with axes

            return FkMath.InRange(-Scale.X - 0.01, point.X, Scale.X + 0.01) &&
                   FkMath.InRange(-Scale.Y - 0.01, point.Y, Scale.Y + 0.01);
        }

        /// <summary>
        /// check if line intersect box
        /// 
        /// NOTE: solution ttrl#7 while is quicker it has THE SAME problem as org## and thus "chk for sneaky #1" was added.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool LineInBox(FkVector2 lineStart, FkVector2 lineEnd)
        {
            if (!this.IsBox)
                throw new InvalidOperationException("this is circle, not box");

            var matrix = new FkMatrix2(-this.Rotation);

            lineStart = matrix.Rotate(lineStart.Subtract(this.Position));
            lineEnd = matrix.Rotate(lineEnd.Subtract(this.Position));

            if (FkMath.InRange(-Scale.X - 0.01, lineStart.X, Scale.X + 0.01) &&
                FkMath.InRange(-Scale.Y - 0.01, lineStart.Y, Scale.Y + 0.01))
                return true;
            if (FkMath.InRange(-Scale.X - 0.01, lineEnd.X, Scale.X + 0.01) &&
                FkMath.InRange(-Scale.Y - 0.01, lineEnd.Y, Scale.Y + 0.01))
                return true;

            var vert = BoxGetVertices();

            //=====chk for sneaky #1
            if(PointOnLine(lineStart, lineEnd, vert[0]) ||//check for sneaky colinear colision (case B in unit test)
               PointOnLine(lineStart, lineEnd, vert[1]) ||
               PointOnLine(lineStart, lineEnd, vert[2]) ||
               PointOnLine(lineStart, lineEnd, vert[3]))
                return true;

            //====================================================================================ttrl#7>>

            var unitVector = lineEnd.Subtract(lineStart);
            var unitVectorLength = unitVector.Length();
            if (unitVectorLength == 0)
                return true;
            unitVector = unitVector.Multiply(1/ unitVectorLength);

            unitVector = new FkVector2(
                (unitVector.X != 0) ? (1 / unitVector.X) : (0),
                (unitVector.Y != 0) ? (1 / unitVector.Y) : (0));

            var min = Scale.Multiply(-1);
            min = min.Subtract(lineStart).Multiply(unitVector);
            var max = Scale;
            max = max.Subtract(lineStart).Multiply(unitVector);

            var tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            var tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
            if (tmax < 0 || tmin > tmax)
                return false;

            var t = (tmin < 0) ? tmax : tmin;
            return t > 0 && t * t < lineEnd.Subtract(lineStart).LengthSquared();

            //====================================================================================org##>>
            /* too expensive 
            return
                LineOnLine(lineStart, lineEnd, vert[0], vert[1]) ||
                LineOnLine(lineStart, lineEnd, vert[1], vert[2]) ||
                LineOnLine(lineStart, lineEnd, vert[2], vert[3]) ||
                LineOnLine(lineStart, lineEnd, vert[3], vert[0]);
            */
        }

        //========================================================================================================================shared functionality

        /// <summary>
        /// -returns left bottom most corner of square containing this object uses:
        /// --this.BoxGetVerticesRelativeRotated()) if is a box
        /// --this.Scale, this.Scale.Multiply(-1) if circle
        /// as a source of points to compare. 
        /// </summary>
        /// <returns>first value is minimum, second maximum</returns>
        public C2<FkVector2, FkVector2> getMinMaxRelative() {
            if (this.IsBox)
            {
                var corners = BoxGetVerticesRelativeRotated();

                var minX = Double.MaxValue;
                var minY = Double.MaxValue;
                var maxX = Double.MinValue;
                var maxY = Double.MinValue;

                for (int i = 0; i < 4; i++)
                {
                    if (corners[i].X < minX)
                        minX = corners[i].X;
                    if (corners[i].Y < minY)
                        minY = corners[i].Y;

                    if (corners[i].X > maxX)
                        maxX = corners[i].X;
                    if (corners[i].Y > maxY)
                        maxY = corners[i].Y;
                }
                return new C2<FkVector2, FkVector2>(new FkVector2(minX, minY), new FkVector2(maxX, maxY));
            }
            else {
                var localScale = new FkVector2(Scale.X, Scale.X);
                return new C2<FkVector2, FkVector2>(localScale.Multiply(-1), localScale);
            }
        }

        /// <summary>
        /// -returns left bottom most corner of square containing this object uses:
        /// --this.BoxGetVerticesRelativeRotated()) if is a box
        /// --this.Scale, this.Scale.Multiply(-1) if circle
        /// as a source of points to compare. 
        /// -adds this.Position to result
        /// </summary>
        /// <returns>first value is minimum, second maximum</returns>
        public C2<FkVector2, FkVector2> getMinMax()
        {
            var c = getMinMaxRelative();
            return new C2<FkVector2, FkVector2>(c.a.Add(this.Position), c.b.Add(this.Position));
        }

        //========================================================================================================================physics functionality

        /// <summary>
        /// checks if bouding box of this promitive is in collision with bouding box of other primitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AABBcolision(Primitive other) {
            var delta = this.Position.Subtract(other.Position);

            var mx0 = getMinMaxRelative();
            var mx1 = other.getMinMaxRelative();

            var mxSUMA = mx0.a.Add(mx1.a);
            var mxSUMB = mx0.b.Add(mx1.b);

            return
                FkMath.InRange(mxSUMA.X, delta.X, mxSUMB.X) &&
                FkMath.InRange(mxSUMA.Y, delta.Y, mxSUMB.Y);
        }

        /// <summary>
        /// checks if bouding box of this promitive contains given point
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AABBcolision(FkVector2 other) {
            other = this.Position.Subtract(other);

            var mx0 = getMinMaxRelative();

            return
                FkMath.InRange(mx0.a.X, other.X, mx0.b.X) &&
                FkMath.InRange(mx0.a.Y, other.Y, mx0.b.Y);
        }

        /// <summary>
        /// copy-paste of LineInBox()
        /// 
        /// check if line intersect AABB box
        /// 
        /// NOTE: solution ttrl#7 while is quicker it has THE SAME problem as org## and thus "chk for sneaky #1" was added.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public bool AABBcolision(FkVector2 lineStart, FkVector2 lineEnd)
        {

            lineStart = lineStart.Subtract(this.Position);
            lineEnd = lineEnd.Subtract(this.Position);

            var minMax = getMinMaxRelative();

            if (FkMath.InRange(minMax.a.X - 0.01, lineStart.X, minMax.b.X + 0.01) &&
                FkMath.InRange(minMax.a.Y - 0.01, lineStart.Y, minMax.b.Y + 0.01))
                return true;
            if (FkMath.InRange(-minMax.a.X - 0.01, lineEnd.X, minMax.b.X + 0.01) &&
                FkMath.InRange(-minMax.a.Y - 0.01, lineEnd.Y, minMax.b.Y + 0.01))
                return true;

            var vert = BoxGetVertices();

            //=====chk for sneaky #1
            if (PointOnLine(lineStart, lineEnd, vert[0]) ||//check for sneaky colinear colision (case B in unit test)
               PointOnLine(lineStart, lineEnd, vert[1]) ||
               PointOnLine(lineStart, lineEnd, vert[2]) ||
               PointOnLine(lineStart, lineEnd, vert[3]))
                return true;

            //====================================================================================ttrl#7>>

            var unitVector = lineEnd.Subtract(lineStart);
            var unitVectorLength = unitVector.Length();
            if (unitVectorLength == 0)
                return true;
            unitVector = unitVector.Multiply(1 / unitVectorLength);

            unitVector = new FkVector2(
                (unitVector.X != 0) ? (1 / unitVector.X) : (0),
                (unitVector.Y != 0) ? (1 / unitVector.Y) : (0));

            var min = minMax.a;
            min = min.Subtract(lineStart).Multiply(unitVector);
            var max = minMax.b;
            max = max.Subtract(lineStart).Multiply(unitVector);

            var tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            var tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
            if (tmax < 0 || tmin > tmax)
                return false;

            var t = (tmin < 0) ? tmax : tmin;
            return t > 0 && t * t < lineEnd.Subtract(lineStart).LengthSquared();

            //====================================================================================org##>>
            /* too expensive 
            return
                LineOnLine(lineStart, lineEnd, vert[0], vert[1]) ||
                LineOnLine(lineStart, lineEnd, vert[1], vert[2]) ||
                LineOnLine(lineStart, lineEnd, vert[2], vert[3]) ||
                LineOnLine(lineStart, lineEnd, vert[3], vert[0]);
            */
        }

        public double GetInertiaTensor(double mass) {
            throw new NotImplementedException();
        }

        //========================================================================================================================InsersectionDetector 2D

        /// <summary>
        /// check if given point is on given line
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool PointOnLine(FkVector2 lineStart, FkVector2 lineEnd, FkVector2 point)
        {
            lineEnd = lineEnd.Subtract(lineStart);
            point = point.Subtract(lineStart);
            //now linestart is 0,0  and we only care about those 2

            var lineLength = lineEnd.Length();//get length of line
            if (lineLength < 0.01)
                return false;//if line too short return false

            lineEnd = lineEnd.Multiply(1 / lineLength);//get direction vector of line

            var pointLength = point.Length();//get length of point
            if (pointLength < 0.01)
                return true;
            point = point.Multiply(1 / pointLength);//get direction vector of point

            return lineEnd.EqualWIthError(point) && (pointLength <= (lineLength + 0.01));
        }

        /// <summary>
        /// checks if 2 lines colide
        /// https://stackoverflow.com/questions/3838329/how-can-i-check-if-two-segments-intersect
        /// 
        /// will return TRUE IF:
        /// -lines intersect
        /// -one line end in on another LINE
        /// 
        /// will return FALSE IF:
        /// -one line ending touches another line ending
        /// -lines DO NOT intersect
        /// 
        /// warry: colinear solution.
        /// </summary>
        /// <param name="aStart"></param>
        /// <param name="aEnd"></param>
        /// <param name="bStart"></param>
        /// <param name="bEnd"></param>
        /// <returns></returns>
        public static bool LineOnLine(FkVector2 aStart, FkVector2 aEnd, FkVector2 bStart, FkVector2 bEnd) {
            return 
                LineOnLineCCW(aStart, bStart, bEnd) != LineOnLineCCW(aEnd, bStart, bEnd) &&
                LineOnLineCCW(aStart, aEnd, bStart) != LineOnLineCCW(aStart, aEnd, bEnd);
        }
        private static bool LineOnLineCCW(FkVector2 a, FkVector2 b, FkVector2 c){
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
        }

        //========================================================================================================================render functionality (for RAL)

    }
}
