using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Exceptions;

namespace WebApi.Infrastructure.Behaviours {
	public class ValidatorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> {
		private readonly IEnumerable<IValidator<TRequest>> validators;

		public ValidatorPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
			=> this.validators = validators;
		
		public Task<TResponse> Handle(
			TRequest request, 
			CancellationToken cancellationToken, 
			RequestHandlerDelegate<TResponse> next) {

			var failuers = validators
				.Select(validator => validator.Validate(request))
				.SelectMany(result => result.Errors)
				.ToArray();

			if (failuers.Any()) {
				var errors = failuers
					.GroupBy(x => x.PropertyName)
					.ToImmutableDictionary(
						k => k.Key,
						v => v.Select(x => x.ErrorMessage).ToArray()
					);

				throw new InputValidationException(errors);
			}

			return next();
		}
	}
}
