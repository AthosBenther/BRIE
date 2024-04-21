using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BRIE.Classes.Statics
{
    public class CacheData
    {
        public Dictionary<DateTime, List<string>> RecentProjects { get; set; }

        public Size WindowSize { get; set; }
        public Point WindowPosition { get; set; }
        public bool IsWindowMaximized { get; set; }
    }
    public static class Cache
    {
        public static event EventHandler RecentProjectsChanged;
        private static CacheData Data;
        private static bool _isInitializing = false;
        private static bool _isInitialized = false;

        private static MainWindow MainWindow;
        private static Dictionary<DateTime, List<string>> RecentProjects
        {
            get
            {
                return Data.RecentProjects.OrderByDescending(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            set
            {
                Data.RecentProjects = value.OrderByDescending(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public static Size WindowSize
        {
            get
            {
                if (Data.WindowSize.Width <= 0 || Data.WindowSize.Height <= 0) Data.WindowSize = new Size(800, 600);
                return Data.WindowSize;
            }
            set { Data.WindowSize = value; Save(); }
        }

        public static Point WindowPosition
        {
            get
            {
                if (isPointOutOfBounds(Data.WindowPosition)) Data.WindowPosition = new Point(0, 0);
                return Data.WindowPosition;
            }
            set { Data.WindowPosition = value; Save(); }
        }

        public static bool IsWindowMaximized
        {
            get { return Data.IsWindowMaximized; }
            set { Data.IsWindowMaximized = value; Save(); }
        }


        public static void Initialize(MainWindow mainWindow)
        {
            if (!_isInitialized && !_isInitializing)
            {
                MainWindow = mainWindow;
                _isInitializing = true;

                // Load cache data from file
                LoadCacheData();

                _isInitialized = true;
                _isInitializing = false;

                Save();
            }
        }



        public static void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSize = e.NewSize;
        }

        public static Dictionary<DateTime, List<string>> GetRecentProjects()
        {
            return RecentProjects;
        }

        public static void UpsertRecentProject(string projectName, string projectPath)
        {
            // Get the current date and time
            DateTime dateTime = DateTime.Now;

            // Remove any recent project with the same name
            var projectsToRemove = RecentProjects.Where(pair => pair.Value[0] == projectName).ToList();
            foreach (var projectToRemove in projectsToRemove)
            {
                RecentProjects.Remove(projectToRemove.Key);
            }

            // Add the new recent project
            RecentProjects.Add(dateTime, new List<string> { projectName, projectPath });
            InvokeRecentProjectsChanged();
            Save();
        }

        public static void RemoveRecentProject(string projectName)
        {
            // Find the keys of recent projects with the specified project name
            var keysToRemove = RecentProjects.Where(pair => pair.Value[0] == projectName).Select(pair => pair.Key).ToList();

            // Remove the recent projects with the specified project name
            foreach (var key in keysToRemove)
            {
                RecentProjects.Remove(key);
            }
            InvokeRecentProjectsChanged();
            Save();
        }

        public static void ClearRecentProjects()
        {
            InvokeRecentProjectsChanged();
            Save();
        }

        private static void InvokeRecentProjectsChanged()
        {
            RecentProjectsChanged?.Invoke(new object(), new EventArgs());
        }

        private static void LoadCacheData()
        {
            Data = new CacheData();
            RecentProjects = new Dictionary<DateTime, List<string>>();
            WindowSize = MainWindow.RenderSize;
            WindowPosition = new Point(MainWindow.Left, MainWindow.Top);
            IsWindowMaximized = MainWindow.WindowState == WindowState.Maximized;

            if (File.Exists(MainWindow.CacheFilePath))
            {
                Data = FileManager.OpenJson<CacheData>(MainWindow.CacheFilePath);
            }
        }
        private static void Save()
        {

            if (!_isInitializing)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MainWindow.CacheFilePath));
                FileManager.SaveJson(Data, MainWindow.CacheFilePath);
            }
        }

        internal static void WindowStateChanged(object? sender, EventArgs e)
        {
            IsWindowMaximized = (sender as MainWindow).WindowState == WindowState.Maximized;
            Save();
        }

        internal static void WindowLocationChanged(object? sender, EventArgs e)
        {
            Window w = sender as Window;
            WindowPosition = new Point(w.Left, w.Top);

            Save();
        }

        private static bool isPointOutOfBounds(Point point)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            return point.X < 0 || point.X > screenWidth ||
                                   point.Y < 0 || point.Y > screenHeight;


        }
    }
}
