using System;
using System.ComponentModel;

namespace BRIE.Classes.Etc
{
    public class Output : INotifyPropertyChanged
    {
        private string? _text;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text
        {
            get { return _text ?? ""; }
            private set
            {
                if (_text != value)
                {
                    _text = value;
                    NotifyPropertyChanged(nameof(Text));
                }
            }
        }

        public void Write(string value)
        {
            Text += value;
        }

        public void WriteLine(string value)
        {
            Text += "> " + value + Environment.NewLine;
        }

        public void WriteLine()
        {
            Text += "> " + Environment.NewLine;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}