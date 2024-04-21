using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BRIE.Etc;

namespace BRIE.Types.Geographics
{
    public class Extents : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        private double _west, _north, _east, _south;

        public double West
        {
            get => _west;
            set { _west = value; OnPropertyChanged(nameof(West)); }
        }

        public double North
        {
            get => _north;
            set { _north = value; OnPropertyChanged(nameof(North)); }
        }

        public double East
        {
            get => _east;
            set { _east = value; OnPropertyChanged(nameof(East)); }
        }

        public double South
        {
            get => _south;
            set { _south = value; OnPropertyChanged(nameof(South)); }
        }

        public double Height => North - South;
        public double Width => East - West;
        public double MetricHeight => Helpers.LatitudeToMeters(Height);
        public double MetricWidth => Helpers.LatitudeToMeters(Width);

        public Extents()
        {
        }

        public Extents(double west, double north, double east, double south)
        {
            West = west;
            North = north;
            East = east;
            South = south;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
