using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Commands;
using WebApi.Core.Interfaces;
using WebApi.Infrastructure.SQLite.Base;
using Xunit;

namespace IntegrationTests {
	public class TopUsersIntegrationTests {
		private string path = $"{Directory.GetCurrentDirectory()}\\data\\top_users_test.db";

		[Fact]
		public async Task Return_user_with_maximum_number_of_subscribers() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var command = fixture.Build<GetTopUsersCommand>().With(f=>f.limit, 1).Create();

			var databaseFactory = fixture.Freeze<Mock<IDatabaseFactory>>();
			databaseFactory.Setup(f => f.Create()).Returns(new Database(path));
			IUsersRepository repository = new UsersRepository(databaseFactory.Object);
			var handler = new GetTopUsersCommandHandler(repository);


			var result = await handler.Handle(command, new CancellationToken());

			Assert.Equal("TestUser", result.First().Name);
		}

		[Fact]
		public async Task Return_all_users_with_a_large_limit() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var command = fixture.Build<GetTopUsersCommand>().With(f => f.limit, 999).Create();

			var databaseFactory = fixture.Freeze<Mock<IDatabaseFactory>>();
			databaseFactory.Setup(f => f.Create()).Returns(new Database(path));
			IUsersRepository repository = new UsersRepository(databaseFactory.Object);
			var handler = new GetTopUsersCommandHandler(repository);


			var result = await handler.Handle(command, new CancellationToken());

			Assert.Equal(5, result.Length);
		}
	}
}
