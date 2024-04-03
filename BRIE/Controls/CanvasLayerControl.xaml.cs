using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BRIE.Controls
{
    /// <summary>
    /// Interaction logic for CanvasLayerControl.xaml
    /// </summary>
    public partial class CanvasLayerControl : UserControl
    {
        public static readonly DependencyProperty TargetCanvasProperty = DependencyProperty.Register(
            "TargetCanvas", typeof(Canvas), typeof(CanvasLayerControl), new PropertyMetadata(null));
        public Canvas TargetCanvas
        {
            get { return (Canvas)GetValue(TargetCanvasProperty); }
            set { SetValue(TargetCanvasProperty, value); }
        }
        public bool CanOpenFile
        {
            get { return (bool)GetValue(CanOpenFileProperty); }
            set
            {
                btnOpenFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(CanOpenFileProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for CanOpenFile.  This enables animation, styling, binding, etc...
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
            }
        }

        // Using a DependencyProperty as the backing store for CanSave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanSaveFileProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(null));



        public bool CanExportFile
        {
            get { return (bool)GetValue(CanExportFileProperty); }
            set
            {
                btnExportFile.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(CanExportFileProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for CanExportFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanExportFileProperty =
            DependencyProperty.Register("CanExport", typeof(bool), typeof(CanvasLayerControl), new PropertyMetadata(null));



        public event RoutedEventHandler OpenFileClick;
        public event RoutedEventHandler SaveFileClick;
        public event RoutedEventHandler SaveCopyFileClick;
        public event RoutedEventHandler ExportFileClick;
        public string Label { get; set; }


        public CanvasLayerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OpenFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileClick?.Invoke(this, e);
        }

        private void btnLayerVisibility_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button source = (Button)sender;

            bool isVisible = TargetCanvas.Visibility == System.Windows.Visibility.Visible;

            TargetCanvas.Visibility = isVisible ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            source.Foreground = isVisible ? Brushes.Red : Brushes.Black;
            source.Content = isVisible ? "\xE738" : "\xE7B3";
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
    }
}
