using System.Windows.Controls;
using BRIE.Classes.Statics;

namespace BRIE.Controls
{
    /// <summary>
    /// Interaction logic for RoadsCanvas.xaml
    /// </summary>
    public partial class RoadsCanvas : UserControl
    {

        public RoadsCanvas()
        {
            InitializeComponent();

            roadsCanvas.Height = Project.Resolution;
            roadsCanvas.Width = Project.Resolution;

        }
    }
}
