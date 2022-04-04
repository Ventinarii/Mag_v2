using Mag.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Render
{
    /// <summary>
    /// Render Abstract Layer - this interface is used to indicate what 2 other moudes (Physics, DBI) can show in window.
    /// </summary>
    internal interface FkRAL
    {
        /// <summary>
        /// this function is called to render ON THIS THREAD single frame
        /// </summary>
        public void RenderFrame();
        /// <summary>
        /// this function is called to START ANIMATOR THREAD that will render ALL given frames
        /// </summary>
        public void RenderAllFrames();
    }
}
