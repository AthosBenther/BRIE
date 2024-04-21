using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BRIE.Classes.Export.FileFormats.Meta;

namespace BRIE.Classes.Export.FileFormats
{
    public class Bmp : FileFormat, IFileFormat
    {
        public override string ShortName => "Bmp";
        public override string LongName => "Bitmap";
        public override List<string> ValidExtensions => new()
        {
            ".bmp"
        };

        public override List<PixelFormat> ValidPixelFormats => new List<PixelFormat>()
        {
            PixelFormats.Gray8
        };

        public override BitmapEncoder Encoder => new BmpBitmapEncoder();
    }
}
