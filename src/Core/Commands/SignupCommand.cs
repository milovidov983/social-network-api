using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Exceptions;
using WebApi.Core.Models;
using WebApi.Core.Interfaces;
using System.Text.RegularExpressions;

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
		public const int NAME_MAX_LEN = 64;

		public SignupCommandValidation() {
			RuleFor(x => x.UserName)
				.NotNull()
				.NotEmpty()
				.MaximumLength(NAME_MAX_LEN);

			RuleFor(x => x.UserName)
				.Must((_, name) =>  name != null
						&& !name.StartsWith(" ")
						&& !name.EndsWith(" ")
						&& !Regex.IsMatch(name, "\\d")
						&& Regex.IsMatch(name, "^[\\w ]+$")
				).WithMessage("Invalid characters in the user name"); ;
		}
	}
}
