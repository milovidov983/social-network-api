using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Exceptions;
using WebApi.Core.Interfaces;
using WebApi.Core.Models;

namespace WebApi.Core.Commands {
	public record MakeSubscriptionCommand(long UserId, long ToUserId) : IRequest<bool>;

	public class MakeSubscriptionCommandHandler : IRequestHandler<MakeSubscriptionCommand, bool> {

		private readonly IUsersRepository usersRepository;

		public MakeSubscriptionCommandHandler(IUsersRepository usersRepository)
			=> this.usersRepository = usersRepository
				?? throw new ArgumentNullException(nameof(usersRepository));


		public async Task<bool> Handle(MakeSubscriptionCommand request, CancellationToken cancellationToken) {
			var isTargetUserExistsTask = usersRepository.IsUserExists(request.UserId);
			var isToSubscribeUserExistsTask = usersRepository.IsUserExists(request.ToUserId);

			await Task.WhenAll(isTargetUserExistsTask, isToSubscribeUserExistsTask);

			bool isTargetUserExists = isTargetUserExistsTask.Result;
			bool isToSubscribeUserExists = isToSubscribeUserExistsTask.Result;


			if (!isTargetUserExists) {
				throw new DomainLayerExceptionNotFound(
					nameof(request.UserId), 
					"The user who is trying to subscribe was not found");
			}

			if (!isToSubscribeUserExists) {
				throw new DomainLayerExceptionNotFound(
					nameof(request.ToUserId),
					"The user you are trying to subscribe to was not found");
			}

			bool isSubscriptionSuccessfully = await usersRepository.MakeSubscription(
				new Subscription {
					UserId = request.UserId,
					SubscribedToUserId = request.ToUserId
				});

			if (!isSubscriptionSuccessfully) {
				throw new DomainLayerException(
					nameof(request),
					"Error when trying to create a subscription. " +
					"It may already exist, or one of the users has been deleted.");
			}

			return isSubscriptionSuccessfully;
		}
	}

	public class MakeSubscriptionValidation : AbstractValidator<MakeSubscriptionCommand> {
		public MakeSubscriptionValidation() {
			RuleFor(x => x.ToUserId)
				.GreaterThan(-1);

			RuleFor(x => x.UserId)
				.GreaterThan(-1);

			RuleFor(x => x)
				.Must((args, _) => args.UserId != args.ToUserId)
				.WithMessage("Forbidden to subscribe to yourself");
		}
	}
}
