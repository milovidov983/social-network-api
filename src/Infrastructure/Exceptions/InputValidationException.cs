using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Infrastructure.Exceptions {
	public class InputValidationException : Exception {
		public readonly ImmutableDictionary<string, string[]> Errors;

		public InputValidationException(ImmutableDictionary<string, string[]> errors) {
			Errors = errors;
		}

		public InputValidationException(IDictionary<string, string[]> errors) {
			Errors = errors.ToImmutableDictionary();
		}
	}
}
