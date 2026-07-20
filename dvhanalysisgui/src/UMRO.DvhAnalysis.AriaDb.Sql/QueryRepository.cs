using System.Data.SqlClient;

namespace UMRO.DvhAnalysis.AriaDb.Sql
{
    public class QueryRepository 
    {
        private readonly string _connectionString;

        public QueryRepository(string connectionString)
        {
            _connectionString = connectionString;

            if(string.IsNullOrEmpty(connectionString?.Trim()))
            {
                is_aria_connected = false;
            }
        }

        private bool is_aria_connected = true;

        public AboutPatient LoadPatientInfoById(string PatientId)
        {
            AboutPatient aboutPatient = new AboutPatient(PatientId, 0);

            if (is_aria_connected == false) return aboutPatient;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "SELECT top 1 [PatientSer],[PatientId],[CreationDate],[HstryDateTime],[FirstName],[LastName] FROM Patient where PatientId = @PatientId";

                var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@PatientId", PatientId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var PatientSer = int.Parse(reader["PatientSer"].ToString());
                        var MRN = reader["PatientId"].ToString();

                        aboutPatient = new AboutPatient(MRN, PatientSer);
                    }
                }

                connection.Close();
            }

            return aboutPatient;
        }



        public AboutCourse LoadCourseInfo(int PatientSer, string CourseID)
        {
            AboutCourse res = new AboutCourse(
                            CourseID: CourseID,
                            PatientSer: PatientSer,
                            CourseSer: 0
                            );

            if (is_aria_connected == false) return res;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "select top 1 c.PatientSer, c.CourseSer, c.CourseId, c.StartDateTime, c.CompletedDateTime, c.ClinicalStatus from VARIAN.dbo.Course c where PatientSer = @PatientSer and CourseId = @CourseId";

                var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@PatientSer", PatientSer);
                command.Parameters.AddWithValue("@CourseId", CourseID);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res = new AboutCourse(
                            CourseID: CourseID,
                            PatientSer: int.Parse(reader["PatientSer"].ToString()),
                            CourseSer: int.Parse(reader["CourseSer"].ToString())
                        );
                    }
                }

                connection.Close();
            }
         
            return res;
        }


        public AboutPlanSum LoadPlanSumInfo(int CourseSer, string PlanSumId)
        {
            AboutPlanSum res = new AboutPlanSum(
                        Id: PlanSumId,
                        CourseSer: CourseSer,
                        PlanSumSer: 0,
                        ImageSer: 0
                    );

            if (is_aria_connected == false) return res;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "select top 100 ps.PlanSumSer, ps.PlanSumId, ps.CourseSer, ps.ImageSer from PlanSum ps where CourseSer = @CourseSer and PlanSumId = @PlanSumId";

                var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@CourseSer", CourseSer);
                command.Parameters.AddWithValue("@PlanSumId", PlanSumId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res = new AboutPlanSum(
                            Id: PlanSumId,
                            CourseSer: int.Parse(reader["CourseSer"].ToString()),
                            PlanSumSer: int.Parse(reader["PlanSumSer"].ToString()),
                            ImageSer: int.Parse(reader["ImageSer"].ToString())
                        );
                    }
                }

                connection.Close();
            }

            return res;
        }



        public AboutPlanSetup LoadPlanSetupInfo(int CourseSer, string PlanSetupId)
        {
            AboutPlanSetup res = new AboutPlanSetup(
                        Id: PlanSetupId,
                        CourseSer: CourseSer,
                        PlanSetupSer: 0,
                        StructureSetSer: 0
                    );

            if (is_aria_connected == false) return res;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "select top 100 p.PlanSetupSer, p.PlanSetupId, p.StructureSetSer, p.CourseSer, p.ImageSer from VARIAN.dbo.PlanSetup p where CourseSer = @CourseSer and PlanSetupId = @PlanSetupId";

                var command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@CourseSer", CourseSer);
                command.Parameters.AddWithValue("@PlanSetupId", PlanSetupId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res = new AboutPlanSetup(
                            Id: PlanSetupId,
                            CourseSer: int.Parse(reader["CourseSer"].ToString()),
                            PlanSetupSer: int.Parse(reader["PlanSetupSer"].ToString()),
                            StructureSetSer: int.Parse(reader["StructureSetSer"].ToString())
                        );
                    }
                }

                connection.Close();

            }
            return res;
        }



    }
}
