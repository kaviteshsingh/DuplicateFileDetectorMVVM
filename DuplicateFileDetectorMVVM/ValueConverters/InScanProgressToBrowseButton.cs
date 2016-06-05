using System;
using System.Windows.Data;
using System.Globalization;

namespace DuplicateFileDetectorMVVM.ValueConverters
{

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InScanProgressToBrowseButton : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsScanInProgress = (bool)value;
            if(IsScanInProgress)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsBrowseButtonEnabled = (bool)value;
            if(IsBrowseButtonEnabled)
                return false;
            else
                return true;
        }
    }
}
