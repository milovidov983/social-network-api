using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Exceptions;
using WebApi.Core.Models;
using WebApi.Core.Interfaces;

namespace WebApi.Core.Commands {
	public record GetTopUsersCommand(long limit) : IRequest<UserWithSubscribers[]>;

	public class GetTopUsersCommandHandler : IRequestHandler<GetTopUsersCommand, UserWithSubscribers[]> {

		private readonly IUsersRepository usersRepository;

		public GetTopUsersCommandHandler(IUsersRepository usersRepository)
			=> this.usersRepository = usersRepository
				?? throw new ArgumentNullException(nameof(usersRepository));


		public async Task<UserWithSubscribers[]> Handle(GetTopUsersCommand request, CancellationToken cancellationToken) {
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
