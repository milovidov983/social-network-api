using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Models {
	/// <summary>
	/// User subscriptions
	/// </summary>
	public class Subscription {
		/// <summary>
		/// User ID
		/// </summary>
		public long UserId { get; set; }
		/// <summary>
		/// Subscribed to
		/// </summary>
		public long SubscribedToUserId { get; set; }

	}
}
