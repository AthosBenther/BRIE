using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = BRIE.Export.ImageRaycasting;
using IOPath = System.IO.Path;

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
                OnPropertyChanged(nameof(BmpFrame));
            }
        }

        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        //private ObservableCollection<string> _fileFormats = new() { "a", "b" };
        public List<string> FileFormats => Image.FormatsExtensions;

        public List<string> _pixelFormats;
        public List<string> PixelFormats
        {
            get => _pixelFormats;
            set
            {
                _pixelFormats = value;
                OnPropertyChanged(nameof(PixelFormats));
            }
        }


        public RoadsCollection Roads;
        public ExportDialog(RoadsCollection roads)
        {
            InitializeComponent();
            DataContext = this;


            Roads = roads;
        }

        private void GetImagePreview()
        {
            var worker = Image.RenderWorker(Roads, false);

            BmpFrame = null;

            if (bgwpb != null && bgwpb.IsInitialized)
            {

                bgwpb.RunWorkerCompleted += (obj, arg) =>
                {
                    BmpFrame = Image.RenderImage();
                    sqBkDrop.Visibility = Visibility.Collapsed;
                };
                bgwpb.RunWorkAsync(worker);
                sqBkDrop.Visibility = Visibility.Visible;
            }
        }

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Image.Save(FilePath);
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

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FilePath = FileManager.NewFile("PNG files (*.png)|*.png", Project.ProjectPath);
        }

        private void FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string path = (sender as TextBox).Text;

            btnExport.IsEnabled = FileManager.IsPathValid(path);
        }

        private void Extension_Changed(object sender, SelectionChangedEventArgs e)
        {
            var format = Image.Formats.FirstOrDefault(f => f.ShortName == e.AddedItems[0]);
            FilePath = IOPath.ChangeExtension(FilePath, format.ValidExtensions[0]);
            Image.FileFormat = format;
            Image.Encoder = format.Encoder;
            PixelFormats = Image.FileFormat.ValidPixelFormats.Select(pf => pf.ToString()).ToList();
            cbbxPixelFormats.SelectedIndex = 0;
        }

        private void PixelFormat_Changed(object sender, SelectionChangedEventArgs e)
        {
            Image.PixelFormat = Image.FileFormat.ValidPixelFormats.FirstOrDefault(f => f.ToString() == e.AddedItems[0].ToString());

            GetImagePreview();
        }

        private void SuperSampling_Changed(object sender, SelectionChangedEventArgs e)
        {
            var content = (e.AddedItems[0] as ComboBoxItem).Content;
            string value = content == null ? "OFF" : content.ToString().ToUpper();
            Image.SuperSampling = value == "OFF" ? 1 : int.Parse(value.Replace("X", ""));

            GetImagePreview();
        }

    }
}
