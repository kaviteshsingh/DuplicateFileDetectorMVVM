using System;
using System.Windows.Data;
using System.Globalization;


namespace DuplicateFileDetectorMVVM.ValueConverters
{

    [ValueConversion(typeof(UInt64), typeof(UInt64))]
    public class FileSizeInBytesToKiloBytes : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UInt64 val = System.Convert.ToUInt64(value);

            return val / 1024;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UInt64 val = System.Convert.ToUInt64(value);

            return val * 1024;
        }
    }
}
