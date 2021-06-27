using System;
using WebApi.Core.Commands;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using WebApi.Core.Interfaces;
using WebApi.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using FluentValidation.TestHelper;
using WebApi.Core.Exceptions;
using System.Linq;

namespace UnitTests {
	public class MakeSubscriptionUnitTests {
		private readonly MakeSubscriptionValidation validator;

		public MakeSubscriptionUnitTests() {
			validator = new MakeSubscriptionValidation();
		}

		[Fact]
		public void Should_not_have_error_when_id_valid_and_differet() {
			var model = new MakeSubscriptionCommand(1,2);

			var result = validator.TestValidate(model);

			result.ShouldNotHaveAnyValidationErrors();
		}


		[Fact]
		public void Should_have_error_when_id_is_negative() {
			var model = new MakeSubscriptionCommand(-1,1);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();
		}

		[Fact]
		public void Should_have_error_when_ids_are_the_same() {
			var model = new MakeSubscriptionCommand(1,1);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();
		}



		[Fact]
		public async Task Throw_notfound_exception_when_user_not_exists() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var command = fixture.Create<MakeSubscriptionCommand>();
			var repositoryMock = fixture.Freeze<Mock<IUsersRepository>>();
			repositoryMock.Setup(r => r.IsUserExists(It.IsAny<long>())).Returns(Task.FromResult(false));
			var handler = fixture.Create<MakeSubscriptionCommandHandler>();

			Func<Task> act = () => handler.Handle(command, new CancellationToken());

			// Assert
			DomainLayerExceptionNotFound exception = await Assert.ThrowsAsync<DomainLayerExceptionNotFound>(act);

			Assert.Equal("The user who is trying to subscribe was not found",
				exception.Errors.Values.First().First());
		}



		[Fact]
		public async Task Throw_domain_exception_when_subscription_alredy_exists() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var command = fixture.Create<MakeSubscriptionCommand>();
			var repositoryMock = fixture.Freeze<Mock<IUsersRepository>>();
			repositoryMock.Setup(r => r.IsUserExists(It.IsAny<long>())).Returns(Task.FromResult(true));
			repositoryMock.Setup(r => r.MakeSubscription(It.IsAny<Subscription>())).Returns(Task.FromResult(false));
			var handler = fixture.Create<MakeSubscriptionCommandHandler>();

			Func<Task> act = () => handler.Handle(command, new CancellationToken());

			// Assert
			DomainLayerException exception = await Assert.ThrowsAsync<DomainLayerException>(act);

			Assert.Equal("Error when trying to create a subscription. " +
					"It may already exist, or one of the users has been deleted.",
				exception.Errors.Values.First().First());
		}

	}
}
