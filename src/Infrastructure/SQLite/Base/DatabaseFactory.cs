using Microsoft.Extensions.Options;

namespace WebApi.Infrastructure.SQLite.Base {
	public class DatabaseFactory : IDatabaseFactory {
		private readonly string databasePath;
		public DatabaseFactory(IOptions<Settings> options)
			=> databasePath = options.Value.DatabasePath;
		
		public Database Create() => new(databasePath);
	}
}
