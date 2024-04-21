using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BRIE.Classes;
using BRIE.Classes.RoadsSources;
using BRIE.Classes.Statics;
using BRIE.Dialogs;
using BRIE.Etc;
using BRIE.ExportFormats;
using Microsoft.Win32;
using static BRIE.Classes.RoadsCollection;
using IOPath = System.IO.Path;
using Path = System.Windows.Shapes.Path;


[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(BRIE.MainWindow))]

namespace BRIE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static event EventHandler HotReloaded;
        public event PropertyChangedEventHandler? PropertyChanged;
        private GeoJson geoJson;
        public int canvasSize;
        public static string CacheFilePath = @"%APPDATA%\Local\BRIE\Cache.json";
        private bool isDragging;
        private Point lastMouseDown;
        public Output Output { get; set; }

        private ProjectData _projectData;

        public ProjectData? ProjectData
        {
            get
            {
                return _projectData;
            }
            set { _projectData = value; OnPropertyChanged(nameof(ProjectData)); }
        }




        public static void UpdateApplication(Type[]? types)
        {
            HotReloaded?.Invoke(new object(), new EventArgs());
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Output = new();
            CacheFilePath = Environment.ExpandEnvironmentVariables(CacheFilePath);
            sqBkDrop.Visibility = Visibility.Visible;
            HotReloaded += MainWindow_HotReloaded;

            this.Loaded += (obj, e) =>
            {
                Output.WriteLine("MainWindow started");
                if (Keyboard.IsKeyDown(Key.LeftShift)) // Check if the 'A' key is pressed
                {
                    MessageBoxResult res = MessageBox.Show("Do you want to delete the cache?", "BRIE Recovery Mode", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        File.Delete(CacheFilePath);
                    }
                }
                setMaxRestoreVis();
                Cache.Initialize(this);
                Project.DataChanged += Project_DataChanged;

                Width = Cache.WindowSize.Width;
                Height = Cache.WindowSize.Height;
                WindowState = Cache.IsWindowMaximized ? WindowState.Maximized : WindowState.Normal;
                Left = Cache.WindowPosition.X;
                Top = Cache.WindowPosition.Y;

                SizeChanged += Cache.WindowSizeChanged;
                StateChanged += (sender, e) =>
                {
                    setMaxRestoreVis();
                    Cache.WindowStateChanged(sender, e);
                };

                LocationChanged += Cache.WindowLocationChanged;

                OpenProjectsDiag();

                drawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;

                sqBkDrop.Visibility = Visibility.Collapsed;
            };
        }

        private void MainWindow_HotReloaded(object? sender, EventArgs e)
        {
            if (geoJson != null)
                this.Dispatcher.Invoke(() =>
                {
                    LoadRoadsHeightmap(geoJson);
                });

        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetUI()
        {
            tviProjectName.Header = "Unsaved Project";
            drawingCanvas.Children.OfType<Canvas>().ToList().ForEach(c => c.Children.Clear());
        }

        private void Project_DataChanged(object? sender, EventArgs e)
        {
            ResetUI();
            ProjectData = Project.Data;
            tviProjectName.Header = Project.Name;
            if (Project.GeoJsonPath != null)
            {
                geoJson = new GeoJson(Project.GeoJsonPath, geoRoadsCanvas);
                LoadRoadsHeightmap(geoJson);
            }
            if (Project.HeightmapPath != null) loadHeightmap(Project.HeightmapPath);
            if (Project.SatMapPath != null) loadSatMap(Project.SatMapPath);
            if (Project.HeightmapPath != null) loadHeightmap(Project.HeightmapPath);
        }

        private void OpenProjectsDiag()
        {
            ProjectsDialog pdiag = new ProjectsDialog()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false
            };

            pdiag.ShowDialog();
        }

        private void LoadRoadsHeightmap(RoadsSource roadsSource)
        {
            roadsSource.ToRoadsCollection();
            geoRoadsCanvas.Children.Clear();
            SetCanvasSize(Project.Resolution);

            List<Shape> roads = RoadsCollection.GetRoadsShapes();

            foreach (Shape road in roads)
            {
                geoRoadsCanvas.Children.Add(road);
            }
        }

        #region Files Stuff

        #region Open Files stuff
        private void OpenGeoJSON_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GeoJSON Files (*.geojson)|*.geojson|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Process the selected GeoJSON file
                string selectedFilePath = openFileDialog.FileName;
                if (Project.IsInitialized) Project.GeoJsonPath = selectedFilePath;

                geoJson = new GeoJson(selectedFilePath, geoRoadsCanvas);
                LoadRoadsHeightmap(geoJson);
            }
        }

        private void OpenBeamNG_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void OpenHeightMap_Click(object sender, RoutedEventArgs e)
        {
            string? file = FileManager.OpenPng();
            if (file != null)
            {
                if (Project.IsInitialized) Project.HeightmapPath = file;
                loadHeightmap(file);
            }
        }

        private void loadHeightmap(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = new Uri(path);
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                hmCanvas.Background = new ImageBrush(bmi);

            }
        }

        private void OpenSatMap_Click(object sender, RoutedEventArgs e)
        {
            string? file = FileManager.OpenPng();
            if (file != null)
            {
                if (Project.IsInitialized) Project.SatMapPath = file;
                loadSatMap(file);
            }
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

        #endregion

        #region Export Files Stuff
        private void ExportRoadsToHeightmap_Click(object sender, RoutedEventArgs e)
        {
            ExportDialog exdiag = new ExportDialog()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false
            };

            sqBkDrop.Visibility = Visibility.Visible;

            exdiag.ShowDialog();

            sqBkDrop.Visibility = Visibility.Collapsed;
        }
        #endregion

        #endregion


        #region Canvas Stuff

        private void SetCanvasSize(int size)
        {
            drawingCanvas.Height = size;
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
                    LoadRoadsHeightmap(geoJson);
                }
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

        #endregion


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

        private void SaveProjectClick(object sender, RoutedEventArgs e)
        {
            Project.Save();
        }

        private void MenuProjects_Click(object sender, RoutedEventArgs e)
        {
            OpenProjectsDiag();
        }

        private void drawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.Space))
            {
                isDragging = true;
                lastMouseDown = e.GetPosition(scrl);
                drawingCanvas.CaptureMouse();
            }
        }

        private void drawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point mousePos = e.GetPosition(scrl);
                double deltaX = mousePos.X - lastMouseDown.X;
                double deltaY = mousePos.Y - lastMouseDown.Y;

                double newHorizontalOffset = Math.Max(0, scrl.HorizontalOffset - deltaX);
                double newVerticalOffset = Math.Max(0, scrl.VerticalOffset - deltaY);

                scrl.ScrollToHorizontalOffset(newHorizontalOffset);
                scrl.ScrollToVerticalOffset(newVerticalOffset);

                lastMouseDown = mousePos;
            }
        }

        private void drawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
                drawingCanvas.ReleaseMouseCapture();
            }
        }

        private void btnApplyRes_Click(object sender, RoutedEventArgs e)
        {
            Project.Resolution = int.Parse(txtResolution.Text);
        }
    }
}
