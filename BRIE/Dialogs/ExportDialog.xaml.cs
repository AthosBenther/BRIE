﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using BRIE.Export;
using BRIE.Export;
using BRIE.ExportFormats;
using BRIE.ExportFormats.FileFormats.Meta;
using Image = BRIE.Export.Image;

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


        public Roads Roads;
        public ExportDialog(Roads roads)
        {
            InitializeComponent();
            DataContext = this;


            Roads = roads;
        }

        private void GetImagePreview()
        {
            var worker = Image.RenderWorker(Roads);

            BmpFrame = null;

            bgwpb.RunWorkerCompleted += (obj, arg) =>
            {
                BmpFrame = Image.RenderImage();
            };
            bgwpb.RunWorkAsync(worker);
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
    }
}
