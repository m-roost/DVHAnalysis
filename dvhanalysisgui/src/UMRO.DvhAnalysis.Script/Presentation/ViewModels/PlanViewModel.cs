using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class PlanViewModel : BindableBase
    {
        public PlanningItem Plan { get; set; }

        public string Name { get { return Plan.Id; } }

        // When the program starts, all plans should be selected
        // by default; the user then unchecks unwanted plans
        private bool _isSelected = true;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }


        //public string GetPlanID
        //{
        //    get 
        //    {
        //        if(Plan is PlanSum psum)
        //        {
        //            return psum.Id;
        //        }
        //        else if(Plan is PlanSetup psetup)
        //        {
        //            return psetup.Id;
        //        }
            
        //    }
        //}

        public bool IsPlanSum
        {
            get { return Plan is PlanSum; }
        }

        public bool HasBeenSavedToDB { get; set; } = false;


    }



    public static class Helpers
    {

        //public static bool Get_PlanSum_Compoments(this PlanSum psum)
        //{
        //    psum.PlanSumComponents.Select(t => t.id);
        //}



        public static string check_if_all_compoments_loaded(this IList<PlanViewModel> planVMs)
        {
            if (planVMs.Any(t => t.IsPlanSum) == false) return "";

            List<string> plan_names = planVMs.Select(t => t.Plan.Id).ToList();

            string msg = "";

            var PSumList = planVMs.Where(t => t.IsPlanSum).ToList();

            foreach(var psum in PSumList)
            {
                List<string> compo_names = (psum.Plan as PlanSum).PlanSumComponents.Select(t => t.PlanSetupId).ToList();

                compo_names.ForEach(t =>
                {
                    if (!plan_names.Contains(t))
                    {
                        msg += $"PlanSum [{psum.Plan.Id}] has compoment [{t}], but it is not loaded or is hidden.\n";
                    }
                });
            }

            return msg;
        }



    }




}
