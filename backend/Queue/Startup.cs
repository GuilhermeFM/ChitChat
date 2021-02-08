using Authentication.Queue.Models;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;

namespace Queue
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

            services.AddHangfireServer();

            #endregion

            #region SETUP - EMAIL

            var username = Configuration["EmailSettings:Username"];
            var password = Configuration["EmailSettings:Password"];
            var server = Configuration["EmailSettings:PrimaryDomain"];
            var port = Configuration.GetValue<int>("EmailSettings:PrimaryPort");

            services
                .AddFluentEmail("chitchat@example.com", "ChitChat - Gossip for everyone.")
                .AddMailtrapSender(username, password, server, port);

            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AuthorizationFilter() },
                IsReadOnlyFunc = (DashboardContext context) => true
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard();
            });
        }
    }
}
