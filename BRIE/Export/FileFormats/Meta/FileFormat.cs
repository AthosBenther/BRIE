using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BRIE.ExportFormats.FileFormats.Meta
{
    public class FileFormat : IFileFormat
    {

        public virtual string ShortName { get; }
        public virtual string LongName { get; }
        public virtual string FileFilter { get; }
        public virtual List<string> ValidExtensions { get; }
        public virtual List<PixelFormat> ValidPixelFormats { get; }
        public virtual BitmapEncoder Encoder { get; }

        private string GetFileFilter()
        {
            // Assuming ValidExtensions property is implemented later
            if (ValidExtensions == null || ValidExtensions.Count == 0)
            {
                return string.Empty; // Return empty string if no extensions are defined
            }

            // Build the filter string
            var filterBuilder = new StringBuilder();
            filterBuilder.Append($"{ShortName} (");

            var commaSeparatedExtensions = string.Join(";*.", ValidExtensions); // Join extensions with ;*. prefix
            filterBuilder.Append($"*.{commaSeparatedExtensions})|*.{commaSeparatedExtensions}");

            return filterBuilder.ToString();
        }
    }
}
