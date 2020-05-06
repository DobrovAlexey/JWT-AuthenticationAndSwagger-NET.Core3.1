using System;
using System.Linq;
using System.Text;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Options;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1
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
            services.AddControllers();

            #region Options

            // Register the ConfigurationBuilder instance of JwtOption
            var jwtOption = Configuration.GetSection(nameof(JwtOption));
            services.Configure<JwtOption>(jwtOption);

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOption[nameof(JwtOption.SecretKey)]));

            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(jwtAppSettingOptions);
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.ValidFor = TimeSpan.FromMinutes(int.Parse(jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidFor)]));
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            #endregion


            #region JWT

            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddSingleton<IJwtTokenHandler, JwtTokenHandler>();
            services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
            services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();

            // the AddJwtBearer middleware receives the JwtBearerOptions object from the IOptions during execution
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            // The Class which holds the JwtBearerOptions will be returned whenever required
            services.ConfigureOptions<ConfigureJwtBearerOptions>();
            #endregion

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            #region Add OpenAPI/Swagger document

            // registers a OpenAPI v3.0 document with the name "v1" (default)
            // https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware#enable-jwt-authentication
            services.AddOpenApiDocument(configure =>
            {
                configure.Version = "v1";
                configure.Title = "Volunteers WebAPI (OpenApi)";
                configure.Description = "ASP.NET Core Web API for Volunteers";
                configure.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.Null;

                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            #region Add OpenAPI/Swagger document

            // Serves the registered OpenAPI/Swagger documents
            app.UseOpenApi();
            // Serves the Swagger UI 3 web ui to view the OpenAPI/Swagger documents
            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/api";
                settings.DocumentPath = "/swagger/v1/swagger.json";
            });
            // serve ReDoc UI
            app.UseReDoc(options => options.Path = "/redoc");

            #endregion

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}