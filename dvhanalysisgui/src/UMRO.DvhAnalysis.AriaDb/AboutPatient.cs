namespace UMRO.DvhAnalysis.AriaDb
{
    public class AboutPatient
    {
        public AboutPatient(string id, int PatientSer)
        {
            Id = id;
            this.PatientSer = PatientSer;
        }

        public string Id { get; }
        public int PatientSer { get; }
    }



    public class AboutCourse
    {
        public AboutCourse(string CourseID, int PatientSer, int CourseSer)
        {
            this.CourseID = CourseID;
            this.PatientSer = PatientSer;
            this.CourseSer = CourseSer;
        }

        public string CourseID { get; }
        public int PatientSer { get; }
        public int CourseSer { get; }
    }



    public class AboutPlanSum
    {
        public AboutPlanSum(string Id, int CourseSer, int PlanSumSer, int ImageSer)
        {
            this.Id = Id;
            this.CourseSer = CourseSer;
            this.PlanSumSer = PlanSumSer;
            this.ImageSer = ImageSer;
        }

        public string Id { get; }
        public int CourseSer { get; }
        public int PlanSumSer { get; }
        public int ImageSer { get; }

    }


    public class AboutPlanSetup
    {
        public AboutPlanSetup(string Id, int CourseSer, int PlanSetupSer, int StructureSetSer)
        {
            this.Id = Id;
            this.CourseSer = CourseSer;
            this.PlanSetupSer = PlanSetupSer;
            this.StructureSetSer = StructureSetSer;
        }

        public string Id { get; }
        public int CourseSer { get; }
        public int PlanSetupSer { get; }
        public int StructureSetSer { get; }
    }




}