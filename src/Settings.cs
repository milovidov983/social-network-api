using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi {
	public class Settings {
		public UserNameRuleSettings UserNameRule { get; set; }

		public class UserNameRuleSettings {
			public int MaxLength { get; set; }
			public string ValidCharsRegexp { get; set; }
		}
	}
}
