using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BRIE.Controls
{
    /// <summary>
    /// Interaction logic for CanvasLayerControl.xaml
    /// </summary>
    public partial class CanvasLayerControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TargetCanvasProperty = DependencyProperty.Register(
            "TargetCanvas", typeof(Canvas), typeof(CanvasLayerControl), new PropertyMetadata(null));
        public Canvas TargetCanvas
        {
            get { return (Canvas)GetValue(TargetCanvasProperty); }
            set
            {
                SetValue(TargetCanvasProperty, value);
                OnPropertyChanged(nameof(TargetCanvas));
            }
        }

        public bool CanOpenFile
        {
            get { return (bool)GetValue(CanOpenFileProperty); }
            set
            {
                btnOpenFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(CanOpenFileProperty, value);
                OnPropertyChanged(nameof(CanOpenFile));
            }
        }

        public static readonly DependencyProperty CanOpenFileProperty =
            DependencyProperty.Register("CanOpen", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(null));

        public bool CanSaveFile
        {
            get { return (bool)GetValue(CanSaveFileProperty); }
            set
            {
                btnSaveFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                btnSaveCopyFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(CanSaveFileProperty, value);
                OnPropertyChanged(nameof(CanSaveFile));
            }
        }

        public static readonly DependencyProperty CanSaveFileProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(null));

        public bool CanExportFile
        {
            get { return (bool)GetValue(CanExportFileProperty); }
            set
            {
                btnExportFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(CanExportFileProperty, value);
                OnPropertyChanged(nameof(CanExportFile));
            }
        }

        public static readonly DependencyProperty CanExportFileProperty =
            DependencyProperty.Register("CanExport", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(null));

        public CanvasLayerControl? ParentLayer
        {
            get { return (CanvasLayerControl)GetValue(ParentLayerProperty); }
            set
            {
                SetValue(ParentLayerProperty, value);
                OnPropertyChanged(nameof(ParentLayer));
            }
        }

        public static readonly DependencyProperty? ParentLayerProperty =
            DependencyProperty.Register("ParentLayer", typeof(CanvasLayerControl), typeof(CanvasLayerControl), null);

        public bool IsLayerVisible
        {
            get { return TargetCanvas.Visibility == Visibility.Visible; }
            set
            {
                SetValue(IsLayerVisibleProperty, value);
                TargetCanvas.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                btnLayerVisibility.Foreground = value ? Brushes.Black : Brushes.Red;
                btnLayerVisibility.Content = value ? "\xE7B3" : "\xE738";
                OnPropertyChanged(nameof(IsLayerVisible));
            }
        }

        public static readonly DependencyProperty IsLayerVisibleProperty =
            DependencyProperty.Register("IsLayerVisible", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(true));

        public event RoutedEventHandler? OpenFileClick;
        public event RoutedEventHandler? SaveFileClick;
        public event RoutedEventHandler? SaveCopyFileClick;
        public event RoutedEventHandler? ExportFileClick;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Label { get; set; }


        public CanvasLayerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Parent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IsLayerVisible) && ParentLayer != null) {
                IsEnabled = ParentLayer.IsLayerVisible;
                TargetCanvas.Visibility = ParentLayer.IsLayerVisible? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OpenFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileClick?.Invoke(this, e);
        }

        private void btnLayerVisibility_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLayerVisible = !IsLayerVisible;
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileClick?.Invoke(this, e);
        }

        private void SaveCopyFile_Click(object sender, RoutedEventArgs e)
        {
            SaveCopyFileClick?.Invoke(this, e);
        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            ExportFileClick?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
