using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using BRIE.Classes.Roads.Collection;
using BRIE.Classes.Statics;

namespace BRIE.UI.Controls.RoadsEditor
{
    /// <summary>
    /// Interaction logic for EditorCanvas.xaml
    /// </summary>
    public partial class EditorCanvas : Canvas
    {
        public EditorCanvas()
        {
            InitializeComponent();

            EditorCanvas_Loaded(this, new RoutedEventArgs());
            Background = Brushes.Red;

            Loaded += EditorCanvas_Loaded;
        }

        private void EditorCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Road road in RoadsCollection.All)
            {
                for (int segIndex = 0; segIndex < road.Segments.Count; segIndex++)
                {
                    Segment segment = road.Segments[segIndex];
                    EditorSegment es = new EditorSegment(segment);

                    Children.Add(es);
                }
            }
        }
    }
}
