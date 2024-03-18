using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace BRIE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeoJson geoJson;
        private Point lastMousePos;
        private Point startScrollPoint;
        public int canvasSize;

        public MainWindow()
        {
            InitializeComponent();

            //Save16BitImage();

            drawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;

            string file = @"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\google\RoadsWithHeigths.geojson";

            loadHeightmap(@"C:\Users\athum\AppData\Local\BeamNG.drive\0.31\levels\franka-mini\height_source.png");

            geoJson = new GeoJson(file, drawingCanvas);



            LoadGeoJson();

            roadsCanvas.Loaded += (a, b) =>
            {
                //Thread.Sleep(10000);
                ExportToPng(roadsCanvas, "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\roadsele.png");
            };

            hmCanvas.Loaded += (a, b) =>
            {
                ExportToPng(hmCanvas, "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\hmele.png");
            };

        }

        private void LoadGeoJson()
        {

            roadsCanvas.Children.Clear();

            List<Shape> roads = geoJson.getRoads();
            List<Ellipse> ellis = roads.OfType<Ellipse>().ToList();
            List<Rectangle> rects = roads.OfType<Rectangle>().ToList();
            foreach (Shape road in ellis)
            {
                roadsCanvas.Children.Add(road);
            }
            foreach (Shape road in rects)
            {
                roadsCanvas.Children.Add(road);
            }

            linesCanvas.Children.Clear();
            //foreach (Polyline pline in geoJson.GetPolys())
            //{
            //    linesCanvas.Children.Add(pline);
            //}

            //ExportToPng(roadsCanvas, "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\roadsele.png");
        }

        public static void ExportToPng(Canvas canvas, string filePath)
        {
            // Create a RenderTargetBitmap with the same size as the canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.Width, (int)canvas.Height,
                96, 96, PixelFormats.Pbgra32);

            // Render the canvas onto the RenderTargetBitmap
            renderBitmap.Render(canvas);

            // Create a PngBitmapEncoder and add the RenderTargetBitmap to it
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            // Create a MemoryStream to hold the encoded image data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add the RenderTargetBitmap to the encoder
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                // Save the encoded image to the memory stream
                encoder.Save(memoryStream);

                // Reset the memory stream position to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Create a new PNG encoder with maximum compression level (lossless)
                PngBitmapEncoder losslessEncoder = new PngBitmapEncoder();
                losslessEncoder.Frames.Add(BitmapFrame.Create(memoryStream));

                // Save the lossless encoded image to the file
                using (FileStream fs = File.Create(filePath))
                {
                    losslessEncoder.Save(fs);
                }
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != e.PreviousSize.Height && e.PreviousSize.Height != 0)
            {
                Canvas canvas = (Canvas)sender;
                int h = (int)canvas.Height;
                canvas.Height = h;
                canvas.Width = h;

                foreach (Canvas i in drawingCanvas.Children)
                {
                    i.Height = i.Width = h;
                }

                roadsCanvas.Children.Clear();

                if (geoJson != null)
                {
                    LoadGeoJson();
                }
            }
        }

        private void OpenGeoJSON_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GeoJSON Files (*.geojson)|*.geojson|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Process the selected GeoJSON file
                string selectedFilePath = openFileDialog.FileName;

                geoJson = new GeoJson(selectedFilePath, roadsCanvas);
            }
        }

        private void OpenBeamNG_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void OpenHeightMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected PNG file as the canvas background
                loadHeightmap(openFileDialog.FileName);
            }
        }

        private void loadHeightmap(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                Uri uri = new Uri(path);
                BitmapImage bitmap = new BitmapImage(uri);

                hmCanvas.Background = new ImageBrush(bitmap);
                txtHm.Text = path.Split("\\").Last<string>();
            }
        }

        private void OpenSatMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                loadSatMap(openFileDialog.FileName);
            }
        }

        private void loadSatMap(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                Uri uri = new Uri(path);
                BitmapImage bitmap = new BitmapImage(uri);

                siCanvas.Background = new ImageBrush(bitmap);
                txtSi.Text = path.Split("\\").Last<string>();
            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Increase or decrease the scale factor based on the mouse wheel delta
            double scaleFactorChange = (e.Delta > 0) ? 0.1 : -0.1;

            // Get the mouse position relative to the canvas
            Point mousePosition = e.GetPosition(drawingCanvas);

            // Store the current scroll offsets
            double currentHorizontalOffset = scrl.HorizontalOffset;
            double currentVerticalOffset = scrl.VerticalOffset;

            // Calculate the new scroll offsets based on the mouse position
            double newHorizontalOffset = currentHorizontalOffset + (mousePosition.X * scaleFactorChange);
            double newVerticalOffset = currentVerticalOffset + (mousePosition.Y * scaleFactorChange);

            // Limit the scaling factor to avoid very small or large scales
            double newScaleFactor = drawingCanvas.LayoutTransform.Value.M11 + scaleFactorChange;
            if (newScaleFactor > 0.01 && newScaleFactor < 100)
            {
                // Apply the new scale factor
                drawingCanvas.LayoutTransform = new ScaleTransform(newScaleFactor, newScaleFactor);
            }

            // Scroll the ScrollViewer to the new position
            scrl.ScrollToHorizontalOffset(newHorizontalOffset);
            scrl.ScrollToVerticalOffset(newVerticalOffset);

            e.Handled = true;
        }

        private void CanvasSize_Selected(object sender, RoutedEventArgs e)
        {
            ComboBoxItem cbbx = (ComboBoxItem)sender;

            if (cbbx.Content != null)
            {
                int size = int.Parse(cbbx.Content.ToString());
                drawingCanvas.Height = size;
            }
        }

        private void canvasVis_Click(object sender, RoutedEventArgs e)
        {
            Button source = (Button)sender;
            string canvasName = source.Name.Replace("Vis", "Canvas");
            Canvas canvas = drawingCanvas.Children.OfType<Canvas>().Where(i => i.Name == canvasName).First();

            bool isVisible = canvas.Visibility == Visibility.Visible;

            canvas.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
            source.Foreground = isVisible ? Brushes.Red : Brushes.Black;
            source.Content = isVisible ? "\xE738" : "\xE7B3";
        }

        private void mod_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (sender as Slider);

            if (slider.Name == "minHmod")
            {
                geoJson.minAscHM = e.NewValue;
            }
            else
            {
                geoJson.maxAscHM = e.NewValue;
            }

            ToolTip toolTip = new ToolTip();
            toolTip.Content = e.NewValue.ToString();
            slider.ToolTip = toolTip;

            LoadGeoJson();
        }
    }
}
