using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebApi.Core.Exceptions;

namespace WebApi.Infrastructure.Filters {
	public class HttpGlobalExceptionFilter : IExceptionFilter {
		private readonly ILogger<HttpGlobalExceptionFilter> logger;

		public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger) {
			this.logger = logger;
		}


		public void OnException(ExceptionContext context) {
			logger.LogError(new EventId(context.Exception.HResult),
				context.Exception,
				context.Exception.Message);

			if(context.HttpContext is null) {
				return;
			}
			context.ExceptionHandled = true;

			var httpContext = context.HttpContext;
			var exception = context.Exception;
			var errorFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();

			var problemDetails = new ProblemDetails {
				Type = $"{exception.GetType().Name}",
				Instance = errorFeature switch {
					ExceptionHandlerFeature e => e.Path,
					_ => "unknown"
				},
				Extensions = {
					["trace"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
				}
			};

			switch (exception) {
				case InputValidationException specialException:
					problemDetails.Status = StatusCodes.Status403Forbidden;
					problemDetails.Title = "One or more validation errors occurred";
					problemDetails.Detail = "The request contains invalid parameters. " +
											"More information can be found in the errors.";
					problemDetails.Extensions["errors"] = specialException.Errors;
					break;
				case DomainLayerExceptionNotFound specialException:
					problemDetails.Status = StatusCodes.Status404NotFound;
					problemDetails.Title = "The requested object was not found";
					problemDetails.Detail = "The request contains an object that could not be found. " +
											"More information can be found in the errors.";
					problemDetails.Extensions["errors"] = specialException.Errors;
					break;
				case DomainLayerException specialException:
					problemDetails.Status = StatusCodes.Status400BadRequest;
					problemDetails.Title = "A domain level error occurred";
					problemDetails.Detail = "More information can be found in the errors.";
					problemDetails.Extensions["errors"] = specialException.Errors;
					break;
				default:
					context.ExceptionHandled = false;
					problemDetails.Status = StatusCodes.Status500InternalServerError;
					problemDetails.Title = "An unexpected error occurred!";
					problemDetails.Detail = "Something went wrong";
					break;
			}


			context.Result = new JsonResult(problemDetails) { 
				ContentType = "application/problem+json",
				StatusCode = problemDetails.Status
			};
		}
	}
}
