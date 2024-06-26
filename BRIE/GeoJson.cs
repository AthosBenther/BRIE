﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BRIE.Etc;
using BRIE.ImageFormats;
using BRIE.Types;
using ImageMagick;
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
        //East 
        public double maxLat = -90;
        //South 
        public double minLong = 180;
        //North 
        public double maxLong = -180;

        public double minH = 10000;
        public double maxH = 0;

        //public double minQgisH = 921.62616;
        //public double maxQgisH = 1005.696167;

        public double minQgisH = 921.62616;
        public double maxQgisH = 1005.696167;
        public double diffQgisH = 84.070007;

        public double minAscH0 = 923.74810791015625;
        public double minAscHM = 0;
        public double minAscH = 0;

        public double maxAscH0 = 1005.6961669921875;
        public double maxAscHM = 0;
        public double maxAscH = 0;

        public double diffAscH = 0;


        public double roadWidth = 10;

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

            Roads r = new Roads(this);

            MinMax();
            SetSizes();

        }

        private void SetSizes()
        {
            MapSize = (int)Canvas.Height;
            scaleX = MapSize / diffLongM;
            scaleY = MapSize / diffLatM;

            minAscH = minAscH0 + minAscHM;
            maxAscH = maxAscH0 + maxAscHM;

            minAscH = minAscH0;
            maxAscH = maxAscH0;

            diffAscH = maxAscH - minAscH;
        }

        public List<Shape> getRoads()
        {
            List<Shape> shapes = new List<Shape>();

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

                            double metersLong = Helpers.LongitudeToMeters(normalLong);
                            double metersLat = Helpers.LatitudeToMeters(normalLat);



                            double scaledX = metersLong * scaleX;
                            double scaledY = metersLat * scaleY;

                            scaledY = (scaledY * -1) + MapSize;

                            return new List<double> { scaledX, scaledY, c[2] };
                        }).ToList();



                        for (int i = 0; i < Points.Count - 1; i++)
                        {
                            //Point p1 = new Point(Points[i][0], Points[i][1]);
                            //double p1h = Points[i][2];
                            //double scaledP1h = (p1h - minQgisH) / diffQgisH;
                            //Point p2 = new Point(Points[i + 1][0], Points[i + 1][1]);
                            //double scaledP2h = (Points[i + 1][2] - minQgisH) / diffQgisH;

                            Point p1 = new Point(Points[i][0], Points[i][1]);
                            double p1h = Points[i][2];
                            double normalP1h = p1h - minAscH;
                            double scaledP1h = normalP1h / (diffAscH);
                            Point p2 = new Point(Points[i + 1][0], Points[i + 1][1]);
                            double p2h = Points[i + 1][2];
                            double normalP2h = p2h - minAscH;
                            double scaledP2h = normalP2h / (diffAscH);


                            // Calculate the length of the segment
                            double length = Math2.DistanceBetweenPoints(p1, p2);

                            // Calculate the angle of the segment
                            double angle = Math2.CalculatePolylineAngle(p1, p2);

                            // Calculate the center point of the segment
                            Point center = Math2.getPointsCenter(p1, p2);

                            // Create a rotation transform
                            angle -= 90;
                            RotateTransform rotation = new RotateTransform(angle);

                            // Create the rectangle
                            Rectangle rectangle = new Rectangle
                            {
                                Width = roadWidth,
                                Height = length
                            };

                            Ellipse ellipse = new Ellipse
                            {
                                Width = roadWidth,
                                Height = roadWidth
                            };

                            // Set the position of the rectangle

                            // Apply rotation to the rectangle
                            rectangle.RenderTransform = rotation;
                            //ellipse.RenderTransform = rotation;

                            Point origin = Math2.Trigonometry.RightTriangle.GetCatheti(roadWidth, angle);
                            Point rectOrigin = new Point(p1.X - (origin.X / 2), p1.Y - (origin.Y / 2));
                            Point elliOrigin = new Point(
                                p1.X - roadWidth / 2,
                                p1.Y - roadWidth / 2
                                );

                            Canvas.SetLeft(rectangle, rectOrigin.X);
                            Canvas.SetTop(rectangle, rectOrigin.Y);

                            Canvas.SetLeft(ellipse, elliOrigin.X);
                            Canvas.SetTop(ellipse, elliOrigin.Y);


                            //Canvas.SetLeft(rectangle, p1.X);
                            //Canvas.SetTop(rectangle,  p1.Y);

                            LinearGradientBrush gradientBrush = new LinearGradientBrush();

                            Color color1 = new Color();
                            color1.ScA = float.MaxValue;
                            color1.ScR = color1.ScG = color1.ScB = (float)scaledP1h;

                            Color color18 = new Color();
                            color18.A = byte.MaxValue;
                            color18.R = color18.G = color18.B = (byte)(scaledP1h * byte.MaxValue);

                            Color color2 = new Color();
                            color2.ScA = float.MaxValue;
                            color2.ScR = color2.ScG = color2.ScB = (float)scaledP2h;

                            Color color28 = new Color();
                            color28.A = byte.MaxValue;
                            color28.R = color28.G = color28.B = (byte)(scaledP2h * byte.MaxValue);

                            // Define gradient stops
                            gradientBrush.GradientStops.Add(new GradientStop(color18, 0.0));
                            gradientBrush.GradientStops.Add(new GradientStop(color28, 1.0));

                            // Set the rectangle's fill to the gradient brush
                            rectangle.Fill = gradientBrush;
                            ellipse.Fill = new SolidColorBrush(color18);

                            ToolTip rttp = new ToolTip();
                            rttp.Content = "OSM ID: " + feature.Properties.Osm_id + "\n";
                            rttp.Content += "Position: X" + Canvas.GetLeft(rectangle) + ", Y" + Canvas.GetTop(rectangle) + "\n";
                            rttp.Content += "Gray: " + scaledP1h + "\n";
                            rttp.Content += feature.Properties.Name;
                            rectangle.ToolTip = rttp;

                            ToolTip ettp = new ToolTip();
                            ettp.Content = "OSM ID: " + feature.Properties.Osm_id + "\n";
                            ettp.Content += "Position: X" + Canvas.GetLeft(rectangle) + ", Y" + Canvas.GetTop(rectangle) + "\n";
                            ettp.Content += "Gray: " + scaledP1h + "\n";
                            ettp.Content += feature.Properties.Name;
                            ellipse.ToolTip = ettp;


                            rectangle.MouseEnter += (sender, e) => { (sender as Rectangle).Fill = Brushes.Red; };
                            rectangle.MouseLeave += (sender, e) => { (sender as Rectangle).Fill = gradientBrush; };
                            rectangle.MouseDown += (sender, e) => { Clipboard.SetText(((sender as Rectangle).ToolTip as ToolTip).Content as string); };

                            ellipse.MouseEnter += (sender, e) => { (sender as Ellipse).Fill = Brushes.Red; };
                            ellipse.MouseLeave += (sender, e) => { (sender as Ellipse).Fill = gradientBrush; };
                            ellipse.MouseDown += (sender, e) => { Clipboard.SetText(((sender as Ellipse).ToolTip as ToolTip).Content as string); };

                            shapes.Add(rectangle);
                            shapes.Add(ellipse);
                        }
                    }
                }
            }

            return shapes;
        }
        public List<Shape> getRoads16()
        {
            List<Shape> shapes = new List<Shape>();

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

                            double metersLong = Helpers.LongitudeToMeters(normalLong);
                            double metersLat = Helpers.LatitudeToMeters(normalLat);



                            double scaledX = metersLong * scaleX;
                            double scaledY = metersLat * scaleY;

                            scaledY = (scaledY * -1) + MapSize;

                            return new List<double> { scaledX, scaledY, c[2] };
                        }).ToList();



                        for (int i = 0; i < Points.Count - 1; i++)
                        {
                            //Point p1 = new Point(Points[i][0], Points[i][1]);
                            //double p1h = Points[i][2];
                            //double scaledP1h = (p1h - minQgisH) / diffQgisH;
                            //Point p2 = new Point(Points[i + 1][0], Points[i + 1][1]);
                            //double scaledP2h = (Points[i + 1][2] - minQgisH) / diffQgisH;

                            Point p1 = new Point(Points[i][0], Points[i][1]);
                            double p1h = Points[i][2];
                            double normalP1h = p1h - minAscH;
                            double scaledP1h = normalP1h / (diffAscH);
                            Point p2 = new Point(Points[i + 1][0], Points[i + 1][1]);
                            double p2h = Points[i + 1][2];
                            double normalP2h = p2h - minAscH;
                            double scaledP2h = normalP2h / (diffAscH);


                            // Calculate the length of the segment
                            double length = Math2.DistanceBetweenPoints(p1, p2);

                            // Calculate the angle of the segment
                            double angle = Math2.CalculatePolylineAngle(p1, p2);

                            // Calculate the center point of the segment
                            Point center = Math2.getPointsCenter(p1, p2);

                            // Create a rotation transform
                            angle -= 90;
                            RotateTransform rotation = new RotateTransform(angle);

                            // Create the rectangle
                            //DrawableRectangle drawableRectangle = new DrawableRectangle(center, rotation);
                            Rectangle rectangle = new Rectangle
                            {
                                Width = roadWidth,
                                Height = length
                            };

                            Ellipse ellipse = new Ellipse
                            {
                                Width = roadWidth,
                                Height = roadWidth
                            };

                            // Set the position of the rectangle

                            // Apply rotation to the rectangle
                            rectangle.RenderTransform = rotation;
                            //ellipse.RenderTransform = rotation;

                            Point origin = Math2.Trigonometry.RightTriangle.GetCatheti(roadWidth, angle);
                            Point rectOrigin = new Point(p1.X - (origin.X / 2), p1.Y - (origin.Y / 2));
                            Point elliOrigin = new Point(
                                p1.X - roadWidth / 2,
                                p1.Y - roadWidth / 2
                                );

                            Canvas.SetLeft(rectangle, rectOrigin.X);
                            Canvas.SetTop(rectangle, rectOrigin.Y);

                            Canvas.SetLeft(ellipse, elliOrigin.X);
                            Canvas.SetTop(ellipse, elliOrigin.Y);


                            //Canvas.SetLeft(rectangle, p1.X);
                            //Canvas.SetTop(rectangle,  p1.Y);

                            LinearGradientBrush gradientBrush = new LinearGradientBrush();


                            ushort gray16A = (ushort)(scaledP1h * ushort.MaxValue);
                            string hex = gray16A.ToString("X").PadLeft(4, '0');
                            byte R = Convert.ToByte(hex.Substring(0, 2), 16);
                            byte G = Convert.ToByte(hex.Substring(2, 2), 16);
                            Color color16A = new Color()
                            {
                                R = R,
                                G = G,
                                B = byte.MinValue,
                                A = byte.MaxValue
                            };

                            ushort gray16B = (ushort)(scaledP2h * ushort.MaxValue);
                            string hex2 = gray16B.ToString("X").PadLeft(4, '0');
                            byte R2 = Convert.ToByte(hex2.Substring(0, 2), 16);
                            byte G2 = Convert.ToByte(hex2.Substring(2, 2), 16);
                            Color color16B = new Color()
                            {
                                R = R2,
                                G = G2,
                                B = byte.MinValue,
                                A = byte.MaxValue
                            };


                            // Define gradient stops
                            gradientBrush.GradientStops.Add(new GradientStop(color16A, 0.0));
                            gradientBrush.GradientStops.Add(new GradientStop(color16B, 1.0));

                            // Set the rectangle's fill to the gradient brush
                            rectangle.Fill = gradientBrush;
                            ellipse.Fill = new SolidColorBrush(color16A);

                            ToolTip rttp = new ToolTip();
                            rttp.Content = "OSM ID: " + feature.Properties.Osm_id + "\n";
                            rttp.Content += "Position: X" + Canvas.GetLeft(rectangle) + ", Y" + Canvas.GetTop(rectangle) + "\n";
                            rttp.Content += "Gray: " + scaledP1h + "\n";
                            rttp.Content += feature.Properties.Name;
                            rectangle.ToolTip = rttp;

                            ToolTip ettp = new ToolTip();
                            ettp.Content = "OSM ID: " + feature.Properties.Osm_id + "\n";
                            ettp.Content += "Position: X" + Canvas.GetLeft(rectangle) + ", Y" + Canvas.GetTop(rectangle) + "\n";
                            ettp.Content += "Gray: " + scaledP1h + "\n";
                            ettp.Content += feature.Properties.Name;
                            ellipse.ToolTip = ettp;


                            rectangle.MouseEnter += (sender, e) => { (sender as Rectangle).Fill = Brushes.Red; };
                            rectangle.MouseLeave += (sender, e) => { (sender as Rectangle).Fill = gradientBrush; };
                            rectangle.MouseDown += (sender, e) => { Clipboard.SetText(((sender as Rectangle).ToolTip as ToolTip).Content as string); };

                            ellipse.MouseEnter += (sender, e) => { (sender as Ellipse).Fill = Brushes.Red; };
                            ellipse.MouseLeave += (sender, e) => { (sender as Ellipse).Fill = gradientBrush; };
                            ellipse.MouseDown += (sender, e) => { Clipboard.SetText(((sender as Ellipse).ToolTip as ToolTip).Content as string); };

                            shapes.Add(rectangle);
                            //shapes.Add(ellipse);
                        }
                    }
                }
            }

            return shapes;
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

            List<int> q = new List<int>();
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
                        q.Add(pC.Count);
                        PathFigure pf = new PathFigure();
                        pf.StartPoint = pC[0]; // Set the start point of the PathFigure

                        if (pC.Count >= 3)
                        {
                            for (int i = 1; i < pC.Count - 1; i++)
                            {
                                QuadraticBezierSegment qbs = new QuadraticBezierSegment(pC[i], pC[i + 1], true);
                                pf.Segments.Add(qbs);

                            }
                        }
                        else if (pC.Count == 2)
                        {
                            LineSegment ls = new LineSegment();
                            ls.Point = pC[1];

                            PathSegmentCollection psc = new PathSegmentCollection
                            {
                                ls
                            };

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
                        string a = string.Join("\n", geoPoints.Select(g => g.ToString()));
                        Clipboard.SetText(a);
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
                bool ignore = ignoreIds.Contains(feature.Properties.ID) || string.IsNullOrEmpty(feature.Properties.Highway);

                if (!ignore)
                {
                    foreach (var coordinatesGroup in feature.Geometry.Coordinates)
                    {
                        foreach (var coord in coordinatesGroup)
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

            diffLatM = Helpers.LatitudeToMeters(diffLat);
            diffLongM = Helpers.LongitudeToMeters(diffLong);

            //Project.Extents = new Extents()
            //{
            //    West = minLat,
            //    North = maxLong,
            //    East = maxLat,
            //    South = minLong
            //};

            //Project.Save();
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
        public string? Name { get; set; }
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

                    double metersLong = Helpers.LongitudeToMeters(normalLong);
                    double metersLat = Helpers.LatitudeToMeters(normalLat);



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
