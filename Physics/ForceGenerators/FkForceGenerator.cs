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
        /// <param name="dt">fraction of second</param>
        /// <param name="frame"></param>
        public void UpdateForce(Primitive ob, int frame);
    }
    public class FkGravity : FkForceGenerator //this generator produces constant gravity
    {
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            ob.forceResult = ob.forceResult.Add(FkPhysicsEngine.G1.Multiply(ob.Mass));
        }
    }
    public class FkGravityRotation : FkForceGenerator //this generator rotates fixed angle / frame
    {
        private int frame = -1;
        private FkVector2 G1local = FkPhysicsEngine.G1;
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            //assume: 30 frames / sec
            //full rotation 1 minute (60 sec)
            //full rotation on frame 1800
            if (this.frame != frame) {
                this.frame = frame;//update only when needed
                double framesASecond = 1/ FkPhysicsEngine.dt;
                var framesinMinute = framesASecond * 60;//<<<< ADJUST ROTATION SPEED HERE. SET ROTATION TIME IN SECONDS. 

                double rotationAngle = 360 * (frame / framesinMinute);
                G1local = new FkMatrix2(rotationAngle).Rotate(FkPhysicsEngine.G1);//multiple rotation possible
            }
            ob.forceResult = ob.forceResult.Add(G1local.Multiply(ob.Mass));
        }
    }
    public class FkFriction : FkForceGenerator //this generator removes exxes energy from system
    {
        void FkForceGenerator.UpdateForce(Primitive ob, int frame)
        {
            ob.forceResult = ob.forceResult.Add(
                ob.Velocity.Multiply(FkPhysicsEngine.frictionFractionLinear)
                );
            ob.AngularForceResult += ob.Angular * FkPhysicsEngine.frictionFractionAngular;
        }
    }    
}
