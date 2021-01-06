using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//using Windows.Devices.Sensors; // !!!

using Xamarin.Forms;
//using Xamarin.Essentials;

using Xamarin.FormsBook.Platform;

//using SkiaSharp;
//using SkiaSharp.Views.Forms;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace TiltMazePuzzle
{

	public partial class MainPage : ContentPage
	{
        const float GRAVITY = 1000;     // pixels per second squared
        const float BOUNCE = 2f / 3;    // fraction of velocity

        const int MAZE_HORZ_CHAMBERS = 14; // 5;
        const int MAZE_VERT_CHAMBERS = 16; //8;

        const int WALL_WIDTH = 4; //16;
        const int BALL_RADIUS = 6; //12;
        const int HOLE_RADIUS = 8; // 18;

        IDeviceInfo deviceInfo; // !

        
        EllipseView ball;
        EllipseView hole;
        MazeGrid mazeGrid;
        List<Line2D> borders = new List<Line2D>();

        Random random = new Random();

        Stopwatch stopwatch = new Stopwatch();
        bool isBallInPlay = false;
        TimeSpan lastElapsedTime;

        Vector2 acceleration;
        Vector2 ballVelocity = Vector2.Zero;
        Vector2 ballPosition;
        Vector2 holePosition;

        //Accelerometer accelerometer; 
        //AccelerometerReading AccelerometerReading;

        // Этот интерфейс будет определять сигнатуру методов, 
        // реализация которых будет зависеть от конкретной платформы. 
        // К примеру, нам надо вывести номер версии операционной системы.
        public interface IDeviceInfo
        {
            bool GetInfo();
            double GetX();
            double GetY();
            double GetZ();
        }

        public MainPage()
		{
            InitializeComponent();

            //accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();

            //if (accelerometer != null)
            //{
            //accelerometer.ReadingChanged += accelerometer_ReadingChanged;

            //    AccelerometerReading AccelerometerReading = accelerometer.GetCurrentReading();
            //}


            // Для получения информации об устройстве здесь используется вызов:
            // IDeviceInfo deviceInfo = DependencyService.Get<IDeviceInfo>();
            // Здесь с помощью метода Get() мы получаем объект IDeviceInfo. 
            // При этом не важно, что в Portable-проекте нет конкретной реализации 
            // данного интерфейса. В качестве реализации будет использоваться объект 
            // класса, который будет определен в проекте для текущей платформы

            deviceInfo = DependencyService.Get<IDeviceInfo>();

            //Label infoLabel = new Label();

            //infoLabel.Text = "test: X=" + deviceInfo.GetX().ToString();

            // проверка, есть ли акселерометр =)
            bool AccFlag = deviceInfo.GetInfo();



            /*
            Accelerometer.ReadingChanged += (sender, args) =>
            {
                // !!! DEBUG ONLY !!!
                Debug.WriteLine(deviceInfo.GetX().ToString());
                Debug.WriteLine(deviceInfo.GetY().ToString());
                Debug.WriteLine(deviceInfo.GetZ().ToString());


                //acceleration = 0.5f * args.Reading.Acceleration + 0.5f * acceleration; //vektor3->vektor2 error

                // Smooth the reading by averaging with prior values
                //System.Numerics.Vector3 accdata3 = args.Reading.Acceleration;
                System.Numerics.Vector2 accdata2;
                accdata2.X = (float) deviceInfo.GetX(); //accdata3.X;
                accdata2.Y = (float) deviceInfo.GetY(); //accdata3.Y;
                acceleration = 0.5f * accdata2 + 0.5f * acceleration;

            };
            */

            // если его нет, то таймер не запускаем
            if (AccFlag)
            {

                Device.StartTimer(TimeSpan.FromMilliseconds(33), () =>
                {

                    // -----------------------------------

                    // Sensor reading zone: PLAN B

                    // !!! DEBUG ONLY !!!
                    //Debug.WriteLine(deviceInfo.GetX().ToString());
                    //Debug.WriteLine(deviceInfo.GetY().ToString());
                    //Debug.WriteLine(deviceInfo.GetZ().ToString());


                    //acceleration = 0.5f * args.Reading.Acceleration + 0.5f * acceleration; //vektor3->vektor2 error

                    // Smooth the reading by averaging with prior values
                    //System.Numerics.Vector3 accdata3 = args.Reading.Acceleration;
                    System.Numerics.Vector2 accdata2;
                    accdata2.X = -(float)deviceInfo.GetX(); //accdata3.X;
                    accdata2.Y = -(float)deviceInfo.GetY(); //accdata3.Y;
                    acceleration = 0.5f * accdata2 + 0.5f * acceleration;

                    // -----------------------------------

                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    float deltaSeconds = (float)(elapsedTime - lastElapsedTime).TotalSeconds;
                    lastElapsedTime = elapsedTime;

                    if (isBallInPlay)
                    {
                        // MoveBall returns true for end of game
                        if (MoveBall(deltaSeconds))
                        {
                            // Aysnchronous method
                            TransitionToNewGame();
                        }
                    }

                    return true;
                });
            }
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // проверка, есть ли поддержка сенсора Акселерометр...               
            //try
            //{
            //    Accelerometer.Start(SensorSpeed.Default);
            //}
            //catch
            // проверка, есть ли акселерометр =)
            bool AccFlag = deviceInfo.GetInfo();

            if (!AccFlag)
            {
                Label label = new Label
                {
                    Text = "Sorry, an accelerometer is not available on this device",
                    FontSize = 24,
                    TextColor = Color.White,
                    BackgroundColor = Color.DarkGray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(48, 0)
                };

                absoluteLayout.Children.Add(label,
                      new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                      AbsoluteLayoutFlags.PositionProportional);
            }
            else
            {
                stopwatch.Start();
            }
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //Accelerometer.Stop();

            bool AccFlag = deviceInfo.GetInfo();

            if (AccFlag)
            {
                stopwatch.Stop();
            }
        
        }

        void OnAbsoluteLayoutSizeChanged(object sender, EventArgs args)
        {
            if (Width > 0 && Height > 0)
            {
                NewGame((float)absoluteLayout.Width, (float)absoluteLayout.Height);
                isBallInPlay = true;

                // Detach this handler to avoid interrupting games
                absoluteLayout.SizeChanged -= OnAbsoluteLayoutSizeChanged;
            }
        }

        async void TransitionToNewGame()
        {
            isBallInPlay = false;
            absoluteLayout.Children.Remove(ball);
            await absoluteLayout.FadeTo(0, 1000);
            NewGame((float)absoluteLayout.Width, (float)absoluteLayout.Height);
            await absoluteLayout.FadeTo(1, 500);
            isBallInPlay = true;
        }

        void NewGame(float width, float height)
        {
            // The constructor creates the random maze layout
            mazeGrid = new MazeGrid(MAZE_HORZ_CHAMBERS, MAZE_VERT_CHAMBERS);

            // Initialize borders collection
            borders.Clear();

            // Create Line2D objects for the border lines of the maze
            float cellWidth = width / mazeGrid.Width;
            float cellHeight = height / mazeGrid.Height;
            int halfWallWidth = WALL_WIDTH / 2;

            for (int x = 0; x < mazeGrid.Width; x++)
                for (int y = 0; y < mazeGrid.Height; y++)
                {
                    MazeCell mazeCell = mazeGrid.Cells[x, y];
                    Vector2 ll = new Vector2(x * cellWidth, (y + 1) * cellHeight);
                    Vector2 ul = new Vector2(x * cellWidth, y * cellHeight);
                    Vector2 ur = new Vector2((x + 1) * cellWidth, y * cellHeight);
                    Vector2 lr = new Vector2((x + 1) * cellWidth, (y + 1) * cellHeight);
                    Vector2 right = halfWallWidth * Vector2.UnitX;
                    Vector2 left = -right;
                    Vector2 down = halfWallWidth * Vector2.UnitY;
                    Vector2 up = -down;

                    if (mazeCell.HasLeft)
                    {
                        borders.Add(new Line2D(ll + down, ll + down + right));
                        borders.Add(new Line2D(ll + down + right, ul + up + right));
                        borders.Add(new Line2D(ul + up + right, ul + up));
                    }
                    if (mazeCell.HasTop)
                    {
                        borders.Add(new Line2D(ul + left, ul + left + down));
                        borders.Add(new Line2D(ul + left + down, ur + right + down));
                        borders.Add(new Line2D(ur + right + down, ur + right));
                    }
                    if (mazeCell.HasRight)
                    {
                        borders.Add(new Line2D(ur + up, ur + up + left));
                        borders.Add(new Line2D(ur + up + left, lr + down + left));
                        borders.Add(new Line2D(lr + down + left, lr + down));
                    }
                    if (mazeCell.HasBottom)
                    {
                        borders.Add(new Line2D(lr + right, lr + right + up));
                        borders.Add(new Line2D(lr + right + up, ll + left + up));
                        borders.Add(new Line2D(ll + left + up, ll + left));
                    }
                }

            // Prepare AbsoluteLayout for new children
            absoluteLayout.Children.Clear();

            // "Draw" the walls of the maze using BoxView
            BoxView createBoxView() => new BoxView { Color = Color.Green };

            for (int x = 0; x < mazeGrid.Width; x++)
                for (int y = 0; y < mazeGrid.Height; y++)
                {
                    MazeCell mazeCell = mazeGrid.Cells[x, y];

                    if (mazeCell.HasLeft)
                    {
                        Rectangle rect = new Rectangle(x * cellWidth,
                                                       y * cellHeight - halfWallWidth,
                                                       halfWallWidth, cellHeight + WALL_WIDTH);

                        absoluteLayout.Children.Add(createBoxView(), rect);
                    }

                    if (mazeCell.HasRight)
                    {
                        Rectangle rect = new Rectangle((x + 1) * cellWidth - halfWallWidth,
                                                       y * cellHeight - halfWallWidth,
                                                       halfWallWidth, cellHeight + WALL_WIDTH);

                        absoluteLayout.Children.Add(createBoxView(), rect);
                    }

                    if (mazeCell.HasTop)
                    {
                        Rectangle rect = new Rectangle(x * cellWidth - halfWallWidth,
                                                       y * cellHeight,
                                                       cellWidth + WALL_WIDTH, halfWallWidth);

                        absoluteLayout.Children.Add(createBoxView(), rect);
                    }

                    if (mazeCell.HasBottom)
                    {
                        Rectangle rect = new Rectangle(x * cellWidth - halfWallWidth,
                                                       (y + 1) * cellHeight - halfWallWidth,
                                                       cellWidth + WALL_WIDTH, halfWallWidth);

                        absoluteLayout.Children.Add(createBoxView(), rect);
                    }
                }

            // Randomly position ball in one of the corners
            bool isBallLeftCorner = random.Next(2) == 0;
            bool isBallTopCorner = random.Next(2) == 0;

            // Create the hole first (so Z order is under the ball) 
            //      and position it in the opposite corner from the ball
            hole = new EllipseView
            {
                Color = Color.Black,
                WidthRequest = 2 * HOLE_RADIUS,
                HeightRequest = 2 * HOLE_RADIUS
            };

            holePosition = new Vector2((isBallLeftCorner ? 2 * mazeGrid.Width - 1 : 1) * (width / mazeGrid.Width / 2),
                                       (isBallTopCorner ? 2 * mazeGrid.Height - 1 : 1) * (height / mazeGrid.Height / 2));

            absoluteLayout.Children.Add(hole, new Point(holePosition.X - HOLE_RADIUS, 
                                                        holePosition.Y - HOLE_RADIUS));

            // Create the ball and set initial position 
            ball = new EllipseView
            {
                Color = Color.Red,
                WidthRequest = 2 * BALL_RADIUS,
                HeightRequest = 2 * BALL_RADIUS
            };

            ballPosition = new Vector2((isBallLeftCorner ? 1 : 2 * mazeGrid.Width - 1) * (width / mazeGrid.Width / 2),
                                       (isBallTopCorner ? 1 : 2 * mazeGrid.Height - 1) * (height / mazeGrid.Height / 2));

            absoluteLayout.Children.Add(ball, new Point(ballPosition.X - BALL_RADIUS,
                                                        ballPosition.Y - BALL_RADIUS));
        }

        bool MoveBall(float deltaSeconds)
        {
            // Convert to standard notation for ease of manipulation
            float t = deltaSeconds;
            Vector2 r0 = ballPosition;
            Vector2 r = new Vector2();
            Vector2 v0 = ballVelocity;
            Vector2 v = new Vector2();
            Vector2 a = GRAVITY * new Vector2(-acceleration.X, acceleration.Y);

            while (t > 0)
            {
                // Here's the basic physics
                r = r0 + v0 * t + 0.5f * a * t * t;
                v = v0 + a * t;

                // Set to real Line2D values if the ball is rolling on a horizontal or vertical edge.
                // If both are set, the ball is not moving in a corner.
                Line2D horzRollLine = new Line2D();
                Line2D vertRollLine = new Line2D();

                // Check for a rolling ball.
                // It's considered rolling if it's within 0.1 pixels of an edge.
                // It's set at a distance of 0.01 pixels away to avoid getting snagged by line.
                foreach (Line2D line in borders)
                {
                    Line2D shiftedLine = line.ShiftOut(BALL_RADIUS * line.Normal);
                    Vector2 pt1 = shiftedLine.Point1;
                    Vector2 pt2 = shiftedLine.Point2;
                    Vector2 normal = shiftedLine.Normal;

                    // Rolling on horizontal edge?
                    if (normal.X == 0 && r0.X > Math.Min(pt1.X, pt2.X) &&
                                         r0.X < Math.Max(pt1.X, pt2.X))
                    {
                        float y = pt1.Y;

                        // Rolling on bottom edge?
                        if (normal.Y > 0 && Math.Abs(r0.Y - y) < 0.1f && r.Y < y)
                        {
                            r.Y = y + 0.01f;
                            v.Y = 0;
                            horzRollLine = line;
                        }
                        // Rolling on top edge?
                        else if (normal.Y < 0 && Math.Abs(y - r0.Y) < 0.1f && r.Y > y)
                        {
                            r.Y = y - 0.01f;
                            v.Y = 0;
                            horzRollLine = line;
                        }
                    }
                    // Rolling on vertical edge? 
                    else if (normal.Y == 0 && r0.Y > Math.Min(pt1.Y, pt2.Y) &&
                                              r0.Y < Math.Max(pt1.Y, pt2.Y))
                    {

                        float x = pt1.X;

                        // Rolling on right side?
                        if (normal.X > 0 && Math.Abs(r0.X - x) < 0.1f && r.X < x)
                        {
                            r.X = x + 0.01f;
                            v.X = 0;
                            vertRollLine = line;
                        }
                        // Rolling on left side?
                        else if (normal.X < 0 && Math.Abs(x - r0.X) < 0.1f && r.X > x)
                        {
                            r.X = x - 0.01f;
                            v.X = 0;
                            vertRollLine = line;
                        }
                    }
                }

                // Set to the information for the minimum distance
                float distanceToCollision = float.MaxValue;
                Line2D collisionLine = new Line2D();
                Vector2 collisionPoint = new Vector2();

                foreach (Line2D line in borders)
                {
                    // Skip the Line2D objects that the ball is rolling along
                    if (line.Equals(horzRollLine) || line.Equals(vertRollLine))
                    {
                        continue;
                    }

                    // Check if ball has crossed a line of the wall
                    Line2D shiftedLine = line.ShiftOut(BALL_RADIUS * line.Normal);
                    Line2D ballTrajectory = new Line2D(r0, r);
                    Vector2 intersection = shiftedLine.SegmentIntersection(ballTrajectory);
                    float angleDiff = WrapAngle(line.Angle - ballTrajectory.Angle);

                    // If so, save the one with the shortest distance, same as shortest time
                    if (Line2D.IsValid(intersection) && angleDiff > 0)
                    {
                        float distance = (intersection - r0).Length();

                        // If it's less distance than the previous ones, save that info
                        if (distance < distanceToCollision)
                        {
                            distanceToCollision = distance;
                            collisionLine = line;
                            collisionPoint = intersection;
                        }
                    }
                }

                // If there's is a bounce, here's where it's handled
                if (distanceToCollision < float.MaxValue)
                {
                    if (distanceToCollision < 0.1f)
                    {
                        //  System.Diagnostics.Debug.WriteLine("Unexpected distanceToCCollision < 0.1f");
                        //  break;
                    }

                    // The velocity magnitude at the collision point
                    float vMag = (float)Math.Sqrt(v0.LengthSquared() + 2 * a.Length() * distanceToCollision);

                    // New velocity vector
                    v = vMag * Vector2.Normalize(v0);

                    // The time until collision
                    float tCollision = (vMag - v0.Length()) / a.Length();

                    // Set up for next iteration
                    t -= tCollision;
                    r0 = collisionPoint;
                    v0 = BOUNCE * Vector2.Reflect(v, collisionLine.Normal);

                    if (tCollision == 0)
                    {
                        //System.Diagnostics.Debug.WriteLine("Unexpected tCollision == 0");
                        //break;
                    }
                }
                // Otherwise, there's no bounce, so stop the iterations
                else
                {
                    t = 0;
                }
            }
            
            // New ball position and velocity
            ballPosition = r;
            ballVelocity = v;

            // Position the ball at ballPosition
            Rectangle ballRect = new Rectangle(ballPosition.X - BALL_RADIUS,
                                               ballPosition.Y - BALL_RADIUS,
                                               AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

            AbsoluteLayout.SetLayoutBounds(ball, ballRect);

            // Return true for GAME OVER if the ball is within the hole 
            return (ballPosition - holePosition).Length() < HOLE_RADIUS - BALL_RADIUS;
        }

        // Forces angle between -PI and PI
        float WrapAngle(float angle)
        {
            const float pi = (float)Math.PI;

            angle = (float)Math.IEEERemainder(angle, 2 * pi);

            if (angle <= -pi)
            {
                angle += 2 * pi;
            }
            if (angle > pi)
            {
                angle -= 2 * pi;
            }
            return angle;
        }
    }
}


/*
namespace TiltMazePuzzle
{
    public partial class MainPage : ContentPage
    {
        SKPaint blackFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black
        };

        SKPaint whiteStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 2,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint whiteFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        SKPaint greenFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.PaleGreen
        };

        SKPaint blackStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 20,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint grayFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray
        };

        SKPaint backgroundFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill
        };

        SKPath catEarPath = new SKPath();
        SKPath catEyePath = new SKPath();
        SKPath catPupilPath = new SKPath();
        SKPath catTailPath = new SKPath();

        SKPath hourHandPath = SKPath.ParseSvgPathData(
            "M 0 -60 C 0 -30 20 -30 5 -20 L 5 0 C 5 7.5 -5 7.5 -5 0 L -5 -20 C -20 -30 0 -30 0 -60");
        SKPath minuteHandPath = SKPath.ParseSvgPathData(
            "M 0 -80 C 0 -75 0 -70 2.5 -60 L 2.5 0 C 2.5 5 -2.5 5 -2.5 0 L -2.5 -60 C 0 -70 0 -75 0 -80");

        public MainPage()
        {
            InitializeComponent();

            // Make cat ear path
            catEarPath.MoveTo(0, 0);
            catEarPath.LineTo(0, 75);
            catEarPath.LineTo(100, 75);
            catEarPath.Close();

            // Make cat eye path
            catEyePath.MoveTo(0, 0);
            catEyePath.ArcTo(50, 50, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, 50, 0);
            catEyePath.ArcTo(50, 50, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, 0, 0);
            catEyePath.Close();

            // Make eye pupil path
            catPupilPath.MoveTo(25, -5);
            catPupilPath.ArcTo(6, 6, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, 25, 5);
            catPupilPath.ArcTo(6, 6, 0, SKPathArcSize.Small, SKPathDirection.Clockwise, 25, -5);
            catPupilPath.Close();
            
            // Make cat tail path
            catTailPath.MoveTo(0, 100);
            catTailPath.CubicTo(50, 200, 0, 250, -50, 200);

            // Create Shader
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("TiltMazePuzzle.WoodGrain.png"))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            using (SKBitmap bitmap = SKBitmap.Decode(skStream))
            using (SKShader shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror))
            {
                backgroundFillPaint.Shader = shader;
            }

            Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
                {
                    canvasView.InvalidateSurface();
                    return true;
                });
        }

        private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.DrawPaint(backgroundFillPaint);

            int width = e.Info.Width;
            int height = e.Info.Height;

            // Set transforms
            canvas.Translate(width / 2, height / 2);
            canvas.Scale(Math.Min(width / 210f, height / 520f));

            // Get DateTime
            DateTime dateTime = DateTime.Now;

            // Head
            canvas.DrawCircle(0, -160, 75, blackFillPaint);

            // Draw ears and eyes
            for (int i = 0; i < 2; i++)
            {
                canvas.Save();
                canvas.Scale(2 * i - 1, 1);

                canvas.Save();
                canvas.Translate(-65, -255);
                canvas.DrawPath(catEarPath, blackFillPaint);
                canvas.Restore();

                canvas.Save();
                canvas.Translate(10, -170);
                canvas.DrawPath(catEyePath, greenFillPaint);
                canvas.DrawPath(catPupilPath, blackFillPaint);
                canvas.Restore();

                // Draw whiskers
                canvas.DrawLine(10, -120, 100, -100, whiteStrokePaint);
                canvas.DrawLine(10, -125, 100, -120, whiteStrokePaint);
                canvas.DrawLine(10, -130, 100, -140, whiteStrokePaint);
                canvas.DrawLine(10, -135, 100, -160, whiteStrokePaint);

                canvas.Restore();
            }

            // Move Tail
            float t = (float)Math.Sin((dateTime.Second % 2 + dateTime.Millisecond / 1000.0) * Math.PI);
            catTailPath.Reset();
            catTailPath.MoveTo(0, 100);
            SKPoint point1 = new SKPoint(-50 * t, 200);
            SKPoint point2 = new SKPoint(0, 250 - Math.Abs(50 * t));
            SKPoint point3 = new SKPoint(50 * t, 250 - Math.Abs(75 * t));
            catTailPath.CubicTo(point1, point2, point3);

            canvas.DrawPath(catTailPath, blackStrokePaint);

            // Clock background
            canvas.DrawCircle(0, 0, 100, blackFillPaint);

            // Hour and minute marks
            for (int angle = 0; angle < 360; angle += 6)
            {
                canvas.DrawCircle(0, -90, angle % 30 == 0 ? 4 : 2, whiteFillPaint);
                canvas.RotateDegrees(6);
            }

            // Hour hand
            canvas.Save();
            canvas.RotateDegrees(30 * dateTime.Hour + dateTime.Minute / 2f);
            canvas.DrawPath(hourHandPath, grayFillPaint);
            canvas.DrawPath(hourHandPath, whiteStrokePaint);
            canvas.Restore();

            // Minute hand
            canvas.Save();
            canvas.RotateDegrees(6 * dateTime.Minute + dateTime.Second / 10f);
            canvas.DrawPath(minuteHandPath, grayFillPaint);
            canvas.DrawPath(minuteHandPath, whiteStrokePaint);
            canvas.Restore();

            // Second hand
            canvas.Save();
            float seconds = dateTime.Second + dateTime.Millisecond / 1000f;
            canvas.RotateDegrees(6 * seconds);
            whiteStrokePaint.StrokeWidth = 2;
            canvas.DrawLine(0, 10, 0, -80, whiteStrokePaint);
            canvas.Restore();
        }
    }
}
*/
