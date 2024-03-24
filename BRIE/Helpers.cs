using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace BRIE
{
    public static class Helpers
    {
        public static double EarthRadius = 6371000;
        public static PointCollection GetAllPoints(this Path path)
        {
            PointCollection points = new PointCollection();

            if (path != null && path.Data != null && path.Data is PathGeometry)
            {
                PathGeometry pathGeometry = path.Data as PathGeometry;

                foreach (PathFigure figure in pathGeometry.Figures)
                {
                    points.Add(figure.StartPoint); // Add the starting point of the figure

                    foreach (PathSegment segment in figure.Segments)
                    {
                        if (segment is LineSegment)
                        {
                            LineSegment lineSegment = segment as LineSegment;
                            points.Add(lineSegment.Point); // Add the end point of the line segment
                        }
                        else if (segment is PolyLineSegment)
                        {
                            PolyLineSegment polyLineSegment = segment as PolyLineSegment;
                            foreach (Point point in polyLineSegment.Points)
                            {
                                points.Add(point); // Add each point in the poly-line segment
                            }
                        }
                        else if (segment is BezierSegment)
                        {
                            BezierSegment bezierSegment = segment as BezierSegment;
                            points.Add(bezierSegment.Point1);
                            points.Add(bezierSegment.Point2);
                            points.Add(bezierSegment.Point3);
                        }
                        else if (segment is QuadraticBezierSegment)
                        {
                            QuadraticBezierSegment quadraticBezierSegment = segment as QuadraticBezierSegment;
                            points.Add(quadraticBezierSegment.Point1);
                            points.Add(quadraticBezierSegment.Point2);
                        }
                        // Add handling for other types of segments like ArcSegment, etc. if needed
                    }
                }
            }

            return points;
        }


        public static string SanitizeFileName(this string fileName, string replacement = "_")
        {
            Regex removeInvalidChars = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))}]",
           RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

            return removeInvalidChars.Replace(fileName, replacement);

        }


        public static double LatitudeToMeters(double Latitude)
        {
            // Convert latitude from degrees to radians

            double latitudeInRadians = Math2.DegreesToRadians(Latitude);

            // Calculate the distance in meters using Haversine formula
            double distanceInMeters = EarthRadius * latitudeInRadians;

            return distanceInMeters;
        }

        public static double LongitudeToMeters(double Longitude)
        {
            // Convert longitude from degrees to radians
            double longitudeInRadians = Longitude * (Math.PI / 180.0);

            // Calculate the distance in meters using Haversine formula
            double distanceInMeters = EarthRadius * longitudeInRadians * Math.Cos(Math2.DegreesToRadians(0));

            return distanceInMeters;
        }
    }
}
