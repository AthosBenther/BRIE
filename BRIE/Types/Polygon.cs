
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using ShapesPolygon = System.Windows.Shapes.Polygon;

namespace BRIE.Types
{
    public class Polygon
    {
        public ObservableCollection<Point> Points = new ObservableCollection<Point>();

        private double _height, _width, _left, _top, _right, _bottom;
        public double Height { get => _height; }
        public double Width { get => _width; }
        public double Left { get => _left; }
        public double Top { get => _top; }
        public double Right { get => _right; }
        public double Bottom { get => _bottom; }

        public Polygon()
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetSizes();
        }

        private void SetSizes()
        {

            double minX = Points[0].X;
            double minY = Points[0].Y;
            double maxX = Points[0].X;
            double maxY = Points[0].Y;

            // Find minimum and maximum x and y coordinates
            foreach (Point point in Points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            _left = minX;
            _top = maxY;
            _right = maxX;
            _bottom = minY;

            // Calculate width and height
            _width = maxX - minX;
            _height = maxY - minY;
        }

        private ShapesPolygon ToShapesPolygon()
        {
            ShapesPolygon spoly = new ShapesPolygon();
            Points.ToList().ForEach(p => spoly.Points.Add(p));
            return spoly;
        }
    }
}
