using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Database
    {
        private static readonly string connectionString= "Server=127.0.0.1;Database=pr4;User ID=root;Password=;";
        public static List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, login, password, src FROM users";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User(
                            reader.GetInt32("id"),
                            reader.GetString("login"),
                            reader.GetString("password"),
                            reader.GetString("src")
                        );
                        users.Add(user);
                    }
                }
            }
            return users;
        }
        public static void AddUserCommand(int userId, string commandText)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO user_commands (userId, commandText) VALUES (@userId, @commandText)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@commandText", commandText);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
