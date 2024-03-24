using System.Windows;

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectsDialog.xaml
    /// </summary>
    public partial class ProjectsDialog : Window
    {
        public Project? project;
        public string? ProjectPath;
        public ProjectsDialog()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            MainWindow MainWindow = (Owner as MainWindow);
            MainWindow.Project = FileManager.OpenBrie();
            if (MainWindow.Project != null)
            {
                Close();
            }
            
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
                project = FileManager.NewBrie(projectNameDialog.Name, projectNameDialog.fileName);
                Close();
            }
            sqBkDrop.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the click event for the "Close" button
            Close();
        }

    }
}
