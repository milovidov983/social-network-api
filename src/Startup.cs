using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Text.Json;
using WebApi.Core.Exceptions;
using WebApi.Core.Interfaces;
using WebApi.Infrastructure.Behaviours;
using WebApi.Infrastructure.SQLite.Base;


namespace WebApi {
	public class Startup {
		public Startup(IWebHostEnvironment env) {
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		
		public void ConfigureServices(IServiceCollection services) {
			services.Configure<Settings>(Configuration);
			services.AddControllers();
			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });
			});

			services.AddMediatR(typeof(Startup));
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorPipelineBehavior<,>));
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<IUsersRepository, UsersRepository>();

			services
				.AddMvc()
				.AddFluentValidation(s => s.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.Configure<ApiBehaviorOptions>(options => {
                options.InvalidModelStateResponseFactory = context => {
                    var problemDetails = new ValidationProblemDetails(context.ModelState);
                    throw new InputValidationException(problemDetails.Errors);
                };
            });
		}

	
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));
			}

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseExceptionHandler(errorApp => {
				errorApp.Run(ExceptionHandler());
			});

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}

		private static RequestDelegate ExceptionHandler() {
			return async context => {
				var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
				var exception = errorFeature.Error;

				// https://tools.ietf.org/html/rfc7807#section-3.1
				var problemDetails = new ProblemDetails {
					Type = $"{exception.GetType().Name}",
					Title = "An unexpected error occurred!",
					Detail = "Something went wrong",
					Instance = errorFeature switch {
						ExceptionHandlerFeature e => e.Path,
						_ => "unknown"
					},
					Status = StatusCodes.Status400BadRequest,
					Extensions =
					{
							["trace"] = Activity.Current?.Id ?? context?.TraceIdentifier
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
				}

				context.Response.ContentType = "application/problem+json";
				context.Response.StatusCode = problemDetails.Status.Value;
				context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue() {
					NoCache = true,
				};
				await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails);
			};
		}
	}
}
