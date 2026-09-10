using System;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public enum UserMessageType
    {
        Information,
        Warning,
        Error
    }

    public class UserMessagedEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public UserMessageType Type { get; set; }

        public UserMessagedEventArgs(string title, string message, UserMessageType type)
        {
            Title = title;
            Message = message;
            Type = type;
        }
    }
}
