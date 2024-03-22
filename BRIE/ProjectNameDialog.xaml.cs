using System.Windows;

namespace BRIE
{
    /// <summary>
    /// Interaction logic for ProjectNameDialog.xaml
    /// </summary>
    public partial class ProjectNameDialog : Window
    {
        public string Name;
        public ProjectNameDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Name = iptName.Text;

            Close();
        }
    }
}
