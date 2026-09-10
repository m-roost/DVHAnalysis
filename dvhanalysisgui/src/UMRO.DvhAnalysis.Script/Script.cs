using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Shapes;
using UMRO.DvhAnalysis.AriaDb.Sql;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using UMRO.DvhAnalysis.Script;
using UMRO.DvhAnalysis.Script.Presentation.Controls;
using VMS.TPS.Common.Model.API;
using Path = System.IO.Path;

namespace VMS.TPS
{
    public class Script
    {
        private ILogger _logger;

        public void Execute(ScriptContext scriptContext)
        {
            Run(scriptContext.CurrentUser,
                scriptContext.Patient,
                scriptContext.Image,
                scriptContext.StructureSet,
                scriptContext.PlanSetup,
                scriptContext.PlansInScope,
                scriptContext.PlanSumsInScope, null);

        }

        public void Run(
            User user,
            Patient patient,
            Image image,
            StructureSet structureSet,
            PlanSetup planSetup,
            IEnumerable<PlanSetup> planSetupsInScope,
            IEnumerable<PlanSum> planSumsInScope,
            Window window)
        {
            // Manually read local NLog.config file (avoids interference with other plug-in scripts)
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assemblyDir, "NLog.config");
            AppLog.Configure(configPath, AssemblySettings.LogDir);
            _logger = AppLog.GetLogger("MainLogger");
            _logger.Info("Script started");

            window = new Window();
            window.Title = "DVH Analysis";
            window.Width = 1280;
            window.Height = 1024;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Closed += WindowOnClosed;

            try
            {
                // TODO: Verify that there is an active plan;
                // otherwise, the code trying to get the course fails

                VerifyPlanSumsStructureSetsAreEqual(planSumsInScope);

                var plansInScope_all = GetPlanningItemsInScope(planSetupsInScope, planSumsInScope);

                var plansInScope = OnlyKeepPlansHavingDose(plansInScope_all);

                var planitem_names = plansInScope.Select(t => t.Id).OrderBy(t => t).ToList();

                // Check plan name duplications
                if (planitem_names.Count() > planitem_names.Distinct().ToList().Count())
                {
                    string allNames = string.Join("; ", planitem_names);

                    MessageBox.Show($"Some loaded plans/plansums have the same name:\n\n {allNames}\n\nPlease make sure each plan/plansum has a unique name.\n\nScript quits.");

                    return;
                }


                PlanningItem plan = GetActivePlan(planSetup, planSumsInScope);

                if (plan == null)
                {
                    throw new Exception("No active plan or plansum found. Please open a plan or plansum in Eclipse first.");
                }

                Course course = plan.GetCourse();

                // Note: This replaces the context structure set (passed into this method)
                // with that of the active plan or plan sum. They're supposed to be the same,
                // but I need to confirm that by testing.
                structureSet = plan.GetStructureSet();

                _logger.Info($"Loaded patient {patient.Id}, active plan '{plan.Id}', " +
                             $"absolute dose unit = {GetAbsoluteDoseUnitName(plan)}");

                MainView mainView = new MainView
                {
                    User = user,
                    Patient = patient,
                    Course = course,
                    Plan = plan,
                    Plans = plansInScope.ToList(),
                    StructureSet = structureSet,

                    InfoQueryRepository = new QueryRepository(GetAriaDbConnectionString()),

                    OncologistRepository = new OncologistRepository(GetAriaDbConnectionString()),
                    UserRepository = new UserRepository(GetAriaSharedFrameworkDbConnectionString())
                };

                window.Content = mainView;

                // Clear the DVH cache each time the script starts
                // because there can be left overs form the previous run
                // (since Eclipse loads the script once into memory)
                DVHAnalysis.DVHCache.DefaultCache.Clear();

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - DVH Analysis",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.Fatal("Fatal error", ex);
            }
        }

        // The plan's absolute dose unit (Gy or cGy), for startup diagnostics. TotalDose is
        // always absolute (unlike Dose.DoseMax3D, which follows the current presentation).
        private string GetAbsoluteDoseUnitName(PlanningItem plan)
        {
            if (plan is PlanSetup)
            {
                return ((PlanSetup)plan).TotalDose.Unit.ToString();
            }

            return ((PlanSum)plan).PlanSetups.First().TotalDose.Unit.ToString();
        }

        private string GetAriaDbConnectionString()
        {
            return AssemblySettings.AriaConnectionString;
        }


        private string GetAriaSharedFrameworkDbConnectionString()
        {
            return AssemblySettings.AriaSfConnectionString;
        }

        private void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            _logger.Info("Script ended");
        }

        private List<PlanningItem> OnlyKeepPlansHavingDose(IEnumerable<PlanningItem> plans)
        {
            var badPlans = plans.Where(p => p.Dose == null).ToList();

            if (badPlans.Any())
            {
                string badPlanNames = string.Join("; ", badPlans.Select(t => t.Id));

                MessageBox.Show($"The following plans [{badPlanNames}] don't have dose calculated yet. They will be filtered out.");
            }

            var goodPlans = plans.Where(p => p.Dose != null).ToList();

            return goodPlans;
        }

        private IEnumerable<PlanningItem> GetPlanningItemsInScope
            (IEnumerable<PlanSetup> planSetupsInScope, IEnumerable<PlanSum> planSumsInScope)
        {
            return planSetupsInScope
                .Concat<PlanningItem>(planSumsInScope);
        }

        private PlanningItem GetActivePlan(PlanSetup planSetup, IEnumerable<PlanSum> planSumsInScope)
        {
            if (planSetup != null)
            {
                return planSetup;
            }
            else if (planSumsInScope != null)
            {
                return planSumsInScope.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        #region Verify that plan sum structure sets are the same

        // A plan sum should use the same structure set of its plans
        private static void VerifyPlanSumsStructureSetsAreEqual
            (IEnumerable<PlanSum> planSums)
        {
            if (planSums == null)
            {
                return;
            }

            foreach (PlanSum planSum in planSums)
            {
                VerifyPlanSumStructureSetsAreEqual(planSum);
            }
        }

        private static void VerifyPlanSumStructureSetsAreEqual(PlanSum planSum)
        {
            if (!StructureSetsAreEqual(planSum))
            {
                MessageBox.Show("Plan sum " + planSum.Id + " contains plans " +
                    "with different structure sets. Metric calculations may not " +
                    "be correct for this plan sum.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static bool StructureSetsAreEqual(PlanSum planSum)
        {
            return StringsAreEqual(GetStructureSetIds(planSum));
        }

        private static IEnumerable<string> GetStructureSetIds(PlanSum planSum)
        {
            return from plan in planSum.PlanSetups
                   select plan.StructureSet.Id;
        }

        private static bool StringsAreEqual(IEnumerable<string> strings)
        {
            return strings.All(s => s == strings.First());
        }

        #endregion // Verify that plan sum structure sets are the same
    }
}
