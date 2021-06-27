using System.Threading.Tasks;
using WebApi.Core.Models;

namespace WebApi.Core.Interfaces {
	public interface IUsersRepository {
		Task<long?> CreateUser(User user);
		Task<bool> MakeSubscription(Subscription subscription);
		Task<UserView[]> GetTopUsers(long limit);
		Task<bool> IsUserExists(long userId);
	}
}
