using System;
using System.ComponentModel;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // Event to send message to the user
        public event EventHandler<UserMessagedEventArgs> UserMessaged;

        public void NotifyUserMessaged(string title, string message, UserMessageType type)
        {
            if (UserMessaged != null)
            {
                UserMessaged(this, new UserMessagedEventArgs(title, message, type));
            }
        }

        public BindableBase()
        {
            UserMessaged += UserMessager.OnUserMessaged;
        }
    }
}
