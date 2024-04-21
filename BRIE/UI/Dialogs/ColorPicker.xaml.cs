using System;
using System.Collections.Generic;
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

namespace BRIE.Dialogs
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public Color oldColor;
        public Color newColor;
        public ColorPicker()
        {
            InitializeComponent();
        }

        private void iptHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = iptHex.Text;
            if (input.Length == 8) // Check if the input string has the correct length
            {
                try
                {
                    // Parse RGBA components from the input string
                    int red = Convert.ToInt32(input.Substring(0, 2), 16);
                    int green = Convert.ToInt32(input.Substring(2, 2), 16);
                    int blue = Convert.ToInt32(input.Substring(4, 2), 16);
                    int alpha = Convert.ToInt32(input.Substring(6, 2), 16);

                    Console.WriteLine($"RGBA values: R={red}, G={green}, B={blue}, A={alpha}");
                    // You can use these values to create a Color object or perform other operations
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input format.");
                }
            }
            else
            {
               
            }
        }
    }
}
