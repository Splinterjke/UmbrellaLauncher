using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using UmbrellaProject.Models;

namespace UmbrellaProject.Converters
{
    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object obj, Type targetType, object parameter, CultureInfo culture)
        {
            var value = obj as PrivateScriptModel;
            if (!string.IsNullOrEmpty(value.ForumUrl) || !string.IsNullOrEmpty(value.ForumUrlEN))
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
