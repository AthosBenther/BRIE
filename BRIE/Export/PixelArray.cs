using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BRIE.ExportFormats
{
    public class PixelArray
    {
        private double[] _pixels;
        public double[] Pixels => _pixels;

        public PixelArray(int size)
        {
            _pixels = new double[size];
        }

        public PixelArray(string filePath)
        {
            
        }

        public void AddPixel(int index, double color)
        {
            _pixels[index] = color;
        }

        public Array GetPixels(PixelFormat format)
        {
            Array pix;

            switch (format.ToString())
            {
                case "Gray8":
                    pix = _pixels.Select(p => (byte)(int)p).ToArray();
                    break;
                case "Gray16":
                    pix = _pixels.Select(p => (ushort)p).ToArray();
                    break;
                case "Gray32Float":
                    pix = _pixels.Select(p => (int)p).ToArray();
                    break;
                default:
                    throw new FormatException("The provided PixelFormat is not supported");
            }

            return pix;
        }

    }
}
