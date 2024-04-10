using System;
using System.Collections;
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
using BRIE.ClassExtensions;
using BRIE.Etc;
using ImageMagick;
using static BRIE.Roads;

namespace BRIE.ImageFormats
{
    internal static class Png16
    {
        public static PngBitmapEncoder PngEncoder = new PngBitmapEncoder();
        public static string FileName = @"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\gray16.png";
        public static string InterFileName = @"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\gray16Inter.png";
        public static string MaskFileName = @"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\gray16Mask.png";
        private static ushort[]? png16;
        private static ushort[]? png16mask;

        public static BackgroundWorker RenderWorker(Roads Roads)
        {
            png16 = new ushort[Project.ResolutionSquared];
            png16mask = new ushort[Project.ResolutionSquared];

            BackgroundWorker bgw = new BackgroundWorker();

            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += (obj, arg) =>
            {
                png16 = new ushort[Project.ResolutionSquared];
                for (int roadIndex = 0; roadIndex < Roads.All.Count - 1; roadIndex++)
                {
                    Road Road = Roads.All[roadIndex];
                    for (int nodeIndex = 0; nodeIndex < Road.Nodes.Count - 1; nodeIndex++)
                    {
                        renderSegment(Road.Nodes[nodeIndex], Road.Nodes[nodeIndex + 1]);
                    }
                    double perc = (double)roadIndex / Roads.All.Count * 100;
                    bgw.ReportProgress((int)perc, "Generating Segments..");
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
            int segPixelLength = (int)Math.Round(segmentVector.Length);
            double segHeightStep = segmentVector.Y / segLength;
            double segWidthStep = segmentVector.X / segLength; // Corrected variable name
            double colorDelta = endColor - startColor;
            double colStep = colorDelta / segLength;



            for (int i = 0; i <= (int)segLength; i += 1)
            {
                double x = Start.ScaledPosition.X + (segWidthStep * i);
                double y = Start.ScaledPosition.FlipY().Y + (segHeightStep * i);

                int stepX = (int)Math.Round(x);
                int stepY = (int)Math.Round(y);

                ushort newColor = (ushort)(startColor + colStep * i);

                //Draws the line
                //DrawPixel(stepX, stepY, newColor);


                // Draw perpendicular lines
                for (int j = -thickness / 2; j <= thickness / 2; j++)
                {
                    double preciseStepYj = stepY - segWidthStep * j;
                    double preciseStepXj = stepX + segHeightStep * j;

                    double preciseNextStepYj = stepY - segWidthStep * (j + 1);
                    double preciseNextStepXj = stepX + segHeightStep * (j + 1);


                    int perpStepX = (int)Math.Round(preciseStepXj);
                    int perpStepY = (int)Math.Round(preciseStepYj);
                    Point perpStep = new(perpStepX, perpStepY);

                    int perpNextStepX = (int)Math.Round(preciseNextStepXj);
                    int perpNextStepY = (int)Math.Round(preciseNextStepYj);
                    Point perpNextStep = new(perpNextStepX, perpNextStepY);

                    Point perpInterStepX = new(perpNextStepX, perpStepY);
                    Point perpInterStepY = new(perpStepX, perpNextStepY);




                    if (perpInterStepX != perpNextStep && perpInterStepX != perpStep)
                        DrawPixel(perpInterStepX, newColor);

                    if (perpInterStepY != perpNextStep && perpInterStepY != perpStep)
                        DrawPixel(perpInterStepY, newColor);

                    // Draw the perpendicular pixel
                    DrawPixel(perpStepX, perpStepY, newColor);

                }
            }

        }

        private static void DrawPixel(int pixelIndex, ushort color)
        {
            if (pixelIndex >= 0 && pixelIndex < Project.ResolutionSquared)
            {
                png16[pixelIndex] = color;
                png16mask[pixelIndex] = ushort.MaxValue;
            }
        }

        private static void DrawPixel(int x, int y, ushort color) => Png16.DrawPixel(GetPixelIndex(x, y), color);
        private static void DrawPixel(Point s, ushort color) => Png16.DrawPixel(GetPixelIndex((int)s.X, (int)s.Y), color);



        public static int GetPixelIndex(int x, int y)
        {
            return y * Project.Resolution + x;
        }

        //public static void DrawLine(Point Start, Point End, ushort startColor) => DrawLine(Start, End, startColor, startColor);

        private static Point[] GetPerpendicular(Point center, double normal, double length, bool anti = false)
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
            PngEncoder.Frames.Clear();
            PngEncoder.Frames.Add(bmpf);
            using (var stream = System.IO.File.Create(FileName))
                PngEncoder.Save(stream);
            PngEncoder = new PngBitmapEncoder();
            png16 = new ushort[Project.ResolutionSquared];
            //FileManager.Start(FileName);
        }

        public static void SaveMaskImage()
        {

            var bitmap = new WriteableBitmap(Project.Resolution, Project.Resolution, 96, 96, PixelFormats.Gray16, null);

            Int32Rect rect = new Int32Rect(0, 0, Project.Resolution, Project.Resolution);
            bitmap.WritePixels(rect, png16mask, Project.Resolution * 2, 0);
            var bmpf = BitmapFrame.Create(bitmap);
            PngEncoder.Frames.Clear();
            PngEncoder.Frames.Add(bmpf);
            using (var stream = System.IO.File.Create(MaskFileName))
                PngEncoder.Save(stream);
            PngEncoder = new PngBitmapEncoder();
            //FileManager.Start(MaskFileName);
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
