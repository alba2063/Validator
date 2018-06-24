using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
	class DB_Adapter
	{
		private static readonly string connetionString = Reader.conString;

		private static SQLiteConnection sqlcon;
		//private static readonly string connetionString = "Data Source=validatordb.sqlite;Version=3;";

		private static void SetConnection()
		{
			sqlcon = new SQLiteConnection("Data Source=validatordb.sqlite;Version=3;New=False;Compress=True;");
		}

		public DataTable ReadLocation(int index)
		{
			DataTable dt = new DataTable();
			string sql;

			switch (index)
			{
				case 0:
					sql = "SELECT * FROM Gocator1";
					break;
				case 1:
					sql = "SELECT * FROM Gocator2";
					break;
				case 2:
					sql = "SELECT * FROM Gocator3";
					break;
				case 3:
					sql = "SELECT * FROM Gocator20x0";
					break;
				default:
					sql = string.Empty;
					break;
			}

			using (var sqlConn = new SQLiteConnection(connetionString))
			{
				var cmd = new SQLiteCommand(sql, sqlConn);

				sqlConn.Open();
				var reader = cmd.ExecuteReader();

				dt.Load(reader);

				sqlConn.Close();
			}

			return dt;
		}
	}
}
