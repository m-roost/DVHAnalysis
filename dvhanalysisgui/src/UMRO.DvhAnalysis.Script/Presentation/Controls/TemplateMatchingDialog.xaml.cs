using System;
using System.Collections.Generic;
using System.Windows;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    public partial class TemplateMatchingDialog : Window
    {
        // Template and PatientStructureIds must be set
        public new Template Template    // The "new" hides Windows.Template (not using)
        {
            get
            {
                if (ViewModel != null)
                {
                    return ViewModel.Template;
                }
                else
                {
                    throw new InvalidOperationException
                        ("Template matching view model must be initialized.");
                }
            }
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.Template = value;
                }
                else
                {
                    throw new InvalidOperationException
                        ("Template matching view model must be initialized.");
                }
            }
        }

        public string[] PatientStructureIds
        {
            get
            {
                if (ViewModel != null)
                {
                    return ViewModel.PatientStructureIds;
                }
                else
                {
                    throw new InvalidOperationException
                        ("Template matching view model must be initialized.");
                }
            }
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.PatientStructureIds = value;
                }
                else
                {
                    throw new InvalidOperationException
                        ("Template matching view model must be initialized.");
                }
            }
        }

        public ICollection<StructureMatchViewModel> StructureMatches { get; private set; }

        private TemplateMatchingViewModel ViewModel { get; set; }

        public TemplateMatchingDialog()
        {
            InitializeComponent();
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ViewModel = new TemplateMatchingViewModel();
            DataContext = ViewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            StructureMatches = ViewModel.StructureMatches;

            DialogResult = true;
            Close();
        }
    }
}
