using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Interfaces;
using WebApi.Core.Models;

namespace WebApi.Core.Commands {
	public record GetTopUsersCommand(long limit) : IRequest<UserView[]>;

	public class GetTopUsersCommandHandler : IRequestHandler<GetTopUsersCommand, UserView[]> {

		private readonly IUsersRepository usersRepository;

		public GetTopUsersCommandHandler(IUsersRepository usersRepository)
			=> this.usersRepository = usersRepository
				?? throw new ArgumentNullException(nameof(usersRepository));


		public async Task<UserView[]> Handle(GetTopUsersCommand request, CancellationToken cancellationToken) {
			var usersWithSubscriptions = await usersRepository.GetTopUsers(request.limit);
			return usersWithSubscriptions;
		}
	}

	public class GetTopUsersCommandValidation : AbstractValidator<GetTopUsersCommand> {
		public GetTopUsersCommandValidation() {
			RuleFor(x => x.limit)
				.GreaterThan(0);
		}
	}
}
