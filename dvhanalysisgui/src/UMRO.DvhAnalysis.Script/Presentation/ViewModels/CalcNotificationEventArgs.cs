using System;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class CalcNotificationEventArgs : EventArgs
    {
        public string Message { get; set; }
        public CalcNotificationType Type { get; set; }

        public CalcNotificationEventArgs(string message, CalcNotificationType type)
        {
            Message = message;
            Type = type;
        }
    }
}