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
	public record SignupCommand(string UserName) : IRequest<long>;

	public class SignupCommandHandler : IRequestHandler<SignupCommand, long> {

		private readonly IUsersRepository usersRepository;

		public SignupCommandHandler(IUsersRepository usersRepository)
			=> this.usersRepository = usersRepository
				?? throw new ArgumentNullException(nameof(usersRepository));


		public async Task<long> Handle(SignupCommand request, CancellationToken cancellationToken) {
			var userId = await usersRepository.CreateUser(new User {
				Name = request.UserName
			});

			if(!userId.HasValue) {
				throw new DomainLayerException(
					nameof(request.UserName), 
					"A user with this name already exists, select a different user name");
			}

			return userId.Value;
		}
	}

	public class SignupCommandValidation : AbstractValidator<SignupCommand> {
		public SignupCommandValidation() {
			RuleFor(x => x.UserName)
				.NotEmpty()
				.MaximumLength(64)
				.Matches("^[\\w ]+$");
		}
	}
}
