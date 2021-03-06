using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;

namespace CourseLibrary.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpCacheHeaders((expirationModelOptions) =>
            {
                expirationModelOptions.MaxAge = 60;
                expirationModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            },
            (validationModelOptions)=>
            {
                validationModelOptions.MustRevalidate = true;
            });
            services.AddResponseCaching();
            
            var (dbSettings, cert) = DatabaseSetting.FromConfig(Configuration);
            var store = new DocumentStore
            {
                Urls = dbSettings.Urls,
                Database = dbSettings.DatabaseName,
                Certificate = cert
            }.Initialize();

            IndexCreation.CreateIndexes(typeof(Startup).Assembly, store);
            services.AddSingleton(store);

            // services.AddRavenDbSingleInstanceMigrations(typeof(Startup).Assembly);

            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.CacheProfiles.Add("240SecondsCacheProfile",
                    new CacheProfile()
                    {
                        Duration = 240
                    });
            })
              .AddNewtonsoftJson(setupAction =>
            {
                setupAction.SerializerSettings.ContractResolver =
                  new CamelCasePropertyNamesContractResolver();
            }).AddXmlDataContractSerializerFormatters()
              .ConfigureApiBehaviorOptions(setupAction =>
              {
                  setupAction.InvalidModelStateResponseFactory = context =>
                   {
                       // Create a problem details object
                       var problemDetailsFactory = context.HttpContext.RequestServices
                       .GetRequiredService<ProblemDetailsFactory>();
                       var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                                            context.HttpContext,
                                            context.ModelState);
                       // Add additional info not added by default
                       problemDetails.Detail = "See the errors field for details.";
                       problemDetails.Instance = context.HttpContext.Request.Path;

                       //find out which status code to use
                       var actionExecutingContext =
                            context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                       // If there are modelstate errors & all arguments were correctly found
                       // or parsed, we're dealing with validation errors.
                       if(
                       (context.ModelState.ErrorCount > 0) &&
                       (actionExecutingContext?.ActionArguments.Count ==
                       context.ActionDescriptor.Parameters.Count))
                       {
                           problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                           problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                           problemDetails.Title = "One or more validation errors occurred.";

                           return new UnprocessableEntityObjectResult(problemDetails)
                           {
                               ContentTypes = { " application/problem+json" }
                           };
                       };

                       //if one of the arguments wasn't correctly found / couldn' be parsed
                       // we're dealing with null/unparseable input.
                       problemDetails.Status = StatusCodes.Status400BadRequest;
                       problemDetails.Title = "One or more errors on input occurred.";
                       return new BadRequestObjectResult(problemDetails)
                       {
                           ContentTypes = { "application/problem+json" }
                       };
                   };
              });

            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                        .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if(newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.marvin.hateoas+json");
                }
            });
            // Register PropertyMappingService
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            // Register PropertyCheckerService
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<ICourseLibraryRepository, RavenDbCoursesRepository >(); // CourseLibraryRepository

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=CourseLibraryDB;Trusted_Connection=True;");
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Please try again later.");
                    });
                });
            }

            // app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
