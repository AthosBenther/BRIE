using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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



            drawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;

            string file = "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\google\\RoadsWithHeigths.geojson";

            geoJson = new GeoJson(file, drawingCanvas);

            LoadGeoJson();

            roadsCanvas.Loaded += (a, b) =>
            {

                ExportToPng(roadsCanvas, "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\roadsele.png");

            };

        }

        private void LoadGeoJson()
        {
            //foreach (System.Windows.Shapes.Rectangle rect in geoJson.GetRects())
            //{
            //    roadsCanvas.Children.Add(rect);
            //}

            //foreach (Polyline pline in geoJson.GetPolys())
            //{
            //    linesCanvas.Children.Add(pline);
            //}

            foreach (System.Windows.Shapes.Path path in geoJson.GetPaths())
            {
                linesCanvas.Children.Add(path);
            }
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
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Save the encoded image to the file
            using (FileStream fs = File.Create(filePath))
            {
                encoder.Save(fs);
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != e.PreviousSize.Height)
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
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap = new BitmapImage(uri);

                hmCanvas.Background = new ImageBrush(bitmap);
                txtHm.Text = openFileDialog.FileName.Split("\\").Last<string>();
            }
        }

        private void OpenSatMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected PNG file as the canvas background
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap = new BitmapImage(uri);

                siCanvas.Background = new ImageBrush(bitmap);
                txtSi.Text = openFileDialog.FileName.Split("\\").Last<string>();
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
    }
}
