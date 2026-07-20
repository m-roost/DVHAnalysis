using System.Windows;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;

namespace UMRO.DvhAnalysis.Script.Presentation
{
    public static class UserMessager
    {
        public static void OnUserMessaged(object sender, UserMessagedEventArgs e)
        {
            MessageBoxImage icon;

            switch (e.Type)
            {
                case UserMessageType.Information:
                    icon = MessageBoxImage.Information;
                    break;

                case UserMessageType.Warning:
                    icon = MessageBoxImage.Warning;
                    break;

                case UserMessageType.Error:
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    icon = MessageBoxImage.None;
                    break;
            }

            MessageBox.Show(e.Message, e.Title, MessageBoxButton.OK, icon);
        }
    }
}
