using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System;

namespace MamitaHealthyFood
{
    public class Database
    {
        private string connectionString = "Server=localhost;Database=mamita_db;uid=root;Password=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}