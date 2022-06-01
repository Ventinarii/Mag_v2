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
            corners[1] = new FkVector2(Scale.X,-Scale.Y);
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

        //========================================================================================================================shared functionality

        /// <summary>
        /// -returns left bottom most corner of square containing this object uses:
        /// --this.BoxGetVerticesRelativeRotated()) if is a box
        /// --this.Scale, this.Scale.Multiply(-1) if circle
        /// as a source of points to compare. 
        /// </summary>
        /// <returns>first value is minimum, second maximum</returns>
        public C2<FkVector2,FkVector2> getMinMaxRelative() {
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
                (mxSUMA.X <= delta.X) && (delta.X <= mxSUMB.X) &&
                (mxSUMA.Y <= delta.Y) && (delta.Y <= mxSUMB.Y);
        }


        public double GetInertiaTensor(double mass) { 
            throw new NotImplementedException();
        }

        //========================================================================================================================render functionality (for RAL)


    }
}
