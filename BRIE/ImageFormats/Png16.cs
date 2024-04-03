using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using BRIE.Etc;
using ImageMagick;
using static BRIE.Roads;

namespace BRIE.ImageFormats
{
    internal static class Png16
    {
        public static bool ToPng16Parallel(this Roads roads, string FilePath)
        {



            MagickImage image = new MagickImage(new MagickColor(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, 0), Project.Resolution, Project.Resolution);

            Drawables drawables = new Drawables();



            List<BackgroundWorker> workers = new List<BackgroundWorker>();
            foreach (Road road in roads.All)
            {
                for (int nodeIndex = 0; nodeIndex < road.Nodes.Count - 2; nodeIndex++)
                {
                    BackgroundWorker worker = new BackgroundWorker();

                    worker.DoWork += (a, b) =>
                    {
                        Road.Node node0 = road.Nodes[nodeIndex];
                        Road.Node node1 = road.Nodes[nodeIndex + 1];


                        ushort color0 = (ushort)(node0.NormalizedElevation * ushort.MaxValue);
                        MagickColor color = new(color0, color0, color0);

                        RoadSegment rS = new RoadSegment(node0.ScaledPosition.FlipY(), node1.ScaledPosition.FlipY(), 10);
                        drawables = new Drawables();

                        drawables.FillColor(color).Polygon(rS.Shape);

                        try
                        {
                            image.Draw(drawables);
                        }
                        catch (Exception ex)
                        {

                            //throw;
                        }

                    };
                    workers.Add(worker);
                }

            }
            bool workersBusy = true;
            workers.ForEach(worker =>
            {

                worker.RunWorkerCompleted += (a, b) =>
                {
                    workersBusy = workers.Any(worker => worker.IsBusy);
                    if (!workersBusy)
                    {
                        image.WriteAsync(FilePath);
                    }
                    workers.Remove(worker);
                };
                worker.RunWorkerAsync();
            });
            while (workersBusy) ;
            return true;
        }

        public static BackgroundWorker ToPng16Worker(this Roads roads, string FilePath)
        {
            BackgroundWorker bgw = new();
            bgw.WorkerReportsProgress = true;
            bgw.DoWork += (a, b) =>
            {
                MagickImage image = new MagickImage(new MagickColor(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, 0), Project.Resolution, Project.Resolution);

                Drawables drawables = new Drawables();
                List<Road> allRoads = roads.All.Take(3).ToList();
                foreach (Road road in allRoads)
                {
                    int allRoadsCount = allRoads.Count();
                    for (int nodeIndex = 0; nodeIndex < road.Nodes.Count - 1; nodeIndex++)
                    {

                        Road.Node node0 = road.Nodes[nodeIndex];
                        Road.Node node1 = road.Nodes[nodeIndex + 1];

                        int roadNodesCount = road.Nodes.Count();



                        ushort color0 = (ushort)(node0.NormalizedElevation * ushort.MaxValue);
                        ushort color1 = (ushort)(node1.NormalizedElevation * ushort.MaxValue);
                        int subdivisionsQty = Math.Abs(color0 - color1);
                        MagickColor mcolor0 = new(color0, color0, color0);
                        MagickColor mcolor1 = new(color1, color1, color1);

                        RoadSegment rS = new RoadSegment(node0.ScaledPosition.FlipY(), node1.ScaledPosition.FlipY(), 10);
                        double subdivisionLenght = rS.Direction.Length / subdivisionsQty;

                        subdivisionsQty = rS.Direction.Length < subdivisionsQty ? (int)rS.Direction.Length + 1: subdivisionsQty;
                        subdivisionLenght = rS.Direction.Length < subdivisionsQty ? 1 : subdivisionsQty;
                        ushort subdivisionsQtyu = (ushort)subdivisionsQty;
                        Point n0 = node0.ScaledPosition.FlipY();
                        Point n1 = node1.ScaledPosition.FlipY();
                        for (ushort i = subdivisionsQtyu; i > 1; i--)
                        {
                            double perc = i / (double)subdivisionsQty;
                            ushort col = (ushort)(color0 + i);
                            MagickColor mcol = new(col, col, col);

                            Point nS = new(n1.X,n1.Y);

                            nS.X *= perc;
                            nS.Y *= perc;

                            RoadSegment subdiv = new RoadSegment(n0, nS, 10);

                            drawables = new Drawables();

                            drawables.FillColor(mcol).Polygon(subdiv.Shape);
                            image.Draw(drawables);
                            string percs = (perc * 100).ToString("###") + "%";
                            string roadName = string.IsNullOrWhiteSpace(road.Name) ? road.Name : road.OsmID.ToString();
                            double roadI = (double)roads.All.IndexOf(road);
                            double roadsPerc = (( roadI / allRoadsCount) * 100);
                            bgw.ReportProgress((int)roadsPerc, $"{roadName}({roadI}/{allRoadsCount}): Node {nodeIndex} / {roadNodesCount} | Subdivision {i} / {subdivisionsQty} | {percs}");
                        }
                    }

                }

                image.WriteAsync(FilePath);
            };
            return bgw;
        }

        public static void ToPng16(this Roads roads, string FilePath)
        {
            MagickImage image = new MagickImage(new MagickColor(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, 0), Project.Resolution, Project.Resolution);

            Drawables drawables = new Drawables();

            foreach (Road road in roads.All)
            {
                for (int nodeIndex = 0; nodeIndex < road.Nodes.Count - 1; nodeIndex++)
                {

                    Road.Node node0 = road.Nodes[nodeIndex];
                    Road.Node node1 = road.Nodes[nodeIndex + 1];


                    ushort color0 = (ushort)(node0.NormalizedElevation * ushort.MaxValue);
                    ushort color1 = (ushort)(node1.NormalizedElevation * ushort.MaxValue);
                    int subdivisionsQty = Math.Abs(color0 - color1);
                    MagickColor mcolor0 = new(color0, color0, color0);
                    MagickColor mcolor1 = new(color1, color1, color1);

                    RoadSegment rS = new RoadSegment(node0.ScaledPosition.FlipY(), node1.ScaledPosition.FlipY(), 10);
                    double subdivisionLenght = rS.Direction.Length / subdivisionsQty;
                    for (ushort i = 1; i < subdivisionsQty; i++)
                    {
                        ushort col = (ushort)(color0 + i);
                        MagickColor mcol = new(col, col, col);

                        Point n1 = node1.ScaledPosition.FlipY();

                        n1.X *= i / subdivisionsQty;
                        n1.Y *= i / subdivisionsQty;

                        RoadSegment subdiv = new RoadSegment(node0.ScaledPosition.FlipY(), node1.ScaledPosition.FlipY(), 10);

                        drawables = new Drawables();

                        drawables.FillColor(mcol).Polygon(subdiv.Shape);
                        image.Draw(drawables);
                    }




                }

            }

            image.WriteAsync(FilePath);
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
