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
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BRIE.Classes.Etc;
using BRIE.Classes.Roads.Collection;

namespace BRIE.UI.Controls.RoadsEditor
{
    /// <summary>
    /// Interaction logic for EditorSegment.xaml
    /// </summary>
    public partial class EditorSegment : Canvas
    {
        private Segment _segment;
        public EditorSegment(Segment segment)
        {
            InitializeComponent();
            _segment = segment;

            EditorSegment_Loaded(this, new RoutedEventArgs());
        }

        private void EditorSegment_Loaded(object sender, RoutedEventArgs e)
        {
            Width = _segment.Width;
            Height = _segment.Height;

            Ellipse rectStart = new Ellipse()
            {
                Height = 5,
                Width = 5,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                RenderTransform = new TranslateTransform()
                {
                    X = _segment.Start.Position.X - 2.5,
                    Y = _segment.Start.Position.FlipY().Y - 2.5,
                }
            };

            rectStart.MouseEnter += (o, e) => rectStart.Stroke = Brushes.Red;
            rectStart.MouseLeave += (o, e) => rectStart.Stroke = Brushes.Blue;

            Ellipse rectEnd = new Ellipse()
            {
                Height = 5,
                Width = 5,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                RenderTransform = new TranslateTransform()
                {
                    X = _segment.Start.Position.X - 2.5,
                    Y = _segment.Start.Position.FlipY().Y - 2.5,
                }
            };

            Line ln = new Line()
            {
                X1 = _segment.Start.Position.X,
                Y1 = _segment.Start.Position.FlipY().Y,
                X2 = _segment.End.Position.X,
                Y2 = _segment.End.Position.FlipY().Y,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round

            };
            ln.StrokeThickness = 1;
            ln.Stroke = Brushes.Blue;

            Children.Add(ln);
            Children.Add(rectStart);
            Children.Add(rectEnd);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
