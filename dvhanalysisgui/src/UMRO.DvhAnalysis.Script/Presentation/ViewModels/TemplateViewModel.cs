using System;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class TemplateViewModel : BindableBase
    {
        public Template Template { get; private set; }

        public string Name
        {
            get { return Template.Name; }
            set
            {
                Template.Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Description
        {
            get { return Template.Description; }
            set
            {
                Template.Description = value;
                NotifyPropertyChanged("Template");    // TODO: Should it say "Description"?
            }
        }

        public string OwnerUserName
        {
            get { return Template.OwnerUserName; }
            set
            {
                Template.OwnerUserName = value;
                NotifyPropertyChanged("OwnerUserName");
            }
        }

        public bool IsGlobal
        {
            get { return Template.IsGlobal; }
            set
            {
                Template.IsGlobal = value;
                NotifyPropertyChanged("IsGlobal");
            }
        }

        public string GroupName
        {
            get { return ShowIsGlobalCheckBox ? "Personal Templates" : "Public Templates"; }
        }

        public bool ShowIsGlobalCheckBox
        {
            get { return OwnerUserName == _userName; }
        }

        public TemplateViewModel(Template template, string userName)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template");
            }

            Template = template;
            _userName = userName;
        }

        private string _userName;
    }
}
