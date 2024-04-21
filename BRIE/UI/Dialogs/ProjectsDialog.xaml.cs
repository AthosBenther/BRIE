using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BRIE.Classes.Statics;
using BRIE.Controls;
using static System.Net.Mime.MediaTypeNames;
using Project = BRIE.Classes.Statics.Project;

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectsDialog.xaml
    /// </summary>
    public partial class ProjectsDialog : Window
    {
        public ProjectsDialog()
        {
            InitializeComponent();
            Cache.RecentProjectsChanged += Cache_RecentProjectsChanged;
            PopulateRecentProjects();
        }

        private void Cache_RecentProjectsChanged(object? sender, System.EventArgs e)
        {
            PopulateRecentProjects();
        }

        private void PopulateRecentProjects()
        {
            projList.Children.Clear();
            var projs = Cache.GetRecentProjects();
            if (projs.Count > 0)
            {
                foreach (var project in projs)
                {
                    Controls.RecentProject ctrlProj = new Controls.RecentProject(project.Key, project.Value[0], project.Value[1]);
                    ctrlProj.Click += (o, e) =>
                    {
                        Visibility = Visibility.Collapsed;
                        Project.Initialize(FileManager.OpenBrie(ctrlProj.Path));
                        Close();
                    };
                    projList.Children.Add(ctrlProj);
                }
            }
            else
            {
                TextBlock emptyList = new TextBlock();
                emptyList.Foreground = SystemColors.GrayTextBrush;
                emptyList.Text = "No projects have been opened recently";
                projList.Children.Add(emptyList);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Project.Initialize(FileManager.OpenBrie());
            if (Project.IsInitialized) Close();
            else this.Visibility = Visibility.Visible;
        }

        private void OpenLevelFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open Level Folder button clicked.");
        }

        private void CreateNewProject_Click(object sender, RoutedEventArgs e)
        {
            ProjectNameDialog projectNameDialog = new ProjectNameDialog();
            projectNameDialog.Owner = this;
            projectNameDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            sqBkDrop.Visibility = Visibility.Visible;
            projectNameDialog.ShowDialog();
            if (projectNameDialog.IsSafe)
            {
                Project.Initialize(FileManager.NewBrie(projectNameDialog.ProjectName, projectNameDialog.FileName));
                Close();
            }
            sqBkDrop.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the click event for the "Close" button
            Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Window.GetWindow(this) is Window window)
                {
                    window.DragMove();
                }
            }
        }
    }
}
