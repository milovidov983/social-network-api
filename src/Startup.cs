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
using WebApi.Infrastructure.Filters;
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
				.AddMvc(o => o.Filters.Add<HttpGlobalExceptionFilter>())
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

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
