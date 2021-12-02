using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableTest {
    class DatabaseConfig{
        public string Server, Port, Databasename, Username, Password;

        public DatabaseConfig(string server, string port, string databasename, string username, string password) {
            Server = server;
            Port = port;
            Databasename = databasename;
            Username = username;
            Password = password;
        }
    }

    class Pegawai {
        public string Nama, NIP, Tanggal, NoMobil;
        public string TransaksiId;
        public long Penghasilan, Komisi;

        public Pegawai(string  nip, string nama, string tanggal, string noMobil, string transaksiId, long penghasilan, long komisi) {
            NIP = nip;
            Nama = nama;
            Tanggal = tanggal;
            NoMobil = noMobil;
            TransaksiId = transaksiId;
            Penghasilan = penghasilan;
            Komisi = komisi;
        }
    }


    internal class Database {

        private static MySqlConnection connection = null;

        private static DatabaseConfig databaseConfig;

        private static string myConfig(DatabaseConfig conn) {
            return $"server={conn.Server}; port={conn.Port}; database={conn.Databasename}; uid={conn.Username}; pwd={conn.Password}";
        }

        internal static int Connect(DatabaseConfig dbConfig, ref string refMessage) {
            int retVal = -1;

            try {
                databaseConfig = dbConfig;
                connection = new MySqlConnection(myConfig(databaseConfig));
                connection.Open();
                refMessage = "OK";
                retVal = 0;
            } catch (Exception err) {
                refMessage = $"Failed | Error: {err.Message}";
            } finally {
                if (connection.State == ConnectionState.Open) connection.Close();
            }

            return retVal;

        }

        internal static int Create(string dbTable, Pegawai pegawai, ref string refMessage) {
            int retVal = -1;

            string query = $"INSERT INTO {dbTable} (trx_id, nip, nama, tanggal, no_mobil, penghasilan, komisi) " +
                $"VALUES ('{pegawai.TransaksiId}','{pegawai.NIP}','{pegawai.Nama}','{pegawai.Tanggal}','{pegawai.NoMobil}',{pegawai.Penghasilan},{pegawai.Komisi});";

            try {

                connection = new MySqlConnection(myConfig(databaseConfig));
                connection.Open();

                MySqlCommand mySqlCommand = new MySqlCommand(query, connection);
                MySqlDataReader mySqlDataReader;
                mySqlDataReader = mySqlCommand.ExecuteReader();

                refMessage = "OK";
                retVal = 0;

                mySqlCommand.Cancel();
                mySqlDataReader.Close();
            } catch (Exception err) {
                refMessage = $"Failed | Error: {err.Message}";
            } finally {
                if (connection.State == ConnectionState.Open) connection.Close();
            }


            return retVal;
        }

        internal static int Read(string dbTable, ref DataTable dataTable, ref string refMessage) {
            int retVal = -1;

            string query = $"SELECT * FROM {dbTable};";

            try {

                connection = new MySqlConnection(myConfig(databaseConfig));
                connection.Open();

                MySqlCommand mySqlCommand = new MySqlCommand(query, connection);

                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
                mySqlDataAdapter.Fill(dataTable);
                refMessage = "OK";
                retVal = 0;


                mySqlCommand.Cancel();
            } catch (Exception err) {
                refMessage = $"Failed | Error: {err.Message}";
            } finally {
                if (connection.State == ConnectionState.Open) connection.Close();
            }


            return retVal;
        }

    }
}
