using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script
{
    public static class TemplateCreator
    {
        public const string NewTemplateDefaultName = "<New template>";

        public static Template CreateTemplate(ViewModel viewModel)
        {
            Template template = new Template
            {
                Name = "<New template>",
                OwnerUserName = viewModel.User.Id,
                IsGlobal = false
            };

            foreach (var metricRow in viewModel.MetricRows)
            {
                if (metricRow.SelectedDVHMetricSetupViewModel != null)
                {
                    template.Add(metricRow.StructureViewModel.Id,
                        metricRow.SelectedDVHMetricSetupViewModel.DVHMetricSetup);
                }
            }

            return template;
        }
    }
}
