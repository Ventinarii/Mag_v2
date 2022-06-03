﻿using System;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <returns>point of hit; normal of hit; t; hit?</returns>
        public C4<FkVector2, FkVector2, double, bool> RayCastCircle(FkVector2 origin, FkVector2 direction) {
            origin = origin.Subtract(this.Position);//<< relative computing
            var fail = new C4<FkVector2, FkVector2, double, bool>(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);
            if (direction.LengthSquared() < 0.01) return fail;//<< drop if NaN
            direction = direction.Normalize();

            //var orginToCircle = this.Position.Subtract(orgin);
            var orginToCircle = origin.Multiply(-1);//<< relative computing
            var radius2 = Scale.X * Scale.X;
            var orginToCircle2 = orginToCircle.LengthSquared();

            // projection
            var a = orginToCircle.DotProdcut(direction);
            var bSq = orginToCircle2 - (a * a);
            if (radius2 - bSq < 0)
                return fail;

            var f = Math.Sqrt(radius2 - bSq);
            var t = a - f;
            if (orginToCircle2 < radius2)
                t = a + f;

            if (t < 0) return fail;//<< drop if imaginary

            //var pointOfHit = orgin.Add(direction.Multiply(t));
            var pointOfHit = origin.Add(direction.Multiply(t));//<< relative computing
            //var normalOfHit = pointOfHit.Subtract(this.Position).Normalize();
            var normalOfHit = pointOfHit.Multiply(-1).Normalize();//<< relative computing
            var pointOfHitOff = pointOfHit.Add(this.Position);//<< relative computing
            return new C4<FkVector2, FkVector2, double, bool>(pointOfHitOff, normalOfHit, t, true);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <returns>point of hit; normal of hit; t; hit?</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public C4<FkVector2, FkVector2, double, bool> RayCastBox(FkVector2 origin, FkVector2 direction)
        {
            if (!this.IsBox)
                throw new InvalidOperationException("this is circle, not box");
            var fail = new C4<FkVector2, FkVector2, double, bool>(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);
            var mat = new FkMatrix2(Rotation);
            if (direction.LengthSquared() == 0)
                return fail;
            direction = direction.Normalize();

            var xAxis = new FkVector2(1, 0);
            var yAxis = new FkVector2(0, 1);
            xAxis = mat.UnRotate(xAxis);
            yAxis = mat.UnRotate(yAxis);

            var p = this.Position.Subtract(origin);
            var f = new FkVector2(xAxis.DotProdcut(direction), yAxis.DotProdcut(direction));

            var e = new FkVector2(xAxis.DotProdcut(p), yAxis.DotProdcut(p));

            double[] arr = { 0, 0, 0, 0 };
            for (int i = 0; i < 2; i++)
            {
                var fi = (i == 0) ? (f.X) : (f.Y);
                var ei = (i == 0) ? (e.X) : (e.Y);
                var sizeI = (i == 0) ? (Scale.X) : (Scale.Y);
                if (FkMath.EqualWIthError(fi, 0))
                {
                    if (-ei - sizeI > 0 || -ei + sizeI < 0)
                    {
                        return fail;
                    }
                    if (i == 0)
                        f = new FkVector2(0.00001, f.Y);
                    else
                        f = new FkVector2(f.X, 0.00001);
                }
                arr[i * 2 + 0] = (ei + sizeI) / fi;
                arr[i * 2 + 1] = (ei - sizeI) / fi;
            }

            var tmin = Math.Max(Math.Min(arr[0], arr[1]), Math.Min(arr[2], arr[3]));
            var tmax = Math.Min(Math.Max(arr[0], arr[1]), Math.Max(arr[2], arr[3]));

            var t = (tmin < 0f) ? tmax : tmin;

            if (t<0)
                return fail;

            var point = new FkVector2(origin).Add(new FkVector2(direction).Multiply(t));
            var normal = new FkVector2(origin).Subtract(point);
            normal = normal.Normalize();
            return new C4<FkVector2, FkVector2, double, bool>(point, normal, t, true);
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
            if (AABBcolision(lineStart) || AABBcolision(lineEnd))
            {
                return true;
            }

            var unitVector = lineEnd.Subtract(lineStart);
            if (unitVector.LengthSquared() == 0)
                return false;
            unitVector = unitVector.Normalize();
            unitVector = new FkVector2((unitVector.X != 0) ? 1 / unitVector.X : 0,
                                       (unitVector.Y != 0) ? 1/ unitVector.Y : 0);
            var minMax = getMinMaxRelative();
            FkVector2 min = minMax.a;
            min = min.Subtract(lineStart).Multiply(unitVector);
            FkVector2 max = minMax.b;
            max = max.Subtract(lineStart).Multiply(unitVector);

            var tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            var tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
            if (tmax < 0 || tmin > tmax)
            {
                return false;
            }

            var t = (tmin < 0) ? tmax : tmin;
            return t > 0 && t * t < lineEnd.Subtract(lineStart).LengthSquared();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="skipRotation">DO -=NOT=- USE THIS PARAM. it is for internal call only</param>
        /// <returns></returns>
        public C4<FkVector2, FkVector2, double, bool> RayCastAABB(FkVector2 origin, FkVector2 direction)
        {
            var fail = new C4<FkVector2, FkVector2, double, bool>(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);

            var unitVector = direction;
            if (unitVector.LengthSquared() == 0)
                return fail;
            unitVector = unitVector.Normalize();
            unitVector = new FkVector2((unitVector.X != 0) ? 1 / unitVector.X : 0,
                                       (unitVector.Y != 0) ? 1 / unitVector.Y : 0);
            var minMax = getMinMaxRelative();
            FkVector2 min = minMax.a;
            min = min.Subtract(origin).Multiply(unitVector);
            FkVector2 max = minMax.b;
            max = max.Subtract(origin).Multiply(unitVector);

            var tmin = Math.Max(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            var tmax = Math.Min(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
            if (tmax < 0 || tmin > tmax)
            {
                return fail;
            }

            var t = (tmin < 0) ? tmax : tmin;

            if (t < 0)
                return fail;

            var hitpos = origin.Add(direction.Multiply(t));
            var noraml = origin.Subtract(hitpos);
            return new C4<FkVector2, FkVector2, double, bool>(hitpos, noraml, t, true);
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

        //========================================================================================================================render functionality (for RAL)

    }
}
