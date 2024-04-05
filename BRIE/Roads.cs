using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BRIE.Etc;
using BRIE.Types;

namespace BRIE
{
    public class Roads
    {
        private Extents? extents;

        public Extents Extents
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


        public List<Road> All = new List<Road>();

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



        public Roads(GeoJson geoJson)
        {
            geoJson.Features.ForEach(feature =>
            {
                bool ignore = geoJson.ignoreIds.Contains(feature.Properties.ID) || string.IsNullOrEmpty(feature.Properties.Highway);

                if (!ignore)
                {
                    All.Add(new Road(this, feature));
                }
            });

            var a = Extents;
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
            public Roads Parent;
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

            public Road(Roads parent, Feature feature)
            {
                Parent = parent;
                _feature = feature;
            }
            public class Node
            {
                //long == X == WIDTH!
                //lat == Y == HEIGHT!

                public Road Parent;

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

                #region Constructors

                public Node(List<double> coordinates, Road parent)
                {
                    Coordinate = new Point(coordinates[0], coordinates[1]);
                    Elevation = coordinates[2];
                    Parent = parent;
                }
                #endregion

                #region Functions



                #endregion
            }
        }
    }
}
