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
    }
}
