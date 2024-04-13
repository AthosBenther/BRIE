using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BRIE.Etc;
using BRIE.ExportFormats.FileFormats;
using BRIE.ExportFormats.FileFormats.Meta;
using static BRIE.Roads;

namespace BRIE.ExportFormats
{
    public class PixelArray
    {
        private double[] pixels;
        public double[] Pixels => pixels;

        public PixelArray(int size)
        {
            pixels = new double[size];
        }

        public void AddPixel(int index, double color)
        {
            pixels[index] = color;
        }

        public Array GetPixels(PixelFormat format)
        {
            Array pix;

            switch (format.ToString())
            {
                case "Gray8":
                    pix = pixels.Select(p =>(byte)(int)p).ToArray();
                    break;
                case "Gray16":
                    pix = pixels.Select(p => (ushort)p).ToArray();
                    break;
                case "Gray32Float":
                    pix = pixels.Select(p => (int)p).ToArray();
                    break;
                default:
                    throw new FormatException("The provided PixelFormat is not supported");
            }

            return pix;
        }

    }
}
