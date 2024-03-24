using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BRIE
{
    public class Roads
    {
        private Rect? extents;

        public Rect Extents
        {
            get
            {
                if (extents != null) return extents.Value;
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

                    extents = new Rect(new Point(maxX, maxY), new Point(minX, minY));
                    return extents.Value;
                }
            }
        }


        public List<Road> All = new List<Road>();


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
            public string? Name;
            public int OsmID;
            public List<Node> Nodes;
            public Roads Parent;
            public Rect Extents { get { return Parent.Extents; } }

            public Road(Roads parent, int osmID, string? name, List<List<double>> nodes)
            {
                Parent = parent;
                OsmID = osmID;
                Name = name;

                Nodes = nodes.Select(n => new Node(n)).ToList();
            }

            public Road(Roads parent, Feature feature)
            {
                Parent = parent;
                OsmID = int.Parse(feature.Properties.Osm_id);
                Name = feature.Properties.Name;

                Nodes = feature.Geometry.Coordinates.SelectMany(CoordinatesGroup => CoordinatesGroup, (CoordinatesGroup, Coordinate) => new Node(Coordinate)).ToList();

                //nodes.Select(n => new Node(n)).ToList();
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
                    get { return new Point(Coordinate.X - Parent.Extents.X, Coordinate.Y - Parent.Extents.Y); }
                }
                #endregion




                #region Metric
                private double elevation;


                /// <summary>
                /// Horizontal position on earth in Meters
                /// </summary>
                public Point Position
                {
                    get
                    {
                        return new Point(Helpers.LongitudeToMeters(NormalCoordinate.X), Helpers.LatitudeToMeters(NormalCoordinate.Y));
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

                #endregion

                #endregion

                #region Constructors

                public Node(List<double> coordinates)
                {
                    Coordinate = new Point(coordinates[0], coordinates[1]);
                    Elevation = coordinates[2];
                }
                #endregion

                #region Functions



                #endregion
            }
        }
    }
}
