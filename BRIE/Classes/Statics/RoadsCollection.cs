using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using BRIE.Classes.Etc;
using BRIE.Classes.Roads.Collection;
using BRIE.Types.Geographics;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace BRIE.Classes.Statics
{
    public static class RoadsCollection
    {
        private static Extents? extents;

        public static Extents Extents
        {
            get
            {
                if (extents != null) return extents;
                else
                {
                    List<Point> coords = All.SelectMany(Road => Road.Nodes, (Road, Node) =>
                    {
                        return Node.Coordinate;
                    }).ToList();

                    List<double> Xs = coords.Select(coord => coord.X).ToList();
                    List<double> Ys = coords.Select(coord => coord.Y).ToList();

                    double minX = Xs.Min();
                    double minY = Ys.Min();

                    double maxX = Xs.Max();
                    double maxY = Ys.Max();

                    extents = new Extents(minX, maxY, maxX, minY);
                    return extents;
                }
            }
        }


        public static List<Road> All = new List<Road>();

        public static bool ScaleToExtents = true;

        private static double? _scaleX, _scaleY;

        public static double ScaleX
        {
            get
            {
                _scaleX = _scaleX ?? Project.Size / Helpers.LongitudeToMeters(Extents.Width);
                return _scaleX.Value;
            }
        }

        public static double ScaleY
        {
            get
            {
                _scaleY = _scaleY ?? Project.Size / Helpers.LongitudeToMeters(Extents.Height);
                return _scaleY.Value;
            }
        }

        public static List<Shape> GetRoadsShapes()
        {
            List<Shape> shapes = new List<Shape>();

            foreach (Road road in All)
            {
                List<Shape> nodes = road.Nodes.Select(node =>
                {
                    var n = node.Polygon.ToSPoly() as Shape;

                    byte c = (byte)(node.NormalizedElevation * byte.MaxValue);

                    Color color = new Color()
                    {
                        R = c,
                        G = c,
                        B = c,
                        A = byte.MaxValue
                    };

                    n.Fill = new SolidColorBrush(color);
                    return n;
                }).ToList();

                //shapes.AddRange(nodes);

                shapes.AddRange(road.Segments.Select(segment =>
                {
                    var seg = segment.Polygon.ToSPoly() as Shape;


                    var line = new LineSegment((Point)segment.Vector, true);



                    byte grayStart = (byte)(segment.Start.NormalizedElevation * byte.MaxValue);
                    Color colorStart = new Color()
                    {
                        R = grayStart,
                        G = grayStart,
                        B = grayStart,
                        A = byte.MaxValue
                    };

                    byte grayEnd = (byte)(segment.End.NormalizedElevation * byte.MaxValue);
                    Color colorEnd = new Color()
                    {
                        R = grayEnd,
                        G = grayEnd,
                        B = grayEnd,
                        A = byte.MaxValue
                    };

                    bool isUphill = segment.Start.Elevation < segment.End.Elevation;

                    //colorStart = Brushes.Red.Color;
                    //colorEnd = Brushes.Blue.Color;

                    LinearGradientBrush lgb = new LinearGradientBrush();


                    lgb.GradientStops.Add(new GradientStop(colorStart, 0));
                    //lgb.GradientStops.Add(new GradientStop(colorStart, 0.4));
                    //lgb.GradientStops.Add(new GradientStop(colorEnd, 0.5));
                    lgb.GradientStops.Add(new GradientStop(colorEnd, 1));

                    //lgb.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;

                    lgb.MappingMode = BrushMappingMode.Absolute;
                    //lgb.SpreadMethod = GradientSpreadMethod.Repeat;
                    lgb.StartPoint = segment.Start.Position.FlipY();
                    lgb.EndPoint = segment.End.Position.FlipY();

                    //lgb.GradientStops.Add(new GradientStop(colorStart, 0.0));
                    //lgb.GradientStops.Add(new GradientStop(colorEnd, 1.0));


                    seg.Fill = lgb;

                    return seg;
                }).ToList());

            }




            return shapes;
        }
    }
}
