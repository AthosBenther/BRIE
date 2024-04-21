using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BRIE.Etc;
using BRIE.ExportFormats;
using BRIE.ExportFormats.FileFormats;
using BRIE.ExportFormats.FileFormats.Meta;
using BRIE.Types;

namespace BRIE.Export
{
    public static class ImageRaycasting
    {
        private static object lockObject = new object();
        public static List<FileFormat> Formats => new()
                {
                    new Png(),
                    new Tiff(),
                    new Bmp()
                };

        public static List<string> FormatsExtensions => Formats.Select(f => ((IFileFormat)f).ShortName).ToList();

        public static FileFormat FileFormat;
        public static PixelFormat PixelFormat = PixelFormats.Gray8;
        public static BitmapEncoder Encoder;
        public static int SuperSampling = 1;


        private static double pixelMaxValue;
        private static double pixelMinValue;
        private static PixelArray Heightmap;
        private static PixelArray Mask;
        private static int ImageResolution => Project.Resolution * SuperSampling;
        private static int ImageResolutionSquared => (int)Math.Pow(ImageResolution, 2);
        public static BackgroundWorker RenderWorker(RoadsCollection RoadsCollection, bool useTerrain = false)
        {
            Heightmap = useTerrain ? new PixelArray(Project.HeightmapPath) : new PixelArray(ImageResolutionSquared);
            Mask = new PixelArray(ImageResolutionSquared);

            pixelMaxValue = Math.Pow(2, PixelFormat.BitsPerPixel) - 1;

            pixelMinValue = PixelFormat.ToString() == "Gray32Float" ? float.MinValue : 0;
            //pixelMinValue = 0;

            BackgroundWorker bgw = new BackgroundWorker();

            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += async (obj, arg) =>
            {
                List<BackgroundWorker> workerList = new List<BackgroundWorker>();


                for (int roadIndex = 0; roadIndex < RoadsCollection.All.Count - 1; roadIndex++)
                {
                    BackgroundWorker roadbgw = new BackgroundWorker();

                    Road road = RoadsCollection.All[roadIndex];

                    roadbgw.DoWork += (obj, arg) =>
                    {

                        for (int segmentIndex = 0; segmentIndex < road.Segments.Count; segmentIndex++)
                        {
                            int capturedIndex = segmentIndex;
                            Segment capturedSegment = road.Segments[capturedIndex];

                            // Capture the loop variable
                            double color = capturedSegment.Start.NormalizedElevation * pixelMaxValue;
                            renderPolygon(capturedSegment.Start.Polygon, color);
                            renderSegment(capturedSegment);
                        }


                    };

                    workerList.Add(roadbgw);
                }
                int completed = 0;
                int workersCount = workerList.Count;
                foreach (var w in workerList)
                {
                    w.RunWorkerCompleted += (o, e) =>
                    {
                        double perc = (double)completed / workersCount * 100;
                        bgw.ReportProgress((int)perc, "Generating Roads...");
                        completed++;
                    };

                    w.RunWorkerAsync();
                }
                while (completed < workersCount) ;
                bgw.ReportProgress(100, "Roads generation done!");
            };

            return bgw;
        }


        private static void renderSegment(Segment segment)
        {
            double startColor = segment.Start.NormalizedElevation * pixelMaxValue;
            double endColor = segment.End.NormalizedElevation * pixelMaxValue;
            double deltaColor = endColor - startColor;

            double incrementBy = 1d / SuperSampling;



            for (double y = segment.Bottom; y <= segment.Top; y += incrementBy)
            {

                for (double x = segment.Left; x <= segment.Right; x += incrementBy)
                {
                    Point currentPosition = new Point(x, y);
                    if (IsInside(currentPosition, segment.Polygon))
                    {
                        // Calculate the distance of the current pixel from the start point of the segment
                        double distance = (currentPosition - segment.Start.ScaledPosition.FlipY()).Length;

                        // Interpolate the color based on the distance from the start point
                        double pixelColor = startColor + deltaColor * (distance / segment.Lenght);
                        DrawPixel((int)(x * SuperSampling), (int)(y * SuperSampling), pixelColor);
                    }
                }

            }
        }

        private static void renderPolygon(Polygon poly, double color)
        {
            double incrementBy = 1d / SuperSampling;

            for (double y = poly.Bottom; y <= poly.Top; y += incrementBy)
            {
                for (double x = poly.Left; x <= poly.Right; x += incrementBy)
                {
                    Point currentPosition = new Point(x, y);
                    if (IsInside(currentPosition, poly))
                    {
                        DrawPixel((int)(x * SuperSampling), (int)(y * SuperSampling), color);
                    }
                }

            }
        }

        private static bool IsInside(Point point, Polygon Polygon)
        {
            int intersections = 0;

            int pointsCount = Polygon.Points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                Point p1 = Polygon.Points[i];
                Point p2 = Polygon.Points[(i + 1) % pointsCount];

                // Check if the point is within the y-range of the edge
                if ((p1.Y <= point.Y && point.Y < p2.Y) || (p2.Y <= point.Y && point.Y < p1.Y))
                {
                    // Calculate the x-coordinate where the edge intersects with the horizontal line at point.Y
                    double xIntersection = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;

                    // If the intersection point is to the right of the test point, it's considered an intersection
                    if (point.X < xIntersection)
                    {
                        intersections++;
                    }
                }
            }

            return intersections % 2 == 1;
        }


        private static void DrawPixel(int pixelIndex, double color)
        {
            if (pixelIndex >= 0 && pixelIndex < ImageResolutionSquared)
            {
                Heightmap.Pixels[pixelIndex] = color;
                Mask.Pixels[pixelIndex] = pixelMaxValue;
            }
        }

        private static void DrawPixel(int x, int y, double color) => DrawPixel(GetPixelIndex(x, y), color);
        private static void DrawPixel(Point s, double color) => DrawPixel(GetPixelIndex((int)s.X, (int)s.Y), color);

        public static int GetPixelIndex(int x, int y)
        {
            return y * ImageResolution + x;
        }

        public static BitmapFrame RenderImage()
        {
            var bitmap = new WriteableBitmap(ImageResolution, ImageResolution, 96, 96, PixelFormat, null);



            Int32Rect rect = new Int32Rect(0, 0, ImageResolution, ImageResolution);

            int stride = ImageResolution * (PixelFormat.BitsPerPixel / 8);

            Array pixels = Heightmap.GetPixels(PixelFormat);
            try
            {
                bitmap.WritePixels(rect, pixels, stride, 0);
            }
            catch (Exception)
            {

                //throw;
            }

            double scale = 1 / (double)SuperSampling;

            var targetBitmap = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));
            return BitmapFrame.Create(targetBitmap);
        }

        public static void Save(string FilePath)
        {
            var bitmap = new WriteableBitmap(ImageResolution, ImageResolution, 96, 96, PixelFormat, null);



            Int32Rect rect = new Int32Rect(0, 0, ImageResolution, ImageResolution);

            int stride = ImageResolution * (PixelFormat.BitsPerPixel / 8);

            Array pixels = Heightmap.GetPixels(PixelFormat);


            bitmap.WritePixels(rect, pixels, stride, 0);
            double scale = 1 / (double)SuperSampling;

            var targetBitmap = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));
            var bmpf = BitmapFrame.Create(targetBitmap);
            Encoder.Frames.Clear();
            Encoder.Frames.Add(bmpf);
            using (var stream = System.IO.File.Create(FilePath))
                Encoder.Save(stream);
            Encoder = FileFormat.Encoder;
        }
    }
}
