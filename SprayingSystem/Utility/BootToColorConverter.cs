using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace SprayingSystem.Utility
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Black border if value is FALSE (not edited)
            var sdColor = Color.Black;

            var isEdited = (bool)value;
            if (isEdited)
                // OrangeRed border if value is TRUE (edited)
                sdColor = Color.OrangeRed;

            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(sdColor.A, sdColor.R, sdColor.G, sdColor.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
