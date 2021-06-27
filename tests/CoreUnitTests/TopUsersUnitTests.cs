using FluentValidation.TestHelper;
using WebApi.Core.Commands;
using Xunit;

namespace UnitTests {
	public class TopUsersUnitTests {
		private readonly GetTopUsersCommandValidation validator;

		public TopUsersUnitTests() {
			validator = new GetTopUsersCommandValidation();
		}

		[Fact]
		public void Should_not_have_error_when_limit_is_positive() {
			var model = new GetTopUsersCommand(1);

			var result = validator.TestValidate(model);

			result.ShouldNotHaveAnyValidationErrors();
		}


		[Fact]
		public void Should_have_error_when_limit_is_zero() {
			var model = new GetTopUsersCommand(0);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();
		}

		[Fact]
		public void Should_have_error_when_limit_is_negative() {
			var model = new GetTopUsersCommand(-1);

			var result = validator.TestValidate(model);

			result.ShouldHaveAnyValidationError();
		}
	}
}
