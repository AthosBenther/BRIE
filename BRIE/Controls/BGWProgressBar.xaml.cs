using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BRIE.Controls
{
    /// <summary>
    /// Interaction logic for BGWProgressBar.xaml
    /// </summary>
    public partial class BGWProgressBar : UserControl, INotifyPropertyChanged
    {
        private BackgroundWorker bgw { get; set; }

        private bool _indeterminate = false;
        public bool Indeterminate
        {
            get => _indeterminate;
            set
            {
                if (_indeterminate != value)
                {
                    _indeterminate = value;
                    OnPropertyChanged(nameof(Indeterminate));
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {

                    _progress = value < 0 ? 0 : value;
                    Indeterminate = value < 0;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }
        private double _progress;
        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }
        private string _label;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

        public BGWProgressBar()
        {
            InitializeComponent();
            DataContext = this;
            bgw = new();
            Visibility = Visibility.Collapsed;
        }

        public void RunWorkAsync(BackgroundWorker BackgroundWorker)
        {
            bgw = BackgroundWorker;
            Visibility = Visibility.Visible;
            if (bgw.WorkerReportsProgress)
            {
                bgw.ProgressChanged += (o, p) =>
                {
                    Progress = p.ProgressPercentage;
                    try
                    {
                        if (p.UserState != null)
                        {
                            Label = p.UserState as string;
                        }
                        else
                        {
                            Label = p.ProgressPercentage.ToString() + "%";
                        }
                    }
                    catch (Exception ex)
                    {
                        (Parent as MainWindow).Output.WriteLine(ex.Message);
                    }
                };
            }
            bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
            bgw.RunWorkerAsync();
        }

        private void Bgw_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Timer timer = new Timer();
            timer.Elapsed += (o, e) => Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            timer.Stop();
                                            Visibility = Visibility.Collapsed;
                                            timer.Dispose();
                                        });

            timer.Interval = 3000;
            timer.Start();
            RunWorkerCompleted?.Invoke(this, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
