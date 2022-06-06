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
        /// >>==>>this is representation of data table in database engine<<==<<
        /// </summary>
        public static readonly List<Primitive> primitives = new List<Primitive>();
        public static int frame = 0;
        public static void Update() {
            //add forces from generators]
            primitives.Where(x => !x.IsStatic && x.ApplyGenerators).ToList().ForEach(ob => {
                generatores.ForEach(generator => generator.UpdateForce(ob, frame));
            });
            //add impluses from colisions /// it is in this order to create potential for static force ditribution (we run impulses only)            
            //apply space partition below
            var circles = primitives.Where(x => !x.IsBox).ToList();
            var sizeC = circles.Count();
            var boxes = primitives.Where(x => x.IsBox).ToList();
            var sizeB = boxes.Count();
            //resolve colisions  
            for (int iterations = 0; iterations <= 2; iterations++) { 
                for (int i = 0; i < sizeC; i++)
                    for (int x = i + 1; x < sizeC; x++)
                        ColisionResolution.CircleVsCircle(circles[i], circles[x], iterations==0);
                for (int i = 0; i < sizeB; i++)
                    for (int x = i + 1; x < sizeB; x++)
                        ColisionResolution.BoxVsBox(boxes[i], boxes[x], iterations == 0);
                for (int i = 0; i < sizeC; i++)
                    for (int x = 0; x < sizeB; x++)
                        ColisionResolution.CircleVsBox(circles[i], boxes[x], iterations == 0);
            }
            //apply space partition above                      
            primitives.ForEach(ob => {
                ob.Update(dt);
            });
            //apply forces and impulses, then clean            
            frame++;
            //note we finihed frame
        }
    }
}
