using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using BRIE.Etc;
using ImageMagick;
using static BRIE.Roads;

namespace BRIE.ImageFormats
{
    internal static class Png16
    {
        public static PngBitmapEncoder PngEncoder = new PngBitmapEncoder();
        public static string FileName = @"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\gray16.png";
        private static ushort[] png16;

        public static BackgroundWorker RenderWorker(Roads Roads)
        {
            png16 = new ushort[Project.ResolutionSquared];
            BackgroundWorker bgw = new BackgroundWorker();

            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += (obj, arg) =>
            {
                ushort[] finalPixels = new ushort[Project.ResolutionSquared];


                

                foreach (Road Road in Roads.All)
                {
                    for (int nodeIndex = 0; nodeIndex < Road.Nodes.Count - 1; nodeIndex++)
                    {
                        renderSegment(Road.Nodes[nodeIndex], Road.Nodes[nodeIndex + 1]);
                        double perc = (double)nodeIndex / Road.Nodes.Count * 100;
                        bgw.ReportProgress((int)perc, "Generating Segments..");
                    }
                }



                bgw.ReportProgress(-1, "Rendering Png16...");
                bgw.ReportProgress(100, "Rendering Png16 Done!");
            };

            return bgw;
        }

        private static void renderSegment(Road.Node Start, Road.Node End, int thickness = 10)
        {

            ushort startColor = (ushort)(Start.NormalizedElevation * ushort.MaxValue);
            ushort endColor = (ushort)(End.NormalizedElevation * ushort.MaxValue);

            Vector segmentVector = End.ScaledPosition.FlipY() - Start.ScaledPosition.FlipY(); // Corrected the subtraction order
            double segLength = segmentVector.Length;
            double segHeightStep = segmentVector.Y / segLength;
            double segWidthStep = segmentVector.X / segLength; // Corrected variable name
            double colorDelta = endColor - startColor;
            double colStep = colorDelta / segLength;



            for (int i = 0; i < segLength; i++)
            {
                double x = Start.ScaledPosition.X + (segWidthStep * i); // Corrected variable name
                double y = Start.ScaledPosition.FlipY().Y + (segHeightStep * i); // Corrected variable name

                int stepX = (int)Math.Round(x); // Rounded to the nearest integer
                int stepY = (int)Math.Round(y); // Rounded to the nearest integer

                for (int j = (int)-thickness / 2; j < thickness / 2; j++)
                {
                    //if (stepX >= 0 && stepX < maskWidth && stepY >= 0 && stepY < maskHeight) // Check if within bounds
                    //{
                    int stepYj = stepY - (int)(segWidthStep * j);
                    int stepXj = stepX + (int)(segHeightStep * j);
                    int pixel = stepYj * Project.Resolution + stepXj;
                    int pixel2 = stepYj * Project.Resolution + stepXj + Project.Resolution;
                    ushort newColor = (ushort)(startColor + colStep * i);
                    if (pixel >= 0 && pixel < Project.ResolutionSquared)
                    {
                        png16[pixel] = LighterPixel(pixel, newColor);

                    }

                    if (pixel2 >= 0 && pixel2 < Project.ResolutionSquared)
                    {
                        png16[pixel2] = LighterPixel(pixel2, newColor);

                    }

                }
            }

            //int initpixel = (int)Start.ScaledPosition.Y * Project.Resolution + (int)Start.ScaledPosition.X;
            //int endpixel = (int)End.ScaledPosition.Y * Project.Resolution + (int)End.ScaledPosition.X;
        }

        private static ushort LighterPixel(int pixel, ushort newColor)
        {
            return Math.Max(newColor, png16[pixel]);
        }


        public static void SaveImage()
        {

            var bitmap = new WriteableBitmap(Project.Resolution, Project.Resolution, 96, 96, PixelFormats.Gray16, null);

            Int32Rect rect = new Int32Rect(0, 0, Project.Resolution, Project.Resolution);
            bitmap.WritePixels(rect, png16, Project.Resolution * 2, 0);
            var bmpf = BitmapFrame.Create(bitmap);
            var owner = PngEncoder.Dispatcher.Thread.Name;
            PngEncoder.Frames.Add(bmpf);
            using (var stream = System.IO.File.Create(FileName))
                PngEncoder.Save(stream);
            PngEncoder = new PngBitmapEncoder();
            FileManager.Start(FileName);
        }
    }

    public class RoadSegment
    {
        public double Thickness;
        public Point Start, End;
        public Vector Direction
        {
            get
            {
                return Start - End;
            }
        }

        public double Angle
        {
            get
            {
                double radians = Math.Atan2(Direction.Y, Direction.X);

                // Convert radians to degrees
                double degrees = radians * (180 / Math.PI);

                // Ensure the angle is between 0 and 360 degrees
                degrees = (degrees + 360) % 360;

                return degrees;
            }
        }

        public PointD[] Shape
        {
            get
            {

                Point[] perp1 = GetPerpendicular(Start, Angle, Thickness);
                Point[] perp2 = GetPerpendicular(End, Angle, Thickness);


                return new PointD[] {
                    new PointD(perp1[0].X, perp1[0].Y),
                    new PointD(perp1[1].X, perp1[1].Y),
                    new PointD(perp2[1].X, perp2[1].Y),
                    new PointD(perp2[0].X, perp2[0].Y)
                };
            }
        }



        public RoadSegment(Point start, Point end, double thickness)
        {
            Start = start;
            End = end;
            Thickness = thickness;
        }

        private Point[] GetPerpendicular(Point center, double normal, double length, bool anti = false)
        {
            // Convert normal angle to radians
            double degs = (normal + 90) % 360;
            double radians = degs * Math.PI / 180;

            // Calculate the endpoint coordinates
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double endX = center.X + length * cos;
            double endY = center.Y + length * sin;

            if (anti)
            {
                // Create the line
                return new Point[2]
                {
                    new(center.X - (endX - center.X), center.Y - (endY - center.Y)),
                    new(endX, endY)
                };
            }
            else
            {
                // Create the line
                return new Point[2]
                {
                    new(center.X - (endX - center.X), center.Y - (endY - center.Y)),
                    new(endX,endY)
                };
            }
        }

    }

}
