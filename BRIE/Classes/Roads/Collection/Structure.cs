using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using BRIE.Classes.Etc;
using BRIE.Classes.Extensions;
using BRIE.Classes.Statics;
using BRIE.Types.Geographics;
using Line = BRIE.Types.Geometry.Line;
using Point = System.Windows.Point;
using Polygon = BRIE.Types.Geometry.Polygon;

namespace BRIE.Classes.Roads.Collection
{
    public class Road
    {
        public string? Name { get; set; }
        public string ID { get; set; }


        private ObservableCollection<Node> _nodes;
        public ObservableCollection<Node> Nodes
        {
            get => _nodes;
            set => _nodes = value;
        }
        public ObservableCollection<Segment> _segments;
        public ObservableCollection<Segment> Segments
        {
            get
            {
                bool forceNew = true;
                if (_segments == null || forceNew)
                {
                    _segments = new ObservableCollection<Segment>();
                    for (int i = 0; i < Nodes.Count - 1; i++)
                    {
                        var segment = new Segment(Nodes[i], Nodes[i + 1], Nodes[i].Parent);
                        _segments.Add(segment);
                    }
                    return _segments;
                }
                return _segments;
            }
        }

        private List<Shape> _shapes;

        public List<Shape> Shapes
        {
            get
            {
                bool forceNew = true;
                if (_shapes == null || forceNew)
                {
                    _shapes = Segments.Select(s => s.Shape).ToList();
                }
                return _shapes;
            }
            set { _shapes = value; }
        }


        public Extents Extents { get { return RoadsCollection.Extents; } }
        public bool ScaleToExtents => RoadsCollection.ScaleToExtents;

        public double ScaleX => RoadsCollection.ScaleX;
        public double ScaleY => RoadsCollection.ScaleY;

        public Road() { }
        public Road(ObservableCollection<Node> nodes) => _nodes = nodes;
    }
    public class Node
    {
        //long == X == WIDTH!
        //lat == Y == HEIGHT!

        public Road Parent;
        public double Width = 10;

        #region Coordinates
        private Point coordinate;


        public Point Coordinate
        {
            get { return coordinate; }
            set { coordinate = value; }
        }



        public Point NormalCoordinate
        {
            get { return new Point(Coordinate.X - Parent.Extents.West, Coordinate.Y - Parent.Extents.South); }
        }
        #endregion

        #region Metric
        private double elevation;




        /// <summary>
        /// Horizontal position on earth in Meters
        /// </summary>
        private Point? _position;
        public Point Position
        {
            get
            {
                _position = _position ?? new Point(Helpers.LongitudeToMeters(NormalCoordinate.X) * Parent.ScaleX, Helpers.LatitudeToMeters(NormalCoordinate.Y) * Parent.ScaleY);
                return _position.Value;
            }
        }


        [Obsolete("ScaledPosition is deprecated, please use Position instead.", true)]
        public Point ScaledPosition
        {
            get => Position;
        }

        /// <summary>
        /// Node elevation in Meters
        /// </summary>
        public double Elevation
        {
            get { return elevation; }
            set { elevation = value; }
        }

        #region Normalaized
        public double NormalizedElevation
        {
            get { return (elevation - Project.TerrainElevationMin) / Project.TerrainElevationDelta; }
        }
        #endregion

        #endregion

        private Polygon _polygon;
        public Polygon Polygon
        {
            get
            {
                bool forceNew = false;
                if (_polygon == null || forceNew) _polygon = GetPolygon(Position.FlipY(), Width / 2);
                return _polygon;
            }
        }

        #region Constructors

        public Node(Point coordinate, double elevation, double width, Road parent)
        {
            Coordinate = coordinate;
            Elevation = elevation;
            Parent = parent;
            Width = width;
        }
        #endregion

        #region Functions


        public static Polygon GetPolygon(Point center, double radius, int numSides = 16)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive");
            }

            if (numSides <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numSides), "Number of sides must be positive");
            }

            Polygon circle = new Polygon();

            double angleIncrement = 2 * Math.PI / numSides;

            for (int i = 0; i < numSides; i++)
            {
                double angle = i * angleIncrement;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                circle.Points.Add(new Point(x, y));
            }

            // Close the polygon (assuming Points collection allows duplicates)
            circle.Points.Add(circle.Points[0]);

            return circle;
        }
        #endregion
    }
    public class Segment
    {
        private Node _start, _end;
        public Node Start { get { return _start; } }
        public Node End { get { return _end; } }

        private Road _parent;
        public Road Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }



        private Polygon _polygon;
        public Polygon Polygon
        {
            get
            {
                bool forceNew = true;
                if (_polygon == null || forceNew) _polygon = GetPolygon();
                return _polygon;
            }
        }


        private Shape _shape;
        public Shape Shape
        {
            get
            {
                bool forceNew = true;
                if (_shape == null || forceNew) _shape = GetShape();
                return _shape;
            }
            set { _shape = value; }
        }


        public double Height { get => Polygon.Height; }
        public double Width { get => Polygon.Width; }
        public double Left { get => Polygon.Left; }
        public double Top { get => Polygon.Top; }
        public double Right { get => Polygon.Right; }
        public double Bottom { get => Polygon.Bottom; }

        private double _lenght;
        public double Lenght { get => (Start.Position - End.Position).Length; }

        private double _angle;
        public double Angle { get => (End.Position - Start.Position).Angle(); }

        public Vector Vector => Start.Position - End.Position;


        public Segment(Node Start, Node End, Road parent)
        {
            _start = Start;
            _end = End;
            _parent = parent;
        }

        private Polygon GetPolygon()
        {
            Polygon poly = new Polygon();

            Vector vector = _start.Position.FlipY() - _end.Position.FlipY();

            Line startPerp = Math2.Trigonometry.GetPerpendicular(_start.Position.FlipY(), vector.Angle(), _start.Width);
            Line endPerp = Math2.Trigonometry.GetPerpendicular(_end.Position.FlipY(), vector.Angle(), _start.Width);

            poly.Points.Add(startPerp.Start);
            poly.Points.Add(startPerp.End);
            poly.Points.Add(endPerp.End);
            poly.Points.Add(endPerp.Start);
            //poly.Points.Add(startPerp.Start);

            return poly;
        }

        private Shape GetShape()
        {
            var segmentShape = Polygon.ToSPoly() as Shape;


            var line = new LineSegment((Point)Vector, true);



            byte grayStart = (byte)(Start.NormalizedElevation * byte.MaxValue);
            Color colorStart = new Color()
            {
                R = grayStart,
                G = grayStart,
                B = grayStart,
                A = byte.MaxValue
            };

            byte grayEnd = (byte)(End.NormalizedElevation * byte.MaxValue);
            Color colorEnd = new Color()
            {
                R = grayEnd,
                G = grayEnd,
                B = grayEnd,
                A = byte.MaxValue
            };

            bool isUphill = Start.Elevation < End.Elevation;

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
            lgb.StartPoint = Start.Position.FlipY();
            lgb.EndPoint = End.Position.FlipY();

            //lgb.GradientStops.Add(new GradientStop(colorStart, 0.0));
            //lgb.GradientStops.Add(new GradientStop(colorEnd, 1.0));


            segmentShape.Fill = lgb;

            return segmentShape;
        }
    }
}
