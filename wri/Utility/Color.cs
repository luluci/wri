using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Utility
{
    internal static class Color
    {
        //System.Windows.Media(WPF)とSystem.Drawing.Color(WinForms)

        public static System.Windows.Media.Color DefaultColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF000000");
        public static System.Windows.Media.Color ToColorOrDefault(this string code) => ToColorOrNull(code) ?? DefaultColor;

        public static System.Windows.Media.Color? ToColorOrNull(this string code)
        {
            try
            {
                return (System.Windows.Media.Color)ColorConverter.ConvertFromString(code);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static SolidColorBrush ToSolidColorBrush(this string mColor)
        {
            return mColor.ToColorOrDefault().ToSolidColorBrush();
        }

        public static SolidColorBrush ToSolidColorBrush(this System.Windows.Media.Color mColor)
        {
            return new SolidColorBrush(mColor);
        }
    }
}