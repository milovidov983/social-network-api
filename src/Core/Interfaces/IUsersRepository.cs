using System.Threading.Tasks;
using WebApi.Core.Models;

namespace WebApi.Core.Interfaces {
	public interface IUsersRepository {
		Task<long?> CreateUser(User user);
		Task<bool> MakeSubscription(Subscription subscription);
		Task<UserWithSubscribers[]> GetTopUsers(long limit);
		Task<bool> IsUserExsists(long userId);
	}
}
