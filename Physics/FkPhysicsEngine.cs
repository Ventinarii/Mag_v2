using Mag.Physics.ForceGenerators;
using Mag.Physics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    public class FkPhysicsEngine
    {
        /// <summary>
        /// NOTE: EACH body that IS SIMULATED is pending edit EACH frame by EACH generator in order they were added.
        /// >>==>>this is representation of data table in database engine<<==<<
        /// </summary>
        public static readonly List<FkForceGenerator> generatores = new List<FkForceGenerator>();

        //define physics constants
        public static readonly FkVector2 G1 = new FkVector2(0d, -100d);
        public static readonly double frictionFractionLinear = -1;
        public static readonly double frictionFractionAngular = -1;
        public static readonly double dt = (1d / 30d);

        //========================================================================================================================

        /// <summary>
        /// 
        /// >>==>>this is representation of data table in database engine<<==<<
        /// </summary>
        public static readonly List<Primitive> primitives = new List<Primitive>();
        public static int frame = 0;

        public static void Update() {
            //todo add iterative colision resolution linear loop 
            primitives.ForEach(ob => {
                generatores.ForEach(generator => generator.UpdateForce(ob, frame));
                ob.Update(dt);
            });
            frame++;
        }
    }
}
