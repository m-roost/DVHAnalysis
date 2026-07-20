using System;
using System.Globalization;
using System.Windows.Data;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    // Converts a DVHModel to bool based on its type (passed as parameter)
    public class DVHModelTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is StandardDVHModel)
            {
                return parameter.Equals(DVHModelTypes.Standard);
            }
            else if (value is LQBioDoseDVHModel)
            {
                return parameter.Equals(DVHModelTypes.LQBioDose);
            }
            else if (value is LQLBioDoseDVHModel)
            {
                return parameter.Equals(DVHModelTypes.LQLBioDose);
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? CreateDVHModel((DVHModelTypes)parameter) : Binding.DoNothing;
        }

        private DVHModel CreateDVHModel(DVHModelTypes dvhModelType)
        {
            switch (dvhModelType)
            {
                case DVHModelTypes.Standard:
                    return new StandardDVHModel();

                case DVHModelTypes.LQBioDose:
                    return new LQBioDoseDVHModel();

                case DVHModelTypes.LQLBioDose:
                    return new LQLBioDoseDVHModel();
            }

            return null;
        }
    }
}
