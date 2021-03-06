using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperHelpers;
using DapperHelpers.Models;
using System.Data;
using WebApi.Infrastructure.SQLite.Tables;
using System.Data.SQLite;

namespace WebApi.Infrastructure.SQLite.Base {
	public class Database : IDisposable {
		public IDbConnection ActiveConnection { get; private set; }

		public readonly Table<Users> UsersTable = TableExtentions.Create<Users>(Users.TableName);
		public readonly Table<Subscriptions> SubscriptionsTable = TableExtentions.Create<Subscriptions>(Subscriptions.TableName);

		//private readonly string dbFile = "data\\sqlitedb.db";
		private readonly string databasePath;

		public Database(string databasePath) {
			this.databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

			ActiveConnection = GetConnection();
		}


		public IDbConnection GetConnection() {
			return new SQLiteConnection($"Data Source={databasePath}");
		}

		public void Dispose() {
			ActiveConnection?.Dispose();
		}
	}
}
