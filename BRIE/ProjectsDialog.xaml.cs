using System.Windows;

namespace BRIE
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

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainWindow).Project = FileManager.OpenBrie();
            Close();
        }

        private void OpenLevelFolderButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open Level Folder button clicked.");
        }

        private void CreateProjectFromFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectNameDialog projectNameDialog = new ProjectNameDialog();
            projectNameDialog.ShowDialog();

            project = FileManager.NewBrie(projectNameDialog.Name);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the click event for the "Close" button
            Close();
        }
    }
}
