using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BRIE.ExportFormats
{
    internal class PixelArray
    {
        PixelFormat PixelFormat;
        int pixelMaxValue;

        public PixelArray(PixelFormat pixelFormat)
        {
            PixelFormat = pixelFormat;
            pixelMaxValue = pixelFormat.BitsPerPixel;
        }
    }
}
