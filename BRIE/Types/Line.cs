using System.Windows;
using System.Windows.Shapes;

namespace BRIE.Types
{
    public class Line
    {
        public double X1, Y1, X2, Y2;
        public Point Start { get => new Point(X1, Y1); set { X1 = value.X; Y1 = value.Y; } }
        public Point End { get => new Point(X2, Y2); set { X2 = value.X; Y2 = value.Y; } }
    }
}
