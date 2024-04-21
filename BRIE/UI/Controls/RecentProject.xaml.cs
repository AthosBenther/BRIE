using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BRIE.Controls
{
    /// <summary>
    /// Interaction logic for Project.xaml
    /// </summary>
    public partial class RecentProject : UserControl
    {
        public event EventHandler Click;
        public string Path { get => lblPath.Text; }
        public RecentProject(DateTime date, string PName, string Path)
        {
            InitializeComponent();
            lblName.Text = PName;
            lblPath.Text = Path;
            lblDate.Text = date.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new EventArgs());
        }
    }
}
