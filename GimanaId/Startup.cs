using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.Generation.Processors.Security;
using GimanaIdApi.Common.Authentication;
using GimanaIdApi.Common.Config;
using GimanaIdApi.Common.Mapper;
using GimanaIdApi.Entities.Entities;
using GimanaIdApi.Infrastructure.AlphanumericTokenGenerator;
using GimanaIdApi.Infrastructure.DataAccess;
using GimanaIdApi.Infrastructure.EmailSender;
using GimanaIdApi.Infrastructure.PasswordHasher;
using GimanaIdApi.Infrastructure.SecurePasswordSaltGenerator;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using GimanaId.Infrastructure.Mocks.MockEmailSender;
using GimanaId.Infrastructure.Mocks.InMemoryDataAccess;
using Microsoft.Net.Http.Headers;
using GimanaId.Infrastructure.DateTimeService;

namespace GimanaIdApi
{
    public class Startup
    {
        private IWebHostEnvironment _env;

        public Startup(
            IWebHostEnvironment env)
        {
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var config =
                new ApiConfig()
                {
                    AuthTokenLength = 32,
                    PasswordResetTokenLength = 32,
                    EmailVerificationTokenLength = 32,
                    EmailVerificationTokenLifetime = new TimeSpan(30, 0, 0, 0),
                    PasswordResetTokenLifetime = new TimeSpan(2, 0, 0),
                    ApiBaseAddress = Environment.GetEnvironmentVariable("API_BASE_ADDRESS")
                };

            services.AddSingleton(typeof(ApiConfig), config);

            services.AddSwaggerDocument(
                (config) =>
                {
                    config.DocumentProcessors.Add(
                    new SecurityDefinitionAppender("AuthToken",
                    new NSwag.OpenApiSecurityScheme
                    {
                        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                        Name = "Auth-Token",
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    }));
                    config.OperationProcessors.Add(new OperationSecurityScopeProcessor("AuthToken"));

                    config.PostProcess =
                    (document) =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "gimana.id API";
                        document.Info.Description = "The backend API for gimana.id, a group project for DTETI FT UGM's enterpreunership course.";
                        document.Info.Contact = new NSwag.OpenApiContact()
                        {
                            Name = "Firdaus Bisma Suryakusuma",
                            Email = "firdausbismasuryakusuma@mail.ugm.ac.id"
                        };
                    };
                });

            #region init db
            AppDbContext dbContext = null;
            if (_env.IsDevelopment())
            {
                var devDatabaseType = Environment.GetEnvironmentVariable("DEV_DATABASE_TYPE");

                switch (devDatabaseType)
                {
                    case "Concrete":
                        dbContext = new AppDbContext();
                        break;
                    case "InMemory":
                        dbContext = new InMemoryAppDbContext();
                        break;
                    default:
                        throw new Exception($"environment variable DEV_DATABASE_TYPE has invalid value({devDatabaseType}).");
                }
            }
            else if (_env.IsProduction())
            {
                dbContext = new AppDbContext();
            }

            if (dbContext.Users.Count() == 0)
            {
                var salt = new SecurePasswordSaltGenerator().GenerateSecureRandomString();
                var hasher = new PasswordHasher();

                dbContext.Users.Add(
                new User()
                {
                    Id = Guid.NewGuid(),
                    Username = Environment.GetEnvironmentVariable("INIT_ADMIN_USERNAME"),
                    Email = new UserEmail()
                    {
                        EmailAddress = Environment.GetEnvironmentVariable("INIT_ADMIN_EMAIL"),
                        IsVerified = true
                    },
                    BanLiftedDate = DateTime.MinValue,
                    Privileges = new List<UserPrivilege>() { new UserPrivilege() { PrivilegeName = "Admin" } },
                    PasswordCredential = new PasswordCredential()
                    {
                        HashedPassword = hasher.HashPassword(Environment.GetEnvironmentVariable("INIT_ADMIN_PASSWORD"), salt),
                        PasswordSalt = salt
                    }
                });

                dbContext.SaveChanges();
            }
            #endregion
            services.AddTransient(
                typeof(AppDbContext), 
                (serviceProvider) => 
                {
                    if (_env.IsDevelopment())
                    {
                        var devDatabaseType = Environment.GetEnvironmentVariable("DEV_DATABASE_TYPE");

                        switch (devDatabaseType)
                        {
                            case "Concrete":
                                return new AppDbContext();
                            case "InMemory":
                                return new InMemoryAppDbContext();
                            default:
                                throw new Exception($"environment variable DEV_DATABASE_TYPE has invalid value({devDatabaseType}).");
                        }
                    }
                    else if (_env.IsProduction())
                    {
                        return new AppDbContext();
                    }
                    else
                    {
                        throw new Exception("unknown environment type.");
                    }                    
                });

            services.AddSingleton(
                typeof(IMapper), 
                new Mapper(new MapperConfig().GetConfiguration()));

            services.AddAuthentication(
                (config) =>
                {
                    config.DefaultScheme = "RandomTokenScheme";
                })
                .AddScheme<RandomTokenAuthenticationSchemeOptions, ValidateRandomTokenAuthenticationHandler>("RandomTokenScheme", (options) => { });
                // Do we need to add `.AddCookie()` here?

            if (_env.IsDevelopment())
            {
                services.AddSingleton(
                    typeof(IEmailSender),
                    new MockEmailSender());
            }
            else if (_env.IsProduction()) 
            {
                services.AddSingleton(
                    typeof(IEmailSender),
                    new SmtpEmailSender(
                        Environment.GetEnvironmentVariable("EMAIL_CREDENTIAL_ADDRESS"),
                        Environment.GetEnvironmentVariable("EMAIL_CREDENTIAL_PASSWORD")));
            }

            services.AddSingleton(
                typeof(IPasswordHasher),
                new PasswordHasher());

            services.AddSingleton(
                typeof(IDateTimeService),
                new DateTimeService());

            services.AddSingleton(
                typeof(ISecurePasswordSaltGenerator),
                new SecurePasswordSaltGenerator());

            services.AddSingleton(
                typeof(IAlphanumericTokenGenerator),
                new AlphanumericTokenGenerator());

            services.AddAuthorization(config =>
            {
                config.AddPolicy(AuthorizationPolicyConstants.EmailVerifiedPolicy, policy => policy.RequireClaim("EmailVerified", "True"));
                config.AddPolicy(AuthorizationPolicyConstants.IsNotBannedPolicy, policy => policy.RequireClaim("IsBanned", "False"));
                config.AddPolicy(AuthorizationPolicyConstants.ModeratorOnlyPolicy, policy => policy.RequireClaim("IsModerator", "True"));
                config.AddPolicy(AuthorizationPolicyConstants.AdminOnlyPolicy, policy => policy.RequireClaim("IsAdmin", "True"));
                config.AddPolicy(AuthorizationPolicyConstants.AuthenticatedUsersOnlyPolicy, policy => policy.RequireClaim("UserId"));
            });

            services.AddSpaStaticFiles(config => config.RootPath = "ClientApp/build");

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc().AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            if (_env.IsDevelopment()) 
            {
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (_env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer("start");
                }
            });
        }
    }
}
