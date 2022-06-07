using Mag.Physics;
using Mag.Physics.ForceGenerators;
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
    //Disclaimer: Fk prefix stands for ForeinKey from databases and is running joke in my code.
    //it is used to indicate that given code and / or function are forein to given location / environment.
    //by intention it is NOT asociated with ANY more than that.

    /// <summary>
    /// This class holds code for rendering and basic calls for rest of code
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer clock;
        public MainWindow()
        {
            InitializeComponent();
            clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromMilliseconds(FkPhysicsEngine.dt * 1);
            clock.Tick += RenderFrame;
            //here create content of world
            Primitive A = null;
            if (true)
            { //BORDER
                A = new Primitive(
                   Position: new FkVector2(500, 10),
                   Rotation: 0,
                   Velocity: new FkVector2(0, 0),
                   Angular: 0,
                   Scale: new FkVector2(500, 10),
                   IsBox: true,
                   IsStatic: true,
                   ApplyGenerators: false,
                   Mass: 10,
                   RestitutionFactor: 0.9);
                FkPhysicsEngine.primitives.Add(A);
                A = new Primitive(
                   Position: new FkVector2(500, 990),
                   Rotation: 0,
                   Velocity: new FkVector2(0, 0),
                   Angular: 0,
                   Scale: new FkVector2(500, 10),
                   IsBox: true,
                   IsStatic: true,
                   ApplyGenerators: false,
                   Mass: 10,
                   RestitutionFactor: 0.9);
                FkPhysicsEngine.primitives.Add(A);
                A = new Primitive(
                   Position: new FkVector2(10, 500),
                   Rotation: 0,
                   Velocity: new FkVector2(0, 0),
                   Angular: 0,
                   Scale: new FkVector2(10, 480),
                   IsBox: true,
                   IsStatic: true,
                   ApplyGenerators: false,
                   Mass: 10,
                   RestitutionFactor: 0.9);
                FkPhysicsEngine.primitives.Add(A);
                A = new Primitive(
                   Position: new FkVector2(990, 500),
                   Rotation: 0,
                   Velocity: new FkVector2(0, 0),
                   Angular: 0,
                   Scale: new FkVector2(10, 480),
                   IsBox: true,
                   IsStatic: true,
                   ApplyGenerators: false,
                   Mass: 10,
                   RestitutionFactor: 0.9);
                FkPhysicsEngine.primitives.Add(A);
            }
            for(int grav = 0; grav<=4; grav++)
                for (int size = 0; size <= 10; size++)
                {
                    //Console.WriteLine("grav:"+ grav+" size:"+size.ToString());
                    FkPhysicsEngine.primitives.Clear();
                    //var size = 0;
                    for (int x = 1; x <= size; x++)
                        for (int y = 1; y <= size; y++)
                        {
                            A = new Primitive(
                           Position: new FkVector2(x * 75 + 5 * y, 1000 - y * 75 - x),
                           Rotation: 10 * x + y,
                           Velocity: new FkVector2(0, 0),
                           Angular: 0,
                           Scale: new FkVector2(25, 10),
                           IsBox: false,
                           IsStatic: false,
                           ApplyGenerators: true,
                           Mass: 10,
                           RestitutionFactor: 0.9);
                           FkPhysicsEngine.primitives.Add(A);
                        }
                    //here create define generators
                    if (grav == 1 || grav == 2) FkPhysicsEngine.generatores.Add(new FkGravity());
                    if (grav == 3 || grav == 4) FkPhysicsEngine.generatores.Add(new FkGravityRotation());
                    if (grav == 2 || grav == 4) FkPhysicsEngine.generatores.Add(new FkFriction());

                    //start animation:
                    //clock.Start();
                    //==========tests:
                    var dt = DateTime.Now;

                    //Console.WriteLine(dt.ToString());
                    //simulation: 100 frames / second
                    //vritual test time: 60 seconds
                    //frame count 6000
                    for (int i = 0; i <= 6000; i++)
                    {
                        FkPhysicsEngine.Update();
                        //Console.WriteLine(i);
                    }
                    var delta = DateTime.Now.Subtract(dt).TotalMilliseconds;
                    Console.WriteLine(delta.ToString());
                }
            RenderFrame(null, null);
        }

        public static readonly double thickness = 1;
        public static readonly double canvasHeight = 1000;
        
        void RenderFrame(object? sender, EventArgs e)
        {
            //simulation
            FkPhysicsEngine.Update();

            //render
            var objects = FkPhysicsEngine.primitives;
            MyCanvas.Children.Clear();
            foreach (var obj in objects) {
                var vert = obj.RALgetRenderVerts();
                DrawLine(obj.Position.X, obj.Position.Y, vert[0].X, vert[0].Y);
                for (int i = 1; i < vert.Length; i++)
                {
                    DrawLine(vert[i - 1].X, vert[i - 1].Y, vert[i].X, vert[i].Y);
                }
                DrawLine(vert[vert.Length - 1].X, vert[vert.Length - 1].Y, vert[0].X, vert[0].Y);
            }
        }
        //draws line from [sX, sY] to [eX, eY]
        private void DrawLine(double sX, double sY, double eX, double eY) {
            var myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.X1 = sX;
            myLine.Y1 = canvasHeight - sY;
            myLine.X2 = eX;
            myLine.Y2 = canvasHeight - eY;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = thickness;
            MyCanvas.Children.Add(myLine);
        }
    }
}
