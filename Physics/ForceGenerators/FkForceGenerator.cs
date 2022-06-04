using Mag.Physics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics.ForceGenerators
{
    public interface FkForceGenerator
    {
        public void UpdateForce(Primitive ob, double dt, int frame);
    }

    public class FkGravity : FkForceGenerator //this generator produces constant gravity
    {
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            throw new NotImplementedException();
        }
    }

    public class FkGravityRotation : FkForceGenerator //this generator rotates fixed angle / frame
    {
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            throw new NotImplementedException();
        }
    }

    public class FkFriction : FkForceGenerator //this generator removes exxes energy from system
    {
        void FkForceGenerator.UpdateForce(Primitive ob, double dt, int frame)
        {
            throw new NotImplementedException();
        }
    }
}
