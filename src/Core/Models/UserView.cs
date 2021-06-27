using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Models {
	public class UserView : User {
		public int SubscribersCount { get; set; }
	}
}
