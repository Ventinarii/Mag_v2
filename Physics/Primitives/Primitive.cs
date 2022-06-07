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
        public double repelForce = 10000;
        public double RestitutionFactor = 1;
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
        public readonly bool ApplyGenerators = true;//do we skip this objects with generators?
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
        /// <param name="ApplyGenerators">Is iterated by generators. can't be changed</param>
        /// <param name="Mass">virtual mass for momentum. can't be changed</param>
        /// <param name="RestitutionFactor">how "bouncy" object is. values <0..1>. can't be changed</param>
        /// <param name="repelForce">in case of verlap, how strong do we repel?. can't be changed</param>
        public Primitive(
            FkVector2 Position = null,
            double Rotation = 0,
            FkVector2 Velocity = null,
            double Angular = 0,
            FkVector2 Scale = null,
            bool IsBox = false,
            bool IsStatic = false,
            bool ApplyGenerators = true,
            double Mass = 10,
            double RestitutionFactor = 0.9d,
            double repelForce = 10000) {
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
            this.ApplyGenerators = ApplyGenerators;
            this.Mass = Mass;
            this.RestitutionFactor = RestitutionFactor;
            this.repelForce = repelForce;
        }
        //========================================================================================================================circle functionality
        /// <summary>
        /// check if line intersect circle
        /// </summary>
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
        /// -rotates all points (as vectors) from center of mass by angle this.Rotation
        /// </summary>
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
        /// <exception cref="InvalidOperationException"></exception>
        public bool PointInBox(FkVector2 point) {
            if (!this.IsBox)
                throw new InvalidOperationException("this is circle, not box");
            point = point.Subtract(this.Position);//now box center is point of reference
            point = new FkMatrix2(-this.Rotation).Rotate(point);//now box is aligned with axes
            return FkMath.InRange(-Scale.X - 0.01, point.X, Scale.X + 0.01) &&
                   FkMath.InRange(-Scale.Y - 0.01, point.Y, Scale.Y + 0.01);
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
        //========================================================================================================================physics functionality
        /// <summary>
        /// checks if bouding box of this promitive is in collision with bouding box of other primitive.
        /// </summary>
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
        //========================================================================================================================render functionality (for RAL)
        /// <summary>
        /// returns vertices for render. Number of circle vertices can be adjusted
        /// </summary>
        public FkVector2[] RALgetRenderVerts() { 
            if(IsBox)
                return BoxGetVerticesRotated();
            var NumberOfVerticecs = 16;
            var step = new FkMatrix2(360d / NumberOfVerticecs);
            FkVector2[] arr = new FkVector2[NumberOfVerticecs];
            arr[0] = new FkVector2(Scale.X, 0);
            for (int i = 1; i < arr.Length; i++)
                arr[i] = step.Rotate(arr[i - 1]);
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
        public CollisionResult(FkVector2 Normal = null, FkVector2 Contact = null, double Depth = -1, bool Hit = false)
        {
            this.Normal = Normal.Normalize();
            this.Contact = Contact;
            this.Depth = Depth;
            this.Hit = Hit;
        }
        public readonly FkVector2 Normal;
        public readonly FkVector2 Contact;
        public readonly double Depth;
        public readonly bool Hit;
        public static CollisionResult CircleVsCircle(Primitive circleA, Primitive circleB) {
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
            var vert = box.BoxGetVerticesRotated();
            if (!(
                box.PointInBox(circle.Position) ||
                circle.LineInCircle(vert[0], vert[1]) ||//right wall
                circle.LineInCircle(vert[1], vert[2]) ||//bottom
                circle.LineInCircle(vert[2], vert[3]) ||//left wall
                circle.LineInCircle(vert[3], vert[0])))// top
                return new CollisionResult();

            var mat = new FkMatrix2(box.Rotation);

            var relativePos = circle.Position.Subtract(box.Position);
            var relativeUnRotated = mat.UnRotate(relativePos);

            List<FkVector2> vectors = new List<FkVector2>();

            Double XL = -box.Scale.X;
            Double XR = -XL;
            Double YT = box.Scale.Y;
            Double YB = -YT;

            var x = 0d;
            var y = 0d;

            //X and Y are given
            //r^2=(x-X)^2+(y-Y)^2

            var r2 = FkMath.Pow2(circle.Scale.X);
            var X = relativeUnRotated.X;
            var Y = relativeUnRotated.Y;

            y = YT;
            var A = r2 - FkMath.Pow2(y - Y);
            if (0 <= A) {
                A = Math.Sqrt(A);
                vectors.Add(new FkVector2(X + A, y));
                if (A != 0)
                {
                    vectors.Add(new FkVector2(X - A, y));
                }
            }
            y = YB;
            A = r2 - FkMath.Pow2(y - Y);
            if (0 <= A)
            {
                A = Math.Sqrt(A);
                vectors.Add(new FkVector2(X - A, y));
                if(A!=0)
                { 
                    vectors.Add(new FkVector2(X + A, y));
                }
            }
            //===========================================
            x = XL;
            A = r2 - FkMath.Pow2(x - X);
            if (0 <= A)
            {
                A = Math.Sqrt(A);
                vectors.Add(new FkVector2(x, Y - A));
                if (A != 0)
                {
                    vectors.Add(new FkVector2(x, Y + A));
                }
            }
            x = XR;
            A = r2 - FkMath.Pow2(x - X);
            if (0 <= A)
            {
                A = Math.Sqrt(A);
                
                vectors.Add(new FkVector2(x, Y - A));
                if (A != 0)
                {
                    vectors.Add(new FkVector2(x, Y + A));
                }
            }


            vectors = vectors.Where(v=>FkMath.InRange(XL-0.01,v.X,XR+0.01)&& FkMath.InRange(YB - 0.01, v.Y, YT+0.01)).ToList();
            if (vectors.Count() == 0)
                return new CollisionResult();
            FkVector2 avg = null;
            if (vectors.Count() == 1)
                avg = vectors[0];
            else
                avg = vectors[0].Add(vectors[1]).Multiply(0.5d);//consider 2 points of contact
            avg = mat.Rotate(avg).Add(box.Position);
            var normal = avg.Subtract(circle.Position).Normalize();
            return new CollisionResult(normal, avg, avg.Subtract(circle.Position).Length()/2,true);
        }
    }

    public class ColisionResolution {
        //skip angular changes //apply fircion for angular
        public static void CircleVsCircle(Primitive circleA, Primitive circleB, bool FirstCall) {
            if (circleA.IsBox || circleB.IsBox)
                throw new InvalidOperationException("Not a circle");
            if (!circleA.AABBvsAABB(circleB))
                return;//no colision
            if (FkMath.Pow2(circleA.Scale.X + circleB.Scale.X) < circleA.Position.Subtract(circleB.Position).LengthSquared())
                return;//no colision
            var bounce = circleA.IsStatic || circleA.Mass == 0 || circleB.IsStatic || circleB.Mass == 0;
            if (bounce) CircleVsCircleStatic(circleA, circleB , FirstCall);
            if (bounce) return;//bounce
            var man = CollisionResult.CircleVsCircle(circleA, circleB);
            if (!man.Hit)
                return;//no contact
            //===================================================exchange of impulses>>
            var realativePositionOfB = circleB.Position.Subtract(circleA.Position);
            var relativeVelocity = circleB.Velocity.Subtract(circleA.Velocity);
            var normalRelativeVelocity = relativeVelocity.Normalize();
            var impactNormal = -man.Normal.DotProdcut(normalRelativeVelocity);
            if (0 < impactNormal)  { 
                var minRestitution = Math.Min(circleA.RestitutionFactor, circleB.RestitutionFactor);
                var num = (-(1 + minRestitution) * relativeVelocity.DotProdcut(man.Normal));
                num = num / (circleA.InvertedMass + circleB.InvertedMass);
                var impulse = man.Normal.Multiply(num);
                circleA.Velocity = circleA.Velocity.Add(impulse.Multiply(-circleA.InvertedMass));
                circleB.Velocity = circleB.Velocity.Add(impulse.Multiply( circleB.InvertedMass));
            }
            if (FirstCall)
            {
                var forceAvg = (circleA.repelForce + circleB.repelForce) / 2;
                var fractionOfForce = 1 - (realativePositionOfB.LengthSquared() / FkMath.Pow2(circleA.Scale.X + circleB.Scale.X));
                circleA.forceResult = circleA.forceResult.Add(man.Normal.Multiply(fractionOfForce * forceAvg * -1));
                circleB.forceResult = circleB.forceResult.Add(man.Normal.Multiply(fractionOfForce * forceAvg));
                var x = 2;
            }
        }
        private static void CircleVsCircleStatic(Primitive circleA, Primitive circleB, bool FirstCall) {
            if ((circleA.IsStatic || circleA.Mass == 0) && (circleB.IsStatic || circleB.Mass == 0))
                return; //deadlock
            if (circleA.IsStatic || circleA.Mass == 0){
                var circle = circleA;
                circleA = circleB;
                circleB = circle;
            }
            var man = CollisionResult.CircleVsCircle(circleA, circleB);
            if (!man.Hit)
                return;//no contact
            //===================================================Bounce>>
            var realativePositionOfB = circleB.Position.Subtract(circleA.Position);
            var relativeVelocity = circleB.Velocity.Subtract(circleA.Velocity);
            var normalRelativeVelocity = relativeVelocity.Normalize();
            var impactNormal = -man.Normal.DotProdcut(normalRelativeVelocity);
            if (0 < impactNormal)
            {
                var minRestitution = Math.Min(circleA.RestitutionFactor, circleB.RestitutionFactor);
                var num = (-(1 + minRestitution) * relativeVelocity.DotProdcut(man.Normal));
                num = num / (circleA.InvertedMass);
                var impulse = man.Normal.Multiply(num);
                circleA.Velocity = circleA.Velocity.Add(impulse.Multiply(-circleA.InvertedMass));
            }
            if (FirstCall)
            {
                var forceAvg = (circleA.repelForce + circleB.repelForce) / 2;
                var fractionOfForce = 1 - (realativePositionOfB.LengthSquared() / FkMath.Pow2(circleA.Scale.X + circleB.Scale.X));
                circleA.forceResult = circleA.forceResult.Add(man.Normal.Multiply(fractionOfForce * forceAvg * -1));
                var x = 2;
            }
        }
        public static void CircleVsBox(Primitive circle, Primitive box, bool FirstCall)
        {
            if (circle.IsBox || !box.IsBox)
                throw new InvalidOperationException("mess");
            if (!circle.AABBvsAABB(box))
                return;//no colision
            var bounce = circle.IsStatic || circle.Mass == 0;
            if (bounce) 
                return;//bounce
            bounce = box.IsStatic || box.Mass == 0;
            if (bounce) 
                CircleVsBoxStatic(circle, box, FirstCall);
            if (bounce) 
                return;//bounce
            var man = CollisionResult.CircleVsBox(circle, box);
            if (!man.Hit)
                return;//no contact
            //===================================================exchange of impulses>>
        }
        private static void CircleVsBoxStatic(Primitive circle, Primitive box, bool FirstCall)
        {
            if ((circle.IsStatic || circle.Mass == 0) && (box.IsStatic || box.Mass == 0))
                return; //deadlock
            var vert = box.BoxGetVerticesRotated();
            if (!(
                box.PointInBox(circle.Position) ||
                circle.LineInCircle(vert[0], vert[1]) ||//right wall
                circle.LineInCircle(vert[1], vert[2]) ||//bottom
                circle.LineInCircle(vert[2], vert[3]) ||//left wall
                circle.LineInCircle(vert[3], vert[0])))// top
                return;
            var man = CollisionResult.CircleVsBox(circle, box);
            if (!man.Hit)
                return;//no contact
            //===================================================Bounce>>
            var realativePositionOfB = box.Position.Subtract(circle.Position);
            var relativeVelocity = box.Velocity.Subtract(circle.Velocity);
            var normalRelativeVelocity = relativeVelocity.Normalize();
            var impactNormal = -man.Normal.DotProdcut(normalRelativeVelocity);
            if (0 < impactNormal)
            {
                var minRestitution = Math.Min(circle.RestitutionFactor, box.RestitutionFactor);
                var num = (-(1 + minRestitution) * relativeVelocity.DotProdcut(man.Normal));
                num = num / (circle.InvertedMass);
                var impulse = man.Normal.Multiply(num);
                circle.Velocity = circle.Velocity.Add(impulse.Multiply(-circle.InvertedMass));
            }
        }
    }

}
