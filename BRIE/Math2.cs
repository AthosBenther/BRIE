using System;
using System.Windows;

namespace BRIE
{
    internal class Math2
    {
        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static Point getPointsCenter(Point p1, Point p2)
        {
            double centerX = (p1.X + p2.X) / 2;
            double centerY = (p1.Y + p2.Y) / 2;

            return new Point(centerX, centerY);
        }

        public static double CalculatePolylineAngle(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Atan2(dy, dx) * (180 / Math.PI);
        }

        public static double DistanceBetweenPoints(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
