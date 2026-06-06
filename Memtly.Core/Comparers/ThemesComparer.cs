using Memtly.Core.Enums;

namespace Memtly.Core.Comparators
{
    public class ThemesComparer : IComparer<Themes>
    {
        private static readonly Dictionary<Themes, int> _order = new()
        {
            { Themes.AutoDetect, 0 },
            { Themes.Blue,       1 },
            { Themes.DarkBlue,   2 },
            { Themes.Green,      3 },
            { Themes.DarkGreen,  4 },
            { Themes.Red,        5 },
            { Themes.DarkRed,    6 },
            { Themes.Pink,       7 },
            { Themes.DarkPink,   8 },
            { Themes.Custom,     9 },
        };

        public int Compare(Themes x, Themes y)
        {
            return _order[x].CompareTo(_order[y]);
        }
    }
}