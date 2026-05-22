using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memtly.Core.Views.Account.Settings.Themes
{
    public class ColourOverridesModel : PageModel
    {
        public ColourOverridesModel()
        {
        }

        /// <summary>
        /// Navbar
        /// </summary>
        public string NavbarTitleText { get; set; } = "#282828";
        public string NavbarBackground { get; set; } = "#FFFFFF";

        /// <summary>
        /// Primary 1
        /// </summary>
        public string PrimaryText1 { get; set; } = "#FFFFFF";
        public string PrimaryBackground1 { get; set; } = "#2563eb";
        public string PrimaryBackgroundHover1 { get; set; } = "#1d4ed8";

        /// <summary>
        /// Primary 2
        /// </summary>
        public string PrimaryText2 { get; set; } = "#FFFFFF";
        public string PrimaryBackground2 { get; set; } = "#3b82f6";
        public string PrimaryBackgroundHover2 { get; set; } = "#2563eb";

        /// <summary>
        /// Primary 3
        /// </summary>
        public string PrimaryText3 { get; set; } = "#FFFFFF";
        public string PrimaryBackground3 { get; set; } = "#60a5fa";
        public string PrimaryBackgroundHover3 { get; set; } = "#3b82f6";

        /// <summary>
        /// Secondary 1
        /// </summary>
        public string SecondaryText1 { get; set; } = "#FFFFFF";
        public string SecondaryBackground1 { get; set; } = "#6366f1";
        public string SecondaryBackgroundHover1 { get; set; } = "#4f46e5";

        /// <summary>
        /// Secondary 2
        /// </summary>
        public string SecondaryText2 { get; set; } = "#FFFFFF";
        public string SecondaryBackground2 { get; set; } = "#818cf8";
        public string SecondaryBackgroundHover2 { get; set; } = "#6366f1";

        public void OnGet()
        {
        }
    }
}