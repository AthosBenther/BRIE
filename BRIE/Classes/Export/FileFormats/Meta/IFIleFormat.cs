using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BRIE.Classes.Export.FileFormats.Meta
{
    interface IFileFormat
    {
        string ShortName { get; }
        string LongName { get; }
        string FileFilter { get; }
        List<string> ValidExtensions { get; }
        List<PixelFormat> ValidPixelFormats { get; }
        BitmapEncoder Encoder { get; }
    }
}
