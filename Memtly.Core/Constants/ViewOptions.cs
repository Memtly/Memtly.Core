namespace Memtly.Core.Constants
{
    public class ViewOptions
    {
        public static IDictionary<string, string> YesNo = new Dictionary<string, string>()
        {
            { "Yes", "true" },
            { "No", "false" }
        };

        public static IDictionary<string, string> YesNoInverted = new Dictionary<string, string>()
        {
            { "Yes", "false" },
            { "No", "true" }
        };

        public static IDictionary<string, string> SingleGalleryMode = new Dictionary<string, string>()
        {
            { "Single", "true" },
            { "Multiple", "false" }
        };

        public static IDictionary<string, string> GallerySelectorDropdown = new Dictionary<string, string>()
        {
            { "Dropdown", "true" },
            { "Input", "false" }
        };

        public static IDictionary<string, string> GalleryWidth = new Dictionary<string, string>()
        {
            { "Full Width", "true" },
            { "Default", "false" }
        };

        public static IDictionary<string, string> GalleryDefaultView = new Dictionary<string, string>()
        {
            { "Default", "0" },
            { "Presentation", "1" },
            { "Slideshow", "2" },
            { "Single", "3" }
        };

        public static IDictionary<string, string> GalleryDefaultGroup = new Dictionary<string, string>()
        {
            { "None", "0" },
            { "Date", "1" },
            { "MediaType", "2" },
            { "Uploader", "3" }
        };

        public static IDictionary<string, string> GalleryDefaultOrder = new Dictionary<string, string>()
        {
            { "Ascending", "0" },
            { "Descending", "1" },
            { "Random", "2" }
        };

        public static IDictionary<string, string> GalleryDefaultFilter = new Dictionary<string, string>()
        {
            { "All", "0" },
            { "Images", "1" },
            { "Videos", "2" },
            { "Landscape", "3" },
            { "Portrait", "4" },
            { "Square", "5" }
        };

        public static IDictionary<string, string> UmamiMaskLevel = new Dictionary<string, string>()
        {
            { "None", "0" },
            { "Moderate", "1" },
            { "Strict", "2" }
        };
    }
}