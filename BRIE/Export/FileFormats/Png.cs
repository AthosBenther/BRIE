using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BRIE.ExportFormats.FileFormats.Meta;

namespace BRIE.ExportFormats.FileFormats
{
    public class Png : FileFormat, IFileFormat
    {
        public override string ShortName => "Png";
        public override string LongName => "Portable Network Graphics";
        public override string FileFilter => "PNG files (*.png)|*.png";
        public override List<string> ValidExtensions => new()
        {
            ".png"
        };

        public override List<PixelFormat> ValidPixelFormats => new List<PixelFormat>()
        {
            PixelFormats.Gray8,
            PixelFormats.Gray16,
            //PixelFormats.Gray32Float
        };

        public override BitmapEncoder Encoder => new PngBitmapEncoder();
    }
}
