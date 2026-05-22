#nullable disable
using MySql.Data.MySqlClient; // Required for MySQL types

namespace MyInformationSystem
{
    public static class DatabaseConnection
    {
        // Standard XAMPP connection string
        private static string connStr = "server=localhost;user=root;password=;database=librarysystem;";

        public static MySqlConnection GetConnection() 
        {
            return new MySqlConnection(connStr);
        }

        public static bool TestConnection(out string error)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                error = "";
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message; // Captures "Target machine actively refused it" if XAMPP is off
                return false;
            }
        }
    }
}