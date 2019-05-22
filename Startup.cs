using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fs.Authorization;
using Microsoft.AspNetCore.Authorization;
using RethinkDb.Driver.Net;
using Fs.Services;
using Fs.Helpers;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Fs
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            this.Configuration = configuration;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddEnvironmentVariables();

            // Only add secrets if we are in development, it is done automatically in production.
            if (environment.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Add our authorization policy to require routes to have an application
            // key present on all requests.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasAppKey", policy =>
                    policy.Requirements.Add(new HasAppKeyRequirement(this.Configuration["Filesystem:Key"])));
            });

            // Provide our Handler for the above policy.
            services.AddSingleton<IAuthorizationHandler, HasAppKeyHandler>();

            // Construct and inject our RethinkDB connection into the dotnet core IOC container.
            var R = RethinkDb.Driver.RethinkDB.R;
            Connection conn = R.Connection()
                        .Hostname("localhost") // Hostnames and IP addresses work.
                        .Port(28015) // .Port() is optional. Default driver port number.
                        .Timeout(60)
                        .Connect();

            services.AddSingleton<IConnection>(conn);

            // Provide our own file management services as needed.
            services.AddSingleton<FileUploadService>();
            services.AddSingleton<FilenameGenerator>();
            services.AddSingleton<FileAttributesHelper>();
            services.AddSingleton<ThumbnailGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            string staticFileDir = "";
            if (Path.IsPathRooted(this.Configuration["Fs:Dir"]))
            {
                staticFileDir = this.Configuration["Fs:Dir"];
            } 
            else
            {
                staticFileDir = Path.Combine(Directory.GetCurrentDirectory(), this.Configuration["Fs:Dir"]);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(staticFileDir),
                RequestPath = ""
            });
            app.UseMvc();
        }
    }
}
