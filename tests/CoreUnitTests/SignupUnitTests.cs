using System;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using WebApi.Core.Commands;
using Moq;
using WebApi.Core.Interfaces;
using WebApi.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using FluentValidation.TestHelper;
using WebApi.Core.Exceptions;
using System.Linq;

namespace UnitTests {
	public class SignupUnitTests {
		private readonly SignupCommandValidation validator;

		public SignupUnitTests() {
			validator = new SignupCommandValidation();
		}

		[Fact]
		public async Task Can_create_user() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var signUpCommand = fixture.Create<SignupCommand>();
			var repositoryMock = fixture.Freeze<Mock<IUsersRepository>>();
			repositoryMock.Setup(r => r.CreateUser(It.IsAny<User>())).Returns(Task.FromResult<long?>(1));
			var handler = fixture.Create<SignupCommandHandler>();

			// Act
			await handler.Handle(signUpCommand, new CancellationToken());

			// Assert
			repositoryMock.Verify(r => r.CreateUser(It.IsAny<User>()), Times.Once);
		}

		[Theory]
		[InlineData("SimpleName")]
		[InlineData("Name and space")]
		[InlineData("Имя на кириллице")]
		[InlineData("A name consisting of a large number of characterssssssssssssssss")]
		public void Should_not_have_error_when_valid_name_is_specified(string validUserName) {
			var model = new SignupCommand(validUserName);

			var result = validator.TestValidate(model);

			result.ShouldNotHaveAnyValidationErrors();
		}

		[Theory]
		[InlineData("")]
		[InlineData("A name with special char %")]
		[InlineData("A name with digit 123")]
		[InlineData(" a name with space in start")]
		[InlineData("a name with space in end ")]
		[InlineData("A wery long name abcdefg abcdefg abcdefg abcdefg abcdefg abcdefg abcdefg abcdefg abcdefg")]
		public void Should_have_error_when_invalid_name_is_specified(string invalidUserName) {
			var model = new SignupCommand(invalidUserName);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();

		}

		[Fact]
		public void Should_have_error_when_Name_is_null() {
			var model = new SignupCommand(null);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();
		}

		[Fact]
		public async Task Throw_domain_exception_when_user_singup_with_the_same_name_as_another_user() {
			var fixture = new Fixture().Customize(new AutoMoqCustomization());

			var signUpCommand = fixture.Create<SignupCommand>();
			var repositoryMock = fixture.Freeze<Mock<IUsersRepository>>();
			repositoryMock.Setup(r => r.CreateUser(It.IsAny<User>())).Returns(Task.FromResult<long?>(null));
			var handler = fixture.Create<SignupCommandHandler>();

			Func<Task> act = () => handler.Handle(signUpCommand, new CancellationToken());

			// Assert
			DomainLayerException exception = await Assert.ThrowsAsync<DomainLayerException>(act);

			Assert.Equal("A user with this name already exists, select a different user name", 
				exception.Errors.Values.First().First());
		}
	}
}
