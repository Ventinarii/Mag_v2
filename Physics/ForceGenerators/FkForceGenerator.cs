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
        public void UpdateForce(Primitive ob, int frame);
    }

    public class FkGravity : FkForceGenerator //this generator produces constant gravity
    {
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            ob.Velocity = ob.Velocity.Add(FkGeneratorHolder.G1.Multiply(FkGeneratorHolder.dt/ 1000));
        }
    }

    public class FkGravityRotation : FkForceGenerator //this generator rotates fixed angle / frame
    {
        private int frame = -1;
        private FkVector2 G1local = FkGeneratorHolder.G1;
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            //assume: 30 frames / sec
            //full rotation 1 minute (60 sec)
            //full rotation on frame 1800
            if (this.frame != frame) {
                double rotationFraction = frame / 1800;
                double rotationAngle = 360 * rotationFraction;
                G1local = new FkMatrix2(rotationAngle).Rotate(FkGeneratorHolder.G1);
            }
            ob.Velocity = ob.Velocity.Add(G1local.Multiply(FkGeneratorHolder.dt / 1000));
        }
    }

    public class FkFriction : FkForceGenerator //this generator removes exxes energy from system
    {
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            ob.VelocityFriction = ob.Velocity.Multiply(FkGeneratorHolder.frictionFractionLinear*FkGeneratorHolder.dt);
            ob.AngularFriction = ob.Angular*FkGeneratorHolder.frictionFractionLinear*FkGeneratorHolder.dt;
        }
    }

    /// <summary>
    /// NOTE: EACH body that IS SIMULATED is pending edit EACH frame by EACH generator in order they were added.
    /// </summary>
    public static class FkGeneratorHolder {
        public static readonly List<FkForceGenerator> generatores = new List<FkForceGenerator>();

        public static void updateForces(List<Primitive> list, int frame) {
            list.AsParallel().ForAll(ob => {
                generatores.ForEach(generator => generator.UpdateForce(ob,frame));
            });
        }

        public static readonly FkVector2 G1 = new FkVector2(0, -100);
        public static readonly double frictionFractionLinear = -1;
        public static readonly double frictionFractionAngular = -1;
        public static readonly double dt = 1 / 30;
    }
}
