using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;
using BRIE.ExportFormats;

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private BitmapFrame? _bmpFrame;
        public BitmapFrame? BmpFrame
        {
            get => _bmpFrame; set
            {
                _bmpFrame = value;
                OnPropertyChanged("BmpFrame");
            }
        }

        public Roads Roads;
        public ExportDialog(Roads roads)
        {
            InitializeComponent();
            DataContext = this;

            Roads = roads;
            GetImagePreview();
        }

        private void GetImagePreview()
        {
            var worker = Png16.RenderWorker(Roads);

            bgwpb.RunWorkerCompleted += (obj, arg) =>
            {
                BmpFrame = Png16.RenderImage();
            };
            bgwpb.RunWorkAsync(worker);
        }

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Png16.SaveImage();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DragWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Window.GetWindow(this) is Window window)
                {
                    window.DragMove();
                }
            }
        }

    }
}
