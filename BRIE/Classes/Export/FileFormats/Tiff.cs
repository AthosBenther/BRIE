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
    public class Tiff : FileFormat, IFileFormat
    {
        public override string ShortName => "TIFF";
        public override string LongName => "Tag Image File Format";
        public override string FileFilter => "TIFF files (*.tiff)|*.tiff";
        public override List<string> ValidExtensions => new()
        {
            ".tiff",
            ".tif"
        };

        public override List<PixelFormat> ValidPixelFormats => new List<PixelFormat>()
        {
            PixelFormats.Gray8,
            PixelFormats.Gray16,
            //PixelFormats.Gray32Float
        };

        public override BitmapEncoder Encoder => new TiffBitmapEncoder();
    }
}
