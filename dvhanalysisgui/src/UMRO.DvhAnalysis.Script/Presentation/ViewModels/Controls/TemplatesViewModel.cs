using System;
using System.Collections.Generic;
using System.Linq;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    public class TemplatesViewModel : BindableBase
    {
        // Holds all templates (current user's, other users', and global)
        public List<TemplateViewModel> Templates { get; private set; }

        // Holds current user's and global templates (to display)
        public IEnumerable<TemplateViewModel> UserTemplates
        {
            get { return Templates.Where(t => t.IsGlobal || t.OwnerUserName == UserName); }

            //load all templates, do not use th filters above
            //get { return Templates; }
        }

        private string UserName { get; }

        public event EventHandler<ProgressEventArgs> Progress;

        public TemplatesViewModel(string userName)
        {
            UserName = userName;
            InitializeTemplates();
        }

        private void InitializeTemplates()
        {
            try
            {
                Templates = new List<TemplateViewModel>
                    (from t in TemplateConfig.Load()
                     select new TemplateViewModel(t, UserName));
            }
            catch (Exception e)
            {
                Templates = new List<TemplateViewModel>();
                NotifyUserMessaged("Template Error",
                    "Failed to load any templates.\n" + e.Message, UserMessageType.Error);
            }
        }

        public void ApplyTemplate(TemplateViewModel template, ViewModel viewModel,
            IEnumerable<StructureMatchViewModel> structureMatches)
        {
            TemplateLoader templateLoader = new TemplateLoader(viewModel);
            templateLoader.Progress += (sender, args) => OnProgress(args);
            templateLoader.Load(template.Template, structureMatches);
        }

        public TemplateViewModel CreateTemplate(ViewModel viewModel)
        {
            return new TemplateViewModel(TemplateCreator.CreateTemplate(viewModel), viewModel.User.Id);
        }

        public void AddTemplate(TemplateViewModel template)
        {
            Templates.Add(template);
            NotifyPropertyChanged(nameof(UserTemplates));
        }

        public void RemoveTemplate(TemplateViewModel template)
        {
            Templates.Remove(template);
            NotifyPropertyChanged(nameof(UserTemplates));
        }

        public void SaveTemplates()
        {
            TemplateConfig.Save(from template in Templates
                                select template.Template);
        }

        protected virtual void OnProgress(ProgressEventArgs e)
        {
            var handler = Progress;
            if (handler != null) handler(this, e);
        }
    }
}
