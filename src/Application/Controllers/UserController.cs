using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApi.Application.Models;
using WebApi.Core.Commands;

namespace WebApi.Application.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase {
		private readonly IMediator mediator;

		public UserController(IMediator mediator)
			=> this.mediator = mediator;

		[HttpPost, Route("Signup")]
		public async Task<IActionResult> Signup([FromBody] SignupRequest request) {
			var id = await mediator.Send(new SignupCommand(request.UserName));
			return Ok(id);
		}

		[HttpPost, Route("MakeSubscription")]
		public async Task<IActionResult> MakeSubscription([FromBody] NewSubscriptionRequest request) {
			var result = await mediator.Send(new MakeSubscriptionCommand(request.UserId, request.SubscribeToUser));
			return Ok(result);
		}

		[HttpGet, Route("TopUsers")]
		public async Task<IActionResult> GetTopUsers([FromQuery] TopUsersRequest request) {
			var usersWithSubscriptions = await mediator.Send(new GetTopUsersCommand(request.Limit));
			return Ok(usersWithSubscriptions);
		}
	}
}
