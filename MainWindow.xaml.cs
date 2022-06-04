using Mag.Physics;
using Mag.Physics.Primitives;
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
using System.Windows.Threading;

namespace Mag
{
    /// <summary>
    /// This class holds code for rendering and basic calls for rest of code
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer clock;
        public MainWindow()
        {
            InitializeComponent();
            /*
            clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromMilliseconds(FkPhysicsEngine.dt * 1000);
            clock.Tick += RenderFrame;

            var A = new Primitive(
               Position: new FkVector2(400, 400),
               Rotation: 45,
               Velocity: new FkVector2(100, 100),
               Angular: 45,
               Scale: new FkVector2(100, 100),
               IsBox: true,
               IsStatic: false);

            FkPhysicsEngine.primitives.Add(A);
            //clock.Start();

            RenderFrame(null,null);*/
        }

        void RenderFrame(object? sender, EventArgs e)
        {
            FkPhysicsEngine.Update();
            var objects = FkPhysicsEngine.primitives;

            MyCanvas.Children.Clear();

            foreach (var obj in objects) {
                //C:\Users\rajze\Desktop\Mag\Mag\Render\Box.png
                //C:\Users\rajze\Desktop\Mag\Mag\Render\Circle.png
                //                            @"/Icons/Arrow.png"
                Image image = null;
                if (obj.IsBox)
                    image = new Image()
                    {
                        Source = new BitmapImage(new Uri(@"\Render\Box.png", UriKind.Relative)),
                        Height = obj.Scale.Y,
                        Width = obj.Scale.X
                    };
                else
                    image = new Image()
                    {
                        Source = new BitmapImage(new Uri(@"\Render\Circle.png", UriKind.Relative)),
                        Height = obj.Scale.Y,
                        Width = obj.Scale.X
                    };
                MyCanvas.Children.Add(image);

                Canvas.SetLeft(image, obj.Position.X);
                Canvas.SetTop(image, obj.Position.Y);

                image.RenderTransform = new RotateTransform(obj.Rotation);
            }
        }
    }
}
