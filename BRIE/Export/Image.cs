using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BRIE.Etc;
using BRIE.ExportFormats;
using BRIE.ExportFormats.FileFormats;
using BRIE.ExportFormats.FileFormats.Meta;
using static BRIE.Roads;

namespace BRIE.Export
{
    public static class Image
    {
        public static List<FileFormat> Formats => new()
                {
                    new Png(),
                    new Tiff(),
                    new Bmp()
                };

        public static List<string> FormatsExtensions => Formats.Select(f => ((IFileFormat)f).ShortName).ToList();

        public static FileFormat FileFormat;
        public static PixelFormat PixelFormat;
        public static BitmapEncoder Encoder;

        private static double pixelMaxValue;
        private static double pixelMinValue;
        private static PixelArray Heightmap;
        private static PixelArray Mask;
        public static BackgroundWorker RenderWorker(Roads Roads)
        {
            Heightmap = new PixelArray(Project.ResolutionSquared);
            Mask = new PixelArray(Project.ResolutionSquared);

            pixelMaxValue = Math.Pow(2, PixelFormat.BitsPerPixel) - 1;

            pixelMinValue = PixelFormat.ToString() == "Gray32Float" ? float.MinValue : 0;
            //pixelMinValue = 0;

            BackgroundWorker bgw = new BackgroundWorker();

            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += (obj, arg) =>
            {
                for (int roadIndex = 0; roadIndex < Roads.All.Count - 1; roadIndex++)
                {
                    Roads.Road Road = Roads.All[roadIndex];
                    for (int nodeIndex = 0; nodeIndex < Road.Nodes.Count - 1; nodeIndex++)
                    {
                        renderSegment(Road.Nodes[nodeIndex], Road.Nodes[nodeIndex + 1]);
                    }
                    double perc = (double)roadIndex / Roads.All.Count * 100;
                    bgw.ReportProgress((int)perc, "Generating Segments..");
                }



                bgw.ReportProgress(-1, "Rendering Heightmap...");
                bgw.ReportProgress(100, "Rendering Heightmap Done!");
            };

            return bgw;
        }

        private static void renderSegment(Road.Node Start, Road.Node End, int thickness = 10)
        {
            double startColor = Start.NormalizedElevation * pixelMaxValue;
            double endColor = End.NormalizedElevation * pixelMaxValue;

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

                double newColor = startColor + colStep * i;

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

        private static void DrawPixel(int pixelIndex, double color)
        {
            if (pixelIndex >= 0 && pixelIndex < Project.ResolutionSquared)
            {
                Heightmap.Pixels[pixelIndex] = color;
                Heightmap.AddPixel(pixelIndex, color);
                Mask.Pixels[pixelIndex] = pixelMaxValue;
            }
        }

        private static void DrawPixel(int x, int y, double color) => DrawPixel(GetPixelIndex(x, y), color);
        private static void DrawPixel(Point s, double color) => DrawPixel(GetPixelIndex((int)s.X, (int)s.Y), color);

        public static int GetPixelIndex(int x, int y)
        {
            return y * Project.Resolution + x;
        }

        public static BitmapFrame RenderImage()
        {
            var bitmap = new WriteableBitmap(Project.Resolution, Project.Resolution, 96, 96, PixelFormat, null);



            Int32Rect rect = new Int32Rect(0, 0, Project.Resolution, Project.Resolution);

            int stride = Project.Resolution * (PixelFormat.BitsPerPixel / 8);

            Array pixels = Heightmap.GetPixels(PixelFormat);

            bitmap.WritePixels(rect, pixels, stride, 0);
            return BitmapFrame.Create(bitmap);
        }

        public static void Save(string FilePath)
        {
            var bitmap = new WriteableBitmap(Project.Resolution, Project.Resolution, 96, 96, PixelFormat, null);



            Int32Rect rect = new Int32Rect(0, 0, Project.Resolution, Project.Resolution);

            int stride = Project.Resolution * (PixelFormat.BitsPerPixel / 8);

            Array pixels = Heightmap.GetPixels(PixelFormat);


            bitmap.WritePixels(rect, pixels, stride, 0);
            var bmpf = BitmapFrame.Create(bitmap);
            Encoder.Frames.Clear();
            Encoder.Frames.Add(bmpf);
            using (var stream = System.IO.File.Create(FilePath))
                Encoder.Save(stream);
            Encoder = FileFormat.Encoder;
        }
    }
}
