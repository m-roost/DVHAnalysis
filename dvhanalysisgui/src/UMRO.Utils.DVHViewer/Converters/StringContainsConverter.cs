// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Utils.DVHViewer-0.9.3.0.dll (version 0.9.3.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System;
using System.Globalization;
using System.Windows.Data;

namespace UMRO.Utils.DVHViewer.Converters
{
    internal class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            string value2 = parameter as string;
            return text.Contains(value2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
