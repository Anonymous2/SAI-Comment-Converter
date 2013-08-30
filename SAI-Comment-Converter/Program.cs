using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SAI_Comment_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("SQL Information:");
                //Console.Write("Host: ");
                //string host = Console.ReadLine();
                //Console.Write("User: ");
                //string user = Console.ReadLine();
                //Console.Write("Pass: ");
                //string pass = Console.ReadLine();
                //Console.Write("World DB: ");
                //string worldDB = Console.ReadLine();
                //Console.Write("Port: ");
                //string port = Console.ReadLine();

                string host = "127.0.0.1";
                string user = "root";
                string pass = "1234";
                string worldDB = "trinitycore_world";
                string port = "3306";

                //Console.WriteLine(host);
                //Console.WriteLine(user);
                //Console.WriteLine(pass);
                //Console.WriteLine(worldDB);
                //Console.WriteLine(port);

                MySqlConnectionStringBuilder connectionString = new MySqlConnectionStringBuilder();
                connectionString.UserID = user;
                connectionString.Password = pass;
                connectionString.Server = host;
                connectionString.Database = worldDB;
                connectionString.Port = Convert.ToUInt32(port);

                using (var connection = new MySqlConnection(connectionString.ToString()))
                {
                    connection.Open();
                    var returnVal = new MySqlDataAdapter(String.Format("SELECT * FROM smart_scripts ORDER BY entryorguid"), connection);
                    var dataTable = new DataTable();
                    returnVal.Fill(dataTable);

                    if (dataTable.Rows.Count <= 0)
                        break;

                    using (var outputFile = new StreamWriter("sai_update.sql", true))
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            MySqlCommand command = new MySqlCommand();
                            MySqlDataReader reader = null;
                            command.Connection = connection;

                            string fullLine = "UPDATE `smart_scripts` SET `comment`='";
                            int entryorguid = Convert.ToInt32(row.ItemArray[0].ToString());
                            int entry = entryorguid;

                            if (Convert.ToInt32(row.ItemArray[0].ToString()) < 0)
                            {
                                command.CommandText = (String.Format("SELECT id FROM creature WHERE guid={0}", -entryorguid));
                                reader = command.ExecuteReader(CommandBehavior.SingleResult);

                                if (reader.Read())
                                    entry = Convert.ToInt32(reader[0].ToString());
                            }

                            reader.Close();
                            command.CommandText = (String.Format("SELECT name FROM creature_template WHERE entry={0}", entry));
                            reader = command.ExecuteReader(CommandBehavior.SingleResult);

                            if (reader.Read())
                                fullLine = reader[0].ToString() + " - ";

                            Console.WriteLine(fullLine);
                            Console.ReadKey();

                            //! event_type
                            switch (Convert.ToInt32(row.ItemArray[4].ToString()))
                            {
                                
                            }
                        }
                    }
                }
            }
        }
    }
}
