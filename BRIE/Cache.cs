using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BRIE
{
    public class Cache
    {
        private Dictionary<DateTime, string[]> _recentProjects;
        private Size _windowSize;
        private Point _windowPosition;
        private bool _isWindowMaximized;
        private bool _isInitializing = false;
        private bool _isInitialized = false;


        public Dictionary<DateTime, string[]> RecentProjects
        {
            get { return _recentProjects; }
        }

        public Size WindowSize
        {
            get { return _windowSize; }
            set { _windowSize = value; Save(); }
        }

        public Point WindowPosition
        {
            get { return _windowPosition; }
            set { _windowPosition = value; Save(); }
        }

        public bool IsWindowMaximized
        {
            get { return _isWindowMaximized; }
            set { _isWindowMaximized = value; Save(); }
        }

        public Cache()
        {
            if (_recentProjects == null) _recentProjects = new Dictionary<DateTime, string[]>();
        }

        public Cache(Window MainWindow)
        {
            _isInitializing = true;

            _recentProjects = new Dictionary<DateTime, string[]>();
            WindowSize = MainWindow.RenderSize;
            WindowPosition = new Point(MainWindow.Left, MainWindow.Top);
            IsWindowMaximized = (MainWindow.WindowState == WindowState.Maximized);

            _isInitializing = false;
            Save();
        }

        public void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSize = e.NewSize;
        }

        public Dictionary<DateTime, string[]> GetRecentProjects()
        {
            return _recentProjects;
        }

        public void UpsertRecentProject(string projectName, string projectPath)
        {
            // Get the current date and time
            DateTime dateTime = DateTime.Now;

            // Remove any recent project with the same name
            var projectsToRemove = _recentProjects.Where(pair => pair.Value[0] == projectName).ToList();
            foreach (var projectToRemove in projectsToRemove)
            {
                _recentProjects.Remove(projectToRemove.Key);
            }

            // Add the new recent project
            _recentProjects.Add(dateTime, new string[] { projectName, projectPath });
            Save();
        }

        public void RemoveRecentProject(string projectName)
        {
            // Find the keys of recent projects with the specified project name
            var keysToRemove = _recentProjects.Where(pair => pair.Value[0] == projectName).Select(pair => pair.Key).ToList();

            // Remove the recent projects with the specified project name
            foreach (var key in keysToRemove)
            {
                _recentProjects.Remove(key);
            }
            Save();
        }

        public void ClearRecentProjects()
        {
            _recentProjects.Clear();
            Save();
        }

        private void Save()
        {

            if (!_isInitializing)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MainWindow.CacheFilePath));
                FileManager.SaveJson(this, MainWindow.CacheFilePath);
            }
        }

        internal void WindowStateChanged(object? sender, EventArgs e)
        {
            IsWindowMaximized = (sender as MainWindow).WindowState == WindowState.Maximized;
            Save();
        }

        internal void WindowLocationChanged(object? sender, EventArgs e)
        {
            Window w = sender as Window;
            WindowPosition = new Point(w.Left, w.Top);

            Save();
        }
    }
}
