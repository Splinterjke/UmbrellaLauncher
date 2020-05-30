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
    internal class DateTimeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userData = (UserData)value;
            if (userData.SubEndTime.ToUniversalTime() < userData.ServerTime.ToUniversalTime() || userData.IsFreeDays || userData.IsLifeTime)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
