using System.Data.SqlClient;

namespace UMRO.DvhAnalysis.AriaDb.Sql
{
    public class OncologistRepository : IOncologistRepository
    {
        private readonly string _connectionString;

        public OncologistRepository(string connectionString)
        {
            _connectionString = connectionString;

            if (string.IsNullOrEmpty(connectionString?.Trim()))
            {
                is_aria_connected = false;
            }
        }

        private bool is_aria_connected = true;


        public Oncologist FindById(string oncologistId)
        {
            Oncologist oncologist = new Oncologist(oncologistId, oncologistId);

            if (is_aria_connected == false) return oncologist;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "select DoctorId, FirstName, LastName from Doctor where DoctorId = @OncologistId";
                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@OncologistId", oncologistId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var id = reader["DoctorId"].ToString();
                        var firstName = reader["FirstName"].ToString();
                        var lastName = reader["LastName"].ToString();
                        var fullName = $"{firstName} {lastName}";
                        oncologist = new Oncologist(id, fullName);
                    }
                }

                connection.Close();
            }

            return oncologist;
        }

    }
}
