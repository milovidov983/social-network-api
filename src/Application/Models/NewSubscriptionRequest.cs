using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Application.Models {
	public class NewSubscriptionRequest {
		public long UserId { get; set; }
		public long SubscribeToUser { get; set; }
	}
}
