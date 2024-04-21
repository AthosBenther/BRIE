using System;
using System.Windows;
using BRIE.Types.Geometry;

namespace BRIE.Classes.Etc
{
    public static class Math2
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

        public static class Trigonometry
        {
            public static class RightTriangle
            {
                public static Point GetCatheti(double hipotenuse, double angleDegrees)
                {
                    double angleRadians = angleDegrees * Math.PI / 180.0;
                    double x = hipotenuse * Math.Cos(angleRadians);
                    double y = hipotenuse * Math.Sin(angleRadians);
                    return new Point(x, y);
                }
            }

            public static double GetNormal(Point startPoint, Point endPoint)
            {
                // Calculate the direction vector from startPoint to endPoint
                double dx = endPoint.X - startPoint.X;
                double dy = endPoint.Y - startPoint.Y;

                // Calculate the angle using arctan(dy / dx)
                double angleInRadians = Math.Atan2(dy, dx);

                // Convert the angle from radians to degrees and ensure it's between 0 and 360 degrees
                double angleInDegrees = (angleInRadians * 180.0 / Math.PI + 360) % 360;

                return angleInDegrees;
            }

            public static Line GetPerpendicular(Point center, double normal, double length)
            {
                // Calculate the endpoint offsets from the center point based on the normal angle
                double dx = Math.Sin(normal * Math.PI / 180) * (length / 2);
                double dy = Math.Cos(normal * Math.PI / 180) * (length / 2);

                // Calculate the start and end points of the perpendicular line
                Point startPoint = new Point(center.X - dx, center.Y + dy);
                Point endPoint = new Point(center.X + dx, center.Y - dy);

                // Create the line
                Line perpendicular = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = endPoint.X,
                    Y2 = endPoint.Y
                };

                return perpendicular;
            }
        }

    }
}
