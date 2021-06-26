using Dapper;
using DapperHelpers;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Core.Models;
using WebApi.Core.Interfaces;
using WebApi.Infrastructure.SQLite.Tables;

namespace WebApi.Infrastructure.SQLite.Base {
	public class UsersRepository : IUsersRepository {
		public async Task<long?> CreateUser(User user) {
			var userIdOrNull = await ExecuteCommitedDbQuery(async db => {
				var sql = db.UsersTable
					.Exclude(f => f.Id)
					.Query(x => $@"
					INSERT OR IGNORE INTO {x.Name}
					({x.SelectInsert()})
					VALUES
					({x.Insert()})
					RETURNING {x.FieldShort(f => f.Id)}
				");

				return await db.ActiveConnection.ExecuteScalarAsync<long?>(sql, user);
			});

			return userIdOrNull;
		}

		public async Task<bool> MakeSubscription(Subscription subscription) {
			var rowsAffected = await ExecuteCommitedDbQuery<int>(async db => {
				var sql = db
					.Query(x => $@"
					INSERT INTO {x.SubscriptionsTable.Name}
					({x.SubscriptionsTable.SelectInsert()})
					SELECT
					{x.SubscriptionsTable.Insert()}
					WHERE NOT EXISTS (
						SELECT 1 
						FROM {x.SubscriptionsTable.Name} 
						WHERE {x.SubscriptionsTable.FieldShort(f => f.UserId)} = @{nameof(Subscriptions.UserId)} 
							AND 
							  {x.SubscriptionsTable.FieldShort(f => f.SubscribedToUserId)} = @{nameof(Subscriptions.SubscribedToUserId)} 
						) 
						AND EXISTS (
							SELECT 1
							FROM {x.UsersTable.Name}
							WHERE {x.UsersTable.FieldShort(f=>f.Id)} = @{nameof(Subscriptions.UserId)}
						) 
						AND EXISTS (
							SELECT 1
							FROM {x.UsersTable.Name}
							WHERE {x.UsersTable.FieldShort(f => f.Id)} = @{nameof(Subscriptions.SubscribedToUserId)}
						)
					
				");

				return await db.ActiveConnection.ExecuteAsync(sql, subscription);
			});

			return rowsAffected > 0;
		}

		public async Task<UserWithSubscribers[]> GetTopUsers(long limit) {
			var db = new Database();
			db.ActiveConnection.Open();

			var sql = db.Query(x => $@"
				SELECT {x.UsersTable.Select()}, 
						{nameof(UserWithSubscribers.SubscribersCount)}
				FROM (SELECT {x.SubscriptionsTable.FieldShort(f => f.SubscribedToUserId)}, 
							COUNT({x.SubscriptionsTable.FieldShort(f => f.UserId)}) as {nameof(UserWithSubscribers.SubscribersCount)}
						FROM {x.SubscriptionsTable.Name}
						GROUP BY {x.SubscriptionsTable.FieldShort(f => f.SubscribedToUserId)}
						UNION 
						SELECT {x.UsersTable.FieldShort(f => f.Id)}, 0 as {nameof(UserWithSubscribers.SubscribersCount)}
						FROM {x.UsersTable.Name}
						WHERE NOT EXISTS(
								SELECT 1
								FROM {x.SubscriptionsTable.Name}
								WHERE {x.UsersTable.FieldShort(f => f.Id)} = {x.SubscriptionsTable.FieldShort(f=>f.SubscribedToUserId)}
							)
						LIMIT @{nameof(limit)}
					)
				JOIN {x.UsersTable.Name}
				ON {x.UsersTable.FieldShort(f => f.Id)} = {x.SubscriptionsTable.FieldShort(f => f.SubscribedToUserId)}
				ORDER BY {nameof(UserWithSubscribers.SubscribersCount)} DESC, {x.UsersTable.FieldShort(f=>f.Name)}
				");

			var result = await db.ActiveConnection.QueryAsync<UserWithSubscribers>(sql, new { limit });


			return result.ToArray();
		}

		public async Task<bool> IsUserExsists(long userId) {
			var db = new Database();
			db.ActiveConnection.Open();

			var sql = db.UsersTable.Query(x => $@"
				SELECT {x.FieldShort(f => f.Id)}
				FROM {x.Name}
				WHERE {x.FieldShort(f => f.Id)} = @{nameof(userId)}
			");

			var userIdOrEmpty= await db.ActiveConnection.QueryAsync<long>(sql, new { userId });

			return userIdOrEmpty.Any();

		}


		private static async Task<TResult> ExecuteCommitedDbQuery<TResult>(Func<Database, Task<TResult>> func) {
			IDbTransaction tx = null;
			try {
				var db = new Database();
				db.ActiveConnection.Open();
				tx = db.ActiveConnection.BeginTransaction();

				var result = await func(db);

				tx.Commit();

				return result;
			} finally {
				tx?.Dispose();
			}
		}
	}
}
