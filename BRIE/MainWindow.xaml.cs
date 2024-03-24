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
using BRIE.Dialogs;
using Microsoft.Win32;
using Path = System.Windows.Shapes.Path;

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
        public static string CacheFilePath = @"%APPDATA%\Local\BRIE\cache.json";
        public Project Project;

        public MainWindow()
        {
            InitializeComponent();
            CacheFilePath = Environment.ExpandEnvironmentVariables(CacheFilePath);
            Cache cache;


            this.Loaded += (obj, e) =>
            {
                setMaxRestoreVis();
                txtOutput.Text = ">";
                if (File.Exists(CacheFilePath)) cache = FileManager.OpenJson<Cache>(CacheFilePath);
                else cache = new Cache(this);

                Width = cache.WindowSize.Width;
                Height = cache.WindowSize.Height;
                WindowState = cache.IsWindowMaximized ? WindowState.Maximized : WindowState.Normal;
                Left = cache.WindowPosition.X;
                Top = cache.WindowPosition.Y;

                SizeChanged += cache.WindowSizeChanged;
                StateChanged += (sender, e) =>
                {
                    setMaxRestoreVis();
                    cache.WindowStateChanged(sender, e);
                };

                LocationChanged += cache.WindowLocationChanged;



                ProjectsDialog pdiag = new ProjectsDialog();

                pdiag.Owner = this;
                pdiag.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                pdiag.ShowDialog();

                //Save16BitImage();

                drawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;

                if (Project != null)
                {
                    Title = Project.Name;
                    if (Project.GeoJsonPath != null) geoJson = new GeoJson(Project.GeoJsonPath, geoRoadsCanvas);
                    if (Project.HeightmapPath != null) loadHeightmap(Project.HeightmapPath);
                    if (Project.SatMapPath != null) loadSatMap(Project.HeightmapPath);
                    if (Project.HeightmapPath != null) loadHeightmap(Project.HeightmapPath);
                }

                sqBkDrop.Visibility = Visibility.Collapsed;
            };
        }

        private void LoadGeoJson()
        {

            geoRoadsCanvas.Children.Clear();

            List<Shape> roads = geoJson.getRoads();
            List<Ellipse> ellis = roads.OfType<Ellipse>().ToList();
            List<Rectangle> rects = roads.OfType<Rectangle>().ToList();
            foreach (Shape road in ellis)
            {
                geoRoadsCanvas.Children.Add(road);
            }
            foreach (Shape road in rects)
            {
                geoRoadsCanvas.Children.Add(road);
            }

            //linesCanvas.Children.Clear();
            //foreach (Polyline pline in geoJson.GetPolys())
            //{
            //    linesCanvas.Children.Add(pline);
            //}

            linesCanvas.Children.Clear();
            foreach (Path path in geoJson.GetPaths())
            {
                linesCanvas.Children.Add(path);
            }

            ExportToPng(geoRoadsCanvas, "C:\\Users\\athum\\AppData\\Local\\BeamNG.drive\\0.31\\levels\\franka-mini\\roadsele.png");
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
                //encoder.Save(memoryStream);

                // Reset the memory stream position to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Create a new PNG encoder with maximum compression level (lossless)
                PngBitmapEncoder losslessEncoder = new PngBitmapEncoder();
                //losslessEncoder.Frames.Add(BitmapFrame.Create(memoryStream));

                // Save the lossless encoded image to the file
                using (FileStream fs = File.Create(filePath))
                {
                    encoder.Save(fs);
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

                geoRoadsCanvas.Children.Clear();

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

                geoJson = new GeoJson(selectedFilePath, geoRoadsCanvas);
                LoadGeoJson();
            }
        }

        private void OpenBeamNG_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void OpenHeightMap_Click(object sender, RoutedEventArgs e)
        {
            string? file = FileManager.OpenPng();
            if (file != null) loadHeightmap(file);
        }

        private void loadHeightmap(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                Uri uri = new Uri(path);
                BitmapImage bitmap = new BitmapImage(uri);

                hmCanvas.Background = new ImageBrush(bitmap);
            }
        }

        private void OpenSatMap_Click(object sender, RoutedEventArgs e)
        {
            string? file = FileManager.OpenPng();
            if (file != null) loadSatMap(file);
        }

        private void loadSatMap(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                Uri uri = new Uri(path);
                BitmapImage bitmap = new BitmapImage(uri);

                smCanvas.Background = new ImageBrush(bitmap);
            }
        }

        #region Zoom Stuff
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
            ChangeZoom(newScaleFactor);

            // Scroll the ScrollViewer to the new position
            scrl.ScrollToHorizontalOffset(newHorizontalOffset);
            scrl.ScrollToVerticalOffset(newVerticalOffset);

            e.Handled = true;
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ChangeZoom(1);
        }

        private void IncZoom_Click(object sender, RoutedEventArgs e)
        {
            double newScaleFactor = drawingCanvas.LayoutTransform.Value.M11 + 0.1;
            ChangeZoom(newScaleFactor);

        }

        private void DecZoom_Click(object sender, RoutedEventArgs e)
        {
            double newScaleFactor = drawingCanvas.LayoutTransform.Value.M11 - 0.1;
            ChangeZoom(newScaleFactor);
        }

        private void ChangeZoom(double scale)
        {
            if (scale > 0.01 && scale < 100)
            {
                // Apply the new scale factor
                drawingCanvas.LayoutTransform = new ScaleTransform(scale, scale);
                txtZoom.Text = (scale * 100).ToString("###");
            }
        }

#endregion

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

        private void winBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Name)
            {
                case "btnWinClose":
                    Close(); break;
                case "btnWinMin":
                    
                    WindowState = WindowState.Minimized;
                    break;
                default:
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; 
                    break;
            }
        }

        private void setMaxRestoreVis()
        {
            bool IsWindowMaximized = WindowState == WindowState.Maximized;

            btnWinMax.Visibility = IsWindowMaximized ? Visibility.Collapsed : Visibility.Visible;
            btnWinRestore.Visibility = !IsWindowMaximized ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Window.GetWindow(this) is Window window)
                {
                    window.DragMove();
                }
            }
        }

        private void ExportFileClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
