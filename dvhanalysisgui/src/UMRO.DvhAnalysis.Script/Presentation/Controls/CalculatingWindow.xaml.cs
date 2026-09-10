using System;
using System.Collections.Specialized;
using System.Windows;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    public partial class CalculatingWindow : Window
    {
        public CalculatingWindow()
        {
            InitializeComponent();
        }

        private CalcProgressViewModel _viewModel;
        public CalcProgressViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                ConnectViewModel();
            }
        }

        private void ConnectViewModel()
        {
            DataContext = ViewModel;

            // Update the notification list in the view manually whenever it changes
            // in the view model, so that it may be updated in the UI thread
            ViewModel.Notifications.CollectionChanged += NotificationsOnCollectionChanged;
        }

        private void NotificationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                // Only handles adding notifications
                // because removing them should never happen
                foreach (var item in args.NewItems)
                {
                    NotificationListView.Items.Add(item);
                }
            }));
        }

        public void CloseIfNoNotifications()
        {
            if (ViewModel == null || !ViewModel.HasNotifications)
            {
                // Must use the current dispatcher because this method
                // is called from another thread (via an event handler)
                Dispatcher.Invoke(() =>
                {
                    // It's important to stop listening to this collection,
                    // or it will try to run the handler even if the window is closed
                    ViewModel.Notifications.CollectionChanged -= NotificationsOnCollectionChanged;

                    Close();
                });
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            // It's important to stop listening to this collection,
            // or it will try to run the handler even if the window is closed
            ViewModel.Notifications.CollectionChanged -= NotificationsOnCollectionChanged;

            Close();
        }
    }
}
