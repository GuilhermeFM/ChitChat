using Api.Hubs;
using Authentication.Contexts;
using Authentication.Models;
using Authentication.Models.Settings;
using Authentication.Services;
using Core.Contexts;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Text;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment WebHostEnvironment { get; }

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            WebHostEnvironment = webHostEnvironment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            #region SETUP - CORS

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            #endregion

            #region SETUP - EF Context

            services.AddDbContext<AuthenticationDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AuthenticationConnStr")));
            services.AddDbContext<CoreDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ChitChatConnStr")));

            #endregion

            #region SETUP - Identity

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AuthenticationDBContext>()
            .AddDefaultTokenProviders();

            #endregion

            #region SETUP - JWT Auth

            var JWTValidAudience = Configuration["JWT:ValidAudience"];
            var JWTValidIssuer = Configuration["JWT:ValidIssuer"];

            var JWTSecrete = Configuration["JWT:Secret"];
            var JWTSecreteBytes = Encoding.UTF8.GetBytes(JWTSecrete);
            var JWTSymetricSecurityKey = new SymmetricSecurityKey(JWTSecreteBytes);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = JWTValidIssuer,
                    ValidAudience = JWTValidAudience,
                    IssuerSigningKey = JWTSymetricSecurityKey
                };
            });

            #endregion

            #region SETUP - HANGFIRE

            var hangfireConnectionString = Configuration.GetConnectionString("HangfireConnStr");
            var sqlServerStorageOptions = new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
            };

            services.AddHangfire(configuration =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(hangfireConnectionString, sqlServerStorageOptions);
            });

            #endregion

            services.AddSignalR();
            services.AddTransient<AuthenticateService>();
            services.Configure<AuthenticateServiceSettings>(Configuration.GetSection("JWT"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/messages");
            });
        }
    }
}
