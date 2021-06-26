using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Models {
	public class UserWithSubscribers : User {
		public int SubscribersCount { get; set; }
	}
}
