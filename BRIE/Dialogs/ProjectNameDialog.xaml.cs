using System.Windows;
using System.Windows.Media;

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectNameDialog.xaml
    /// </summary>
    public partial class ProjectNameDialog : Window
    {
        public string Name, fileName;
        public bool IsSafe = false;
        public ProjectNameDialog()
        {
            InitializeComponent();
            btnClose.IsEnabled = false;
        }

        private void iptName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

            Name = iptName.Text;
            fileName = iptName.Text.SanitizeFileName();

            IsSafe = !(string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(fileName));

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
            Name = fileName = "";
            IsSafe = false;
            Close();
        }
    }
}
