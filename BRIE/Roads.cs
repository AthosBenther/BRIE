using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using BRIE.ClassExtensions;
using BRIE.Etc;
using BRIE.Types;

namespace BRIE
{
    public class RoadsCollection
    {
        private Extents? extents;

        public Extents Extents
        {
            get
            {
                if (extents != null) return extents;
                else
                {
                    List<Point> coords = Roads.SelectMany(Road => Road.Nodes, (Road, Node) =>
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


        public List<Road> Roads = new List<Road>();

        public bool ScaleToExtents = true;

        private double? _scaleX, _scaleY;

        public double ScaleX
        {
            get
            {
                _scaleX = _scaleX ?? Project.Size / Helpers.LongitudeToMeters(Extents.Width);
                return _scaleX.Value;
            }
        }

        public double ScaleY
        {
            get
            {
                _scaleY = _scaleY ?? Project.Size / Helpers.LongitudeToMeters(Extents.Height);
                return _scaleY.Value;
            }
        }



        public RoadsCollection(GeoJson geoJson)
        {
            geoJson.Features.ForEach(feature =>
            {
                bool ignore = geoJson.ignoreIds.Contains(feature.Properties.ID) || string.IsNullOrEmpty(feature.Properties.Highway);

                if (!ignore)
                {
                    Roads.Add(new Road(this, feature));
                }
            });

            var a = Extents;
        }
    }
    public class Road
    {
        private List<Node> _nodes;
        private Feature _feature;
        public string? Name
        {
            get
            {
                return _feature.Properties.Name;
            }
        }
        public int OsmID
        {
            get
            {
                return int.Parse(_feature.Properties.Osm_id);
            }
        }
        public List<Node> Nodes
        {
            get
            {
                if (_nodes == null)
                {
                    _nodes = _feature.Geometry.Coordinates.SelectMany(CoordinatesGroup => CoordinatesGroup, (CoordinatesGroup, Coordinate) => new Node(Coordinate, this)).ToList();
                }
                return _nodes;
            }
        }
        public List<Segment> _segments;
        public List<Segment> Segments
        {
            get
            {
                if (_segments == null || true)
                {
                    _segments = new List<Segment> { };
                    for (int i = 0; i < Nodes.Count - 1; i++)
                    {
                        _segments.Add(new Segment(Nodes[i], Nodes[i + 1]));
                    }
                    return _segments;
                }
                return _segments;
            }
        }
        public RoadsCollection Parent;
        public Extents Extents { get { return Parent.Extents; } }
        public bool ScaleToExtents => Parent.ScaleToExtents;

        public double ScaleX => Parent.ScaleX;
        public double ScaleY => Parent.ScaleY;




        //public Road(Roads parent, int osmID, string? name, List<List<double>> nodes)
        //{
        //    Parent = parent;
        //    OsmID = osmID;
        //    Name = name;

        //    Nodes = nodes.Select(n => new Node(n)).ToList();
        //}

        public Road(RoadsCollection parent, Feature feature)
        {
            Parent = parent;
            _feature = feature;
        }
    }
    public class Node
    {
        //long == X == WIDTH!
        //lat == Y == HEIGHT!

        public Road Parent;
        public int Width = 10;

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
        private Point? _position, _scaledPosition;
        public Point Position
        {
            get
            {
                _position = _position ?? new Point(Helpers.LongitudeToMeters(NormalCoordinate.X), Helpers.LatitudeToMeters(NormalCoordinate.Y));
                return _position.Value;
            }
        }

        public Point ScaledPosition
        {
            get
            {
                _scaledPosition = _scaledPosition ?? new Point(Helpers.LongitudeToMeters(NormalCoordinate.X) * Parent.ScaleX, Helpers.LatitudeToMeters(NormalCoordinate.Y) * Parent.ScaleY);
                return _scaledPosition.Value;
            }
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
                if (_polygon == null || forceNew) _polygon = GetPolygon(ScaledPosition.FlipY(), Width/2);
                return _polygon;
            }
        }

        #region Constructors

        public Node(List<double> coordinates, Road parent)
        {
            Coordinate = new Point(coordinates[0], coordinates[1]);
            Elevation = coordinates[2];
            Parent = parent;
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

        private Polygon _polygon;
        public Polygon Polygon
        {
            get
            {
                bool forceNew = false;
                if (_polygon == null || forceNew) _polygon = GetPolygon();
                return _polygon;
            }
        }

        public double Height { get => Polygon.Height; }
        public double Width { get => Polygon.Width; }
        public double Left { get => Polygon.Left; }
        public double Top { get => Polygon.Top; }
        public double Right { get => Polygon.Right; }
        public double Bottom { get => Polygon.Bottom; }

        private double _lenght;
        public double Lenght { get => (Start.ScaledPosition - End.ScaledPosition).Length; }

        public Segment(Node Start, Node End)
        {
            _start = Start;
            _end = End;
        }

        private Polygon GetPolygon()
        {
            Polygon poly = new Polygon();

            Vector vector = (_start.ScaledPosition.FlipY() - _end.ScaledPosition.FlipY());

            Line startPerp = Math2.Trigonometry.GetPerpendicular(_start.ScaledPosition.FlipY(), vector.Angle(), _start.Width);
            Line endPerp = Math2.Trigonometry.GetPerpendicular(_end.ScaledPosition.FlipY(), vector.Angle(), _start.Width);

            poly.Points.Add(startPerp.Start);
            poly.Points.Add(startPerp.End);
            poly.Points.Add(endPerp.End);
            poly.Points.Add(endPerp.Start);

            return poly;
        }
    }
}
