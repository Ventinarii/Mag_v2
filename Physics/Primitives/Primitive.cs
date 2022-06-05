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

        //============================================== mass mess
        public FkVector2 forceResult = new FkVector2(0, 0);
        public double AngularForceResult = 0;
        public double Mass {
            get { return massTh; }
            set {
                massTh = value;
                if (massTh != 0)
                    invertedMassTh = 1 / massTh;
            } }
        private double massTh = 10;
        public double InvertedMass {
            get { return invertedMassTh; } }
        private double invertedMassTh = 1d / 10d;
        //==============================================
        public void Update(double dt) {
            if (massTh != 0 && !IsStatic)
            {
                //linear vel
                Velocity =
                    Velocity.Add(
                        forceResult//get forces
                        .Multiply(invertedMassTh)//get acceleration (gravity force must be adjust dynamicaly to mass)
                        .Multiply(dt)//multiply change by time fraction
                    );

                Position = Position.Add(Velocity.Multiply(dt));//change POSITION by VELOCITY * DeltaTIme

                Angular += AngularForceResult * invertedMassTh * dt;
                Rotation += Angular * dt;

                // <<??== write here custom transform on demand
            }
            forceResult = new FkVector2(0, 0);
            AngularForceResult = 0;
        }

        //these variables are responsible for DESCRIPTION of object (what is it, what shape it has, does it move)

        public readonly FkVector2 Scale;//IF IsBox==false then we ONLY use X from vector as circle RADIUS
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
        public RayCastResult RayCastCircle(FkVector2 origin, FkVector2 direction) {
            origin = origin.Subtract(this.Position);//<< relative computing
            var fail = new RayCastResult(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);
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
            return new RayCastResult(pointOfHitOff, normalOfHit, t, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CrircleVsCircle(Primitive circleA, Primitive circleB) {
            if (circleA.IsBox || circleB.IsBox)
                throw new InvalidOperationException("one of these is box");

            return circleA.Position.Subtract(circleB.Position).LengthSquared() <= FkMath.Pow2(circleA.Scale.X + circleB.Scale.X);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CircleVsBox(Primitive circle, Primitive box) {
            if (circle.IsBox || !box.IsBox)
                throw new InvalidOperationException("miss match");

            var vert = box.BoxGetVerticesRotated();
            return
                box.PointInBox(circle.Position) ||
                circle.LineInCircle(vert[0], vert[1]) ||
                circle.LineInCircle(vert[1], vert[2]) ||
                circle.LineInCircle(vert[2], vert[3]) ||
                circle.LineInCircle(vert[3], vert[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="AABB"></param>
        /// <returns></returns>
        public static bool CircleVsAABB(Primitive circle, Primitive AABB)
        {
            if (circle.IsBox)
                throw new InvalidOperationException("circle is box");

            var minMax = AABB.getMinMaxRelative();
            var swap = new FkVector2(minMax.b.X, -minMax.b.Y);
            FkVector2[] vert = { minMax.b, swap, minMax.a, swap.Multiply(-1) };
            for(int i = 0; i < vert.Length; i++)
                vert[i] = vert[i].Add(AABB.Position);
            return
                AABB.AABBvsPoint(circle.Position) ||
                circle.LineInCircle(vert[0], vert[1]) ||
                circle.LineInCircle(vert[1], vert[2]) ||
                circle.LineInCircle(vert[2], vert[3]) ||
                circle.LineInCircle(vert[3], vert[0]);
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
            var vert = BoxGetVerticesRotated();
            return
                PointInBox(lineStart) || PointInBox(lineEnd) ||
                LineOnLine(lineStart, lineEnd, vert[0], vert[1]) ||
                LineOnLine(lineStart, lineEnd, vert[1], vert[2]) ||
                LineOnLine(lineStart, lineEnd, vert[2], vert[3]) ||
                LineOnLine(lineStart, lineEnd, vert[3], vert[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <returns>point of hit; normal of hit; t; hit?</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public RayCastResult RayCastBox(FkVector2 origin, FkVector2 direction)
        {
            if (!this.IsBox)
                throw new InvalidOperationException("this is circle, not box");
            var fail = new RayCastResult(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);
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

            var point = origin.Add(direction.Multiply(t));
            var normal = origin.Subtract(point);
            normal = normal.Normalize();
            return new RayCastResult(point, normal, t, true);
        }

        /// <summary>
        /// //todo: optimize #11
        /// </summary>
        /// <param name="boxA"></param>
        /// <param name="boxB"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool BoxVsBox(Primitive boxA, Primitive boxB) {//todo: optimize #11
            if(!boxA.IsBox || !boxB.IsBox)
                throw new InvalidOperationException("box is circle");

            if (boxA.Scale.LengthSquared() < boxB.Scale.LengthSquared()) {
                //check against situation where one box is inside another. for this thing to work we detect IF box B is INSIDE box A. 
                //whithout this check situation where box A is inside box B are skipped.
                var swap = boxA;
                boxA = boxB;
                boxB = swap;
            }

            var vertB = boxB.BoxGetVerticesRotated();

            return
                boxA.LineInBox(vertB[0],vertB[1]) ||
                boxA.LineInBox(vertB[1],vertB[2]) ||
                boxA.LineInBox(vertB[2],vertB[3]) ||
                boxA.LineInBox(vertB[3],vertB[0]);
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
        /// effectivly: left bottom corner of AABB and top right corner of AABB.
        /// 
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
        public bool AABBvsAABB(Primitive other) {
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
        public bool AABBvsPoint(FkVector2 other) {
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
        public bool AABBvsLine(FkVector2 lineStart, FkVector2 lineEnd)
        {
            var minMax = getMinMax();
            var swap = new FkVector2(minMax.b.X, -minMax.b.Y);
            FkVector2[] vert = { minMax.b, swap, minMax.a, swap.Multiply(-1) };

            return
                AABBvsPoint(lineStart) || AABBvsPoint(lineEnd) ||
                LineOnLine(lineStart, lineEnd, vert[0], vert[1]) ||
                LineOnLine(lineStart, lineEnd, vert[1], vert[2]) ||
                LineOnLine(lineStart, lineEnd, vert[2], vert[3]) ||
                LineOnLine(lineStart, lineEnd, vert[3], vert[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="skipRotation">DO -=NOT=- USE THIS PARAM. it is for internal call only</param>
        /// <returns></returns>
        public RayCastResult RayCastAABB(FkVector2 origin, FkVector2 direction)
        {
            var fail = new RayCastResult(new FkVector2(0, 0), new FkVector2(0, 0), -1, false);
            var mat = new FkMatrix2(Rotation);
            if (direction.LengthSquared() == 0)
                return fail;
            direction = direction.Normalize();
            var max = getMinMaxRelative().b;

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
                var sizeI = (i == 0) ? (max.X) : (max.Y);
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

            if (t < 0)
                return fail;

            var point = origin.Add(direction.Multiply(t));
            var normal = origin.Subtract(point);
            normal = normal.Normalize();
            return new RayCastResult(point, normal, t, true);
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
        public static bool LineOnLine(FkVector2 aStart, FkVector2 aEnd, FkVector2 bStart, FkVector2 bEnd)
        {
            return
                LineOnLineCCW(aStart, bStart, bEnd) != LineOnLineCCW(aEnd, bStart, bEnd) &&
                LineOnLineCCW(aStart, aEnd, bStart) != LineOnLineCCW(aStart, aEnd, bEnd);
        }
        private static bool LineOnLineCCW(FkVector2 a, FkVector2 b, FkVector2 c)
        {
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
        }

        //========================================================================================================================render functionality (for RAL)

        public FkVector2[] RALgetRenderVerts() { 
            if(IsBox)
                return BoxGetVerticesRotated();

            var rotated = new FkMatrix2(-45).Rotate(new FkVector2(Scale.X, 0));

            FkVector2[] arr = new FkVector2[8];
            arr[0] = new FkVector2(Scale.X, 0);//tight
            arr[1] = rotated;
            arr[2] = new FkVector2(0, Scale.X);//top
            arr[3] = new FkVector2(-rotated.X, rotated.Y);
            arr[4] = arr[0].Multiply(-1);//left
            arr[5] = arr[1].Multiply(-1);
            arr[6] = arr[2].Multiply(-1);//bottom
            arr[7] = arr[3].Multiply(-1);

            var mat = new FkMatrix2(this.Rotation);

            for (int i = 0; i < arr.Length; i++)
                arr[i] = mat.Rotate(arr[i]).Add(this.Position);
            return arr;
        }

    }

    public class RayCastResult {
        public RayCastResult(FkVector2 PointOfHit, FkVector2 NormalOfHit, double t, bool Hit) { 
            this.PointOfHit = PointOfHit;
            this.NormalOfHit = NormalOfHit;
            this.t = t;
            this.Hit = Hit;
        }
        public readonly FkVector2 PointOfHit;
        public readonly FkVector2 NormalOfHit;
        public readonly double t;
        public readonly bool Hit;
    }

    public class CollisionResult {
        public static CollisionResult CircleVsCircle(Primitive circleA, Primitive circleB) {
            if (circleA.IsBox || circleB.IsBox)
                throw new InvalidOperationException("Not a circle");
            if (FkMath.Pow2(circleA.Scale.X+circleB.Scale.X)<circleA.Position.Subtract(circleB.Position).LengthSquared())
                return new CollisionResult();//if no collision
            
            var radiusSum = circleA.Scale.X+circleB.Scale.X;

            var delta = circleB.Position.Subtract(circleA.Position);
            var deltaLength = delta.Length();

            var depth = radiusSum - deltaLength;
            depth = depth * 0.5d;

            var normal = delta.Normalize();

            var contact = circleA.Position.Add(normal.Multiply(circleA.Scale.X - depth));

            return new CollisionResult(
                normal,
                contact,
                depth,
                true
                );
        }

        public static CollisionResult CircleVsBox(Primitive circle, Primitive box)
        {
            if (circle.IsBox || !box.IsBox)
                throw new InvalidOperationException("mess");


            //write stuff here

            return null;
        }

        public static CollisionResult BoxVsBox(Primitive boxA, Primitive boxB)
        {
            if (!boxA.IsBox || !boxB.IsBox)
                throw new InvalidOperationException("Not a box");

            //write stuff here

            return null;
        }

        public CollisionResult(FkVector2 Normal = null, FkVector2 Contact = null, double Depth = -1, bool Hit = false)
        {
            this.Normal = Normal.Normalize();
            this.ContactA = ContactA;
            this.ContactB = ContactB;
            this.Depth = Depth;
            this.Hit=Hit;
        }
        public readonly FkVector2 Normal;
        public readonly FkVector2 Contact;
        public readonly double Depth;
        public readonly bool Hit;
    }

    //impulse: FkVector location; FkVector direction; double force

    public class ColisionResolution { 
        
        
    }

}
