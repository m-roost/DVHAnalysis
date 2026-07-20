using System.Linq;
using System.Windows;
using System.Windows.Data;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    public partial class TemplateDialog : Window
    {
        public TemplatesViewModel ViewModel { get; set; }

        public TemplateDialog(string userName)
        {
            InitializeComponent();
            InitializeViewModel(userName);
            InitializeCollectionView();
        }

        private void InitializeViewModel(string userName)
        {
            ViewModel = new TemplatesViewModel(userName);
            DataContext = ViewModel;
        }

        private void InitializeCollectionView()
        {
            // The collection view source doesn't automatically refresh
            // its sorting or grouping when a property changes,
            // so it must be done manually by listening to changes
            // to each template's properties. When a change happens,
            // the underlying view of the view source is refreshed.
            // This was important when grouping was based on IsGlobal,
            // but now that it's based on the owner's user name,
            // which doesn't change during the application, this is only
            // important for sorting, when the user changes the template's name.
            // Also, the selected item of the view is saved and reapplied
            // because refreshing the view forgets about the selected item.
            foreach (var template in ViewModel.UserTemplates)
            {
                template.PropertyChanged += (sender, args) =>
                {
                    var collectionView = ((CollectionViewSource)Resources["TemplatesView"]).View;
                    var selectedItem = collectionView.CurrentItem;
                    collectionView.Refresh();
                    collectionView.MoveCurrentTo(selectedItem);
                };
            }
        }

        private void SaveTemplatesAndClose(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Templates.Any
                (t => t.Name == TemplateCreator.NewTemplateDefaultName))
            {
                ViewModel.NotifyUserMessaged("Template error",
                    "Please rename the new template.", UserMessageType.Error);
            }
            else
            {
                ViewModel.SaveTemplates();
                DialogResult = true;
                Close();
            }
        }

        private void RemoveTemplate(object sender, RoutedEventArgs e)
        {
            if (GetSelectedTemplate() != null)
            {
                ViewModel.RemoveTemplate(GetSelectedTemplate());
            }
        }

        private TemplateViewModel GetSelectedTemplate()
        {
            return TemplateListBox.SelectedItem as TemplateViewModel;
        }
    }
}
