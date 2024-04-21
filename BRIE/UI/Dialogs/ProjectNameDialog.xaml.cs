using System.Windows;
using System.Windows.Media;
using BRIE.Classes.Etc;

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectNameDialog.xaml
    /// </summary>
    public partial class ProjectNameDialog : Window
    {
        public string ProjectName, FileName;
        public bool IsSafe = false;
        public bool DialogCanceled = false;
        public ProjectNameDialog()
        {
            InitializeComponent();
            btnClose.IsEnabled = false;
        }

        private void iptName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

            ProjectName = iptName.Text.Trim();
            FileName = iptName.Text.Trim().SanitizeFileName();

            IsSafe = !(string.IsNullOrWhiteSpace(ProjectName) && string.IsNullOrWhiteSpace(FileName));

            if (!IsSafe)
            {
                iptName.BorderBrush = Brushes.Red;
                btnClose.IsEnabled = false;
            }
            else
            {
                iptName.BorderBrush = SystemColors.ActiveBorderBrush;
                btnClose.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectName = FileName = "";
            IsSafe = false;
            DialogCanceled = true;
            Close();
        }
    }
}
