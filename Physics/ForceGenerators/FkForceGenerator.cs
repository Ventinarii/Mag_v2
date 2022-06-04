using Mag.Physics.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics.ForceGenerators
{
    public interface FkForceGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ob"></param>
        /// <param name="dt">time in miliseconds (1/1000)</param>
        /// <param name="frame"></param>
        public void UpdateForce(Primitive ob, double dt, int frame);
    }

    public class FkGravity : FkForceGenerator //this generator produces constant gravity
    {
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            ob.Velocity = ob.Velocity.Add(FkGeneratorHolder.G1.Multiply(dt/1000));
        }
    }

    public class FkGravityRotation : FkForceGenerator //this generator rotates fixed angle / frame
    {
        private int frame = -1;
        private FkVector2 G1local = FkGeneratorHolder.G1;
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            //assume: 30 frames / sec
            //full rotation 1 minute (60 sec)
            //full rotation on frame 1800
            if (this.frame != frame) {
                double rotationFraction = frame / 1800;
                double rotationAngle = 360 * rotationFraction;
                G1local = new FkMatrix2(rotationAngle).Rotate(FkGeneratorHolder.G1);
            }
            ob.Velocity = ob.Velocity.Add(G1local.Multiply(dt / 1000));
        }
    }

    public class FkFriction : FkForceGenerator //this generator removes exxes energy from system
    {
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            ob.VelocityFriction = ob.Velocity.Multiply(FkGeneratorHolder.frictionFractionLinear);
            ob.AngularFriction = ob.Angular*FkGeneratorHolder.frictionFractionLinear;
        }
    }

    public static class FkGeneratorHolder {
        public static readonly LinkedList<FkForceGenerator> generatores = new LinkedList<FkForceGenerator>();

        public static readonly FkVector2 G1 = new FkVector2(0, -100);
        public static readonly double frictionFractionLinear = -0.1;
        public static readonly double frictionFractionAngular = -0.1;
    }
}
