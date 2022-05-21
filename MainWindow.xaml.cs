using Mag.Physics;
using Mag.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mag
{
    /// <summary>
    /// This class holds code for rendering and basic calls for rest of code
    /// </summary>
    public partial class MainWindow : Window, FkRAL
    {
        public MainWindow()
        {
            InitializeComponent();
            double d = 1;


        }

        void FkRAL.RenderAllFrames()
        {
            throw new NotImplementedException();
        }

        void FkRAL.RenderFrame()
        {
            throw new NotImplementedException();
        }
    }
}
