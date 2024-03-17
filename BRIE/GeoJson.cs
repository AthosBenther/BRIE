using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace BRIE
{
    public class GeoJson
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Crs Crs { get; set; }
        public List<Feature> Features { get; set; }
        public List<Polyline>? Polylines { get; set; }

        //West
        public double minLat = 90;
        //East double 
        public double maxLat = -90;
        //Northdouble 
        public double minLong = 180;
        //Southdouble 
        public double maxLong = -180;

        public double minH = 10000;
        public double maxH = 0;

        //public double minQgisH = 921.62616;
        //public double maxQgisH = 1005.696167;

        public double minQgisH = 489;
        public double maxQgisH = 1302;
        public double diffQgisH = 0;

        public List<int> ignoreIds = new List<int> { -1, 5, 6 };

        public double diffLat, diffLong, diffLatM, diffLongM, diffH, scaleX, scaleY;

        public int MapSize;

        private Canvas Canvas;

        public GeoJson()
        {

        }
        public GeoJson(string FileName, Canvas canvas)
        {
            var json = JsonConvert.DeserializeObject<GeoJson>(System.IO.File.ReadAllText(FileName));
            Type = json.Type;
            Name = json.Name;
            Crs = json.Crs;
            Features = json.Features;
            Canvas = canvas;

            MinMax();
            SetSizes();
        }

        private void SetSizes()
        {
            MapSize = (int)Canvas.Height;
            scaleX = MapSize / diffLongM;
            scaleY = MapSize / diffLatM;
        }

        public List<Rectangle> GetRects()
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            SetSizes();

            foreach (Feature feature in Features)
            {
                PointCollection points = new PointCollection();

                bool ignore = ignoreIds.Contains(feature.Properties.ID) || feature.Properties.Highway == null;

                if (!ignore)
                {
                    foreach (var Coordinates in feature.Geometry.Coordinates)
                    {


                        List<List<double>> Points = Coordinates.Select(c =>
                        {

                            double longitude = c[0];
                            double latitude = c[1];
                            double? heigh = c.Count == 3 ? c[2] : null;


                            double normalLat = latitude - minLat;
                            double normalLong = longitude - minLong;

                            double metersLong = LongitudeToMeters(normalLong);
                            double metersLat = LatitudeToMeters(normalLat);



                            double scaledX = metersLong * scaleX;
                            double scaledY = metersLat * scaleY;

                            scaledY = (scaledY * -1) + MapSize;

                            return new List<double> { scaledX, scaledY, c[2] };
                        }).ToList();



                        for (int i = 0; i < Points.Count - 1; i++)
                        {
                            Point p1 = new Point(Points[i][0], Points[i][1]);
                            double p1h = Points[i][2];
                            double normalP1h = (p1h - minQgisH) / diffQgisH;
                            Point p2 = new Point(Points[i + 1][0], Points[i + 1][1]);
                            double normalP2h = (Points[i + 1][2] - minQgisH) / diffQgisH;


                            // Calculate the length of the segment
                            double length = Math2.DistanceBetweenPoints(p1, p2);

                            // Calculate the angle of the segment
                            double angle = Math2.CalculatePolylineAngle(p1, p2);

                            // Calculate the center point of the segment
                            Point center = Math2.getPointsCenter(p1, p2);

                            // Create a rotation transform
                            RotateTransform rotation = new RotateTransform(angle - 90);

                            // Create the rectangle
                            Rectangle rectangle = new Rectangle
                            {
                                Width = 10,
                                Height = length
                            };

                            // Apply rotation to the rectangle
                            rectangle.RenderTransform = rotation;

                            // Calculate the position of the rectangle
                            double x = center.X - rectangle.Width / 2;
                            double y = center.Y - length / 2;

                            // Set the position of the rectangle
                            Canvas.SetLeft(rectangle, p1.X);
                            Canvas.SetTop(rectangle, p1.Y);

                            LinearGradientBrush gradientBrush = new LinearGradientBrush();

                            Color color1 = new Color();
                            color1.A = 255;
                            color1.ScR = color1.ScG = color1.ScB = (float)normalP1h;

                            Color color2 = new Color();
                            color2.A = 255;
                            color2.ScR = color2.ScG = color2.ScB = (float)normalP2h;

                            // Define gradient stops
                            gradientBrush.GradientStops.Add(new GradientStop(color1, 0.0));
                            gradientBrush.GradientStops.Add(new GradientStop(color2, 1.0));

                            // Set the rectangle's fill to the gradient brush
                            rectangle.Fill = gradientBrush;

                            ToolTip ttp = new ToolTip();
                            ttp.Content = feature.Properties.Osm_id + "\n" + feature.Properties.Name;
                            rectangle.ToolTip = ttp;


                            rectangle.MouseEnter += (sender, e) => { (sender as Rectangle).Fill = Brushes.Red; };
                            rectangle.MouseLeave += (sender, e) => { (sender as Rectangle).Fill = gradientBrush; };
                            rectangle.MouseDown += (sender, e) => { Clipboard.SetText(((sender as Rectangle).ToolTip as ToolTip).Content as string); };

                            rectangles.Add(rectangle);
                        }
                    }
                }
            }

            return rectangles;
        }

        public List<Polyline> GetPolys()
        {
            List<Polyline> Polys = new List<Polyline>();

            SetSizes();

            foreach (Feature feature in Features)
            {
                Polyline poly = new Polyline();
                poly.Stroke = Brushes.Black;
                poly.StrokeThickness = 1;
                PointCollection Points = new PointCollection();

                bool ignore = ignoreIds.Contains(feature.Properties.ID) || feature.Properties.Highway == null;

                if (!ignore)
                {
                    feature.Geometry.GetPointsCollections(this).ForEach(lp => lp.ToList().ForEach(p => Points.Add(p)));
                }

                poly.Points = Points;
                Polys.Add(poly);
            }

            return Polys;
        }

        public List<Path> GetPaths()
        {
            List<Path> Paths = new List<Path>();
            Path path = new Path();

            SetSizes();

            foreach (Feature feature in Features)
            {
                string osm = feature.Properties.Osm_id;
                if (osm == "972309200")
                {
                    var b = "tbm nao sei mano";
                }
                bool ignore = ignoreIds.Contains(feature.Properties.ID) || feature.Properties.Highway == null;

                if (!ignore)
                {

                    path = new Path();
                    List<PointCollection> geoPoints = feature.Geometry.GetPointsCollections(this);

                    PathGeometry pG = new PathGeometry();

                    foreach (PointCollection pC in geoPoints)
                    {
                        PathFigure pf = new PathFigure();
                        pf.StartPoint = pC[0]; // Set the start point of the PathFigure

                        if (pC.Count >= 3)
                        {
                            PolyBezierSegment pbs = new PolyBezierSegment();
                            pbs.Points = pC;
                            pf.Segments.Add(pbs);
                        }
                        else if (pC.Count == 2)
                        {
                            LineSegment ls = new LineSegment();
                            ls.Point = pC[1];

                            PathSegmentCollection psc = new PathSegmentCollection();
                            psc.Add(ls);

                            pf.Segments = psc;
                        }
                        else
                        {
                            string alala = "sei lá mano";
                        }



                        // Add the PathFigure to the PathGeometry
                        pG.Figures.Add(pf);
                    }



                    path.Data = pG;
                    path.Stroke = Brushes.Black;
                    path.StrokeThickness = 2;


                    ToolTip ttp = new ToolTip();
                    ttp.Content = feature.Properties.Osm_id + "\n" + feature.Properties.Name;
                    path.ToolTip = ttp;

                    path.IsMouseDirectlyOverChanged += (sender, e) =>
                    {
                        if ((bool)e.NewValue == true)
                        {
                            (sender as Path).Stroke = Brushes.Red;
                            (sender as Path).StrokeThickness = 3;
                        }
                        else
                        {

                            (sender as Path).Stroke = Brushes.Black;
                            (sender as Path).StrokeThickness = 2;
                        }
                    };
                    path.MouseDown += (sender, e) =>
                    {
                        Clipboard.SetText(feature.Properties.Osm_id);
                    };

                    Paths.Add(path);
                }
            }
            
            return Paths;
        }

        private void MinMax()
        {
            foreach (Feature feature in Features)
            {
                bool ignore = ignoreIds.Contains(feature.Properties.ID) || feature.Properties.Highway == null;

                if (!ignore)
                {
                    foreach (var coordinates in feature.Geometry.Coordinates)
                    {
                        foreach (var coord in coordinates)
                        {
                            double longCoord = coord[0];
                            double latCoord = coord[1];
                            double height = coord[2];

                            // Update min and max values
                            minLat = (latCoord < minLat) ? latCoord : minLat;
                            maxLat = (latCoord > maxLat) ? latCoord : maxLat;
                            minLong = (longCoord < minLong) ? longCoord : minLong;
                            maxLong = (longCoord > maxLong) ? longCoord : maxLong;
                            minH = (height < minH) ? height : minH;
                            maxH = (height > maxH) ? height : maxH;
                        }
                    }
                }
            }

            diffLat = maxLat - minLat;
            diffLong = maxLong - minLong;
            diffH = maxH - minH;
            diffQgisH = maxQgisH - minQgisH;

            diffLatM = LatitudeToMeters(diffLat);
            diffLongM = LongitudeToMeters(diffLong);
        }

        public double LatitudeToMeters(double lat)
        {
            // Earth's radius in meters
            int earthRadius = 6371000;

            // Convert latitude from degrees to radians

            double latitudeInRadians = Math2.DegreesToRadians(lat);

            // Calculate the distance in meters using Haversine formula
            double distanceInMeters = earthRadius * latitudeInRadians;

            return distanceInMeters;
        }

        public double LongitudeToMeters(double lon)
        {
            // Earth's radius in meters
            double earthRadius = 6371000;

            // Convert longitude from degrees to radians
            double longitudeInRadians = lon * (Math.PI / 180.0);

            // Calculate the distance in meters using Haversine formula
            double distanceInMeters = earthRadius * longitudeInRadians * Math.Cos(Math2.DegreesToRadians(0));

            return distanceInMeters;
        }


    }

    public class CrsProperties
    {
        public string Name { get; set; }
    }

    public class Crs
    {
        public string Type { get; set; }
        public CrsProperties Properties { get; set; }
    }

    public class Properties
    {
        public string Osm_id { get; set; }
        public object Name { get; set; }
        public string Highway { get; set; }
        public object Waterway { get; set; }
        public object Aerialway { get; set; }
        public object Barrier { get; set; }
        public object ManMade { get; set; }
        public object Railway { get; set; }
        public int ZOrder { get; set; }
        public string OtherTags { get; set; }
        public string GTTRoad { get; set; }
        public int ID { get; set; }
        public int ORDER { get; set; }
    }

    public class Geometry
    {
        public string Type { get; set; }
        public List<List<List<double>>> Coordinates { get; set; }

        public List<PointCollection> GetPointsCollections(GeoJson geoJson)
        {
            List<PointCollection> points = new List<PointCollection>();
            foreach (var C in Coordinates)
            {
                PointCollection pointColl = new PointCollection();
                foreach (var c in C)
                {
                    double longitude = c[0];
                    double latitude = c[1];
                    double? heigh = c.Count == 3 ? c[2] : null;


                    double normalLat = latitude - geoJson.minLat;
                    double normalLong = longitude - geoJson.minLong;

                    double metersLong = geoJson.LongitudeToMeters(normalLong);
                    double metersLat = geoJson.LatitudeToMeters(normalLat);



                    double scaledX = metersLong * geoJson.scaleX;
                    double scaledY = metersLat * geoJson.scaleY;

                    scaledY = (scaledY * -1) + geoJson.MapSize;

                    pointColl.Add(new Point(scaledX, scaledY));
                }
                points.Add(pointColl);
            }
            return points;
        }
    }

    public class Feature
    {
        public string Type { get; set; }
        public Properties Properties { get; set; }
        public Geometry Geometry { get; set; }
    }

    public class FeatureCollection
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Crs Crs { get; set; }
        public List<Feature> Features { get; set; }
    }
}
