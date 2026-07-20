using System;
using System.Collections.Generic;
using System.Linq;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script
{
    public class TemplateLoader
    {
        public ViewModel ViewModel { get; set; }

        public event EventHandler<ProgressEventArgs> Progress;

        public TemplateLoader(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public void Load(Template template, IEnumerable<StructureMatchViewModel> structureMatches)
        {
            int nStructureMetrics = template.StructureMetrics.Count();
            int i = 0;

            foreach (StructureMetric structureMetric in template.StructureMetrics)
            {
                i++;

                if (Progress != null)
                {
                    Progress(this, new ProgressEventArgs((double) i/nStructureMetrics));
                }

                string structureId = (from sm in structureMatches
                    where sm.TemplateStructureId == structureMetric.StructureId
                    select sm.PatientStructureId).FirstOrDefault();

                if (structureId != null)
                {
                    StructureViewModel structure = FindStructure
                        (ViewModel.StructureViewModels, structureId);

                    if (structure != null)
                    {
                        ViewModel.AddMetricRowForStructure(structure);
                        MetricRowViewModel metricRow = ViewModel.SelectedMetricRow;

                        DVHMetricSetupViewModel dvhMetricSetupViewModel =
                            new DVHMetricSetupViewModel
                            {
                                DVHMetricSetup = structureMetric.DVHMetricSetup
                            };

                        metricRow.AllDVHMetricSetups.Add(dvhMetricSetupViewModel);
                        metricRow.SelectedDVHMetricSetupViewModel = dvhMetricSetupViewModel;
                    }
                }
            }
        }

        private StructureViewModel FindStructure
            (IEnumerable<StructureViewModel> structures, string structureName)
        {
            return structures.FirstOrDefault(s => s.Id == structureName);
        }
    }
}
