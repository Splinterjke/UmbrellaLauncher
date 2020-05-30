using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UmbrellaProject.Models;

namespace UmbrellaProject.Converters
{
    class ScriptConfigToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scriptConfig = (ScriptConfigModel)value;
            if (!string.IsNullOrEmpty(scriptConfig.RepositoryPath) || !string.IsNullOrEmpty(scriptConfig.ScriptPath))
                return "GithubCircle";
            return "Laptop";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
