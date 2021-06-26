using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Exceptions {
	public class DomainLayerException : Exception {
		public readonly ImmutableDictionary<string, string[]> Errors;

		public DomainLayerException(ImmutableDictionary<string, string[]> errors) {
			Errors = errors;
		}

		public DomainLayerException(IDictionary<string, string[]> errors) {
			Errors = errors.ToImmutableDictionary();
		}

		public DomainLayerException(string field, string message) {
			string fieldName = string.IsNullOrWhiteSpace(field) ? "undefinedField" : field;
			Errors = new Dictionary<string, string[]>() { [fieldName] = new[] { message } }.ToImmutableDictionary();
		}
	}

	public class DomainLayerExceptionNotFound : DomainLayerException {
		public DomainLayerExceptionNotFound(ImmutableDictionary<string, string[]> errors) 
			: base(errors) { }

		public DomainLayerExceptionNotFound(IDictionary<string, string[]> errors) 
			: base(errors) { }

		public DomainLayerExceptionNotFound(string field, string message)
			: base(field, message) { }
	}
}
