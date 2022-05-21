using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics.Primitives
{
    internal class Primitive
    {
        //these variables are responsible for positioning of object in THIS simulation frame
        public FkVector2 Position { get; set; }
        public Double Rotation { get; set; }
        public FkMatrix2 RotationMatrix { get; set; }
        //these variables are responsible for storing MOVMENT of object in between THIS frame and NEXT frame
        //(i.e after update they sould be already up to date)
        public FkVector2 Velocity { get; set; }
        public Double Angular { get; set; }
        //these variables are responsible for DESCRIPTION of object (what is it, what shape it has, does it move)
        public FkVector2 Scale { get; set; }//IF IsBox==false then we ONLY use X from vector as circle RADIUS
        public FkMatrix2 ScaleMatrix { get; set; }
        public bool IsBox { get; set; }//in simulation we only got boxes(that can be streched using Scale) and circles
        public bool IsStatic { get; set; }//is the obect static (used as map boudary or obstacle)




    }
}
