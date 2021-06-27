using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WebApi.Core.Exceptions {
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
