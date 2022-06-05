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
            //add forces from generators
            primitives.ForEach(ob => {
                generatores.ForEach(generator => generator.UpdateForce(ob, frame));
            });

            //add forces from colisions /// it is in this order to create potential for static force ditribution (we run impulses only) 
            var circles = 
                from prim in primitives.AsParallel()
                where !prim.IsBox 
                select prim;

            var boxes =
                from prim in primitives.AsParallel()
                where prim.IsBox
                select prim;

            var boxesVsCircles = 1;

            //apply forces anc clean
            primitives.ForEach(ob => {
                ob.Update(dt);
            });

            //note we finihed frame
            frame++;
        }
    }
}
