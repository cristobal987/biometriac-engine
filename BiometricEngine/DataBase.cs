using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace BiometricEngine
{
    public class DataBase
    {
        private static string connectionString = "server=localhost;user=adminfaces;database=faces;port=3306;password=f4c3Eng1n3;SslMode=none";

        public int ExecuteNonQuery(string sql) {
            MySqlConnection conn = new MySqlConnection(connectionString);
            int result = -1;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally {
                conn.Dispose();
                GC.Collect();
            }
            return result;
        }

        public int ExecuteNonQuery(MySqlCommand cmd)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            int result = -1;
            try
            {
                conn.Open();
                cmd.Connection = conn;
                result = cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                conn.Dispose();
                GC.Collect();
            }
            return result;
        }

        public DataTable ExecuteQuery(string sql)
        {
            DataTable datatable = new DataTable();
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            try
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                adapter.SelectCommand.CommandType = CommandType.Text;
                adapter.Fill(datatable);
                adapter.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{oops - {0}", ex.Message);
            }
            finally
            {
                conn.Dispose();
                GC.Collect();
            }
            return datatable;
        }

        public string GenerateId() {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
