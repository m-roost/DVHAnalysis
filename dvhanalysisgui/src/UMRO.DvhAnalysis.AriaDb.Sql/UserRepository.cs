using System.Data.SqlClient;

namespace UMRO.DvhAnalysis.AriaDb.Sql
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;

            if (string.IsNullOrEmpty(connectionString?.Trim()))
            {
                is_aria_connected = false;
            }
        }

        private bool is_aria_connected = true;

        public User FindById(string userId)
        {
            User user = new User(userId, userId);

            if (is_aria_connected == false) return user;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = "select Id, Name from Users where Id = @UserId";
                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var id = reader["Id"].ToString();
                        var name = reader["Name"].ToString();
                        user = new User(id, name);
                    }
                }

                connection.Close();
            }
             
            return user;
        }
    }
}
