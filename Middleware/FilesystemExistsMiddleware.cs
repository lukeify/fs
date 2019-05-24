using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fs.Middleware
{
    public class FilesystemExistsMiddleware
    {
        private RequestDelegate Next { get; set; }

        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Middleware.FilesystemExistsMiddleware"/> class.
        /// </summary>
        /// 
        /// <param name="next">When called, continues the request pipeline.</param>
        /// 
        public FilesystemExistsMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.Next = next;
            this.Configuration = configuration;
        }

        /// <summary>
        /// Invokes the middleware asynchronously. We check if the three directories
        /// we need exist, and if they don't, they are created.
        /// </summary>
        /// 
        /// <returns>The async.</returns>
        /// 
        /// <param name="context">Context.</param>
        /// 
        public async Task InvokeAsync(HttpContext context)
        {
            this.CreateFilesystemDir();
            this.CreateDefaultThumbnailDir();
            this.CreateCustomThumbnailDir();

            await this.Next(context);
        }

        /// <summary>
        /// Creates the root filesystem directory.
        /// </summary>
        private void CreateFilesystemDir()
        {
            Directory.CreateDirectory(this.Configuration["Fs:Dir"]);
        }

        /// <summary>
        /// Creates the default thumbnails directory.
        /// </summary>
        private void CreateDefaultThumbnailDir()
        {
            Directory.CreateDirectory(this.Configuration["Fs:DefaultThumbDir"]);
        }

        /// <summary>
        /// Creates the custom thumbnails directory.
        /// </summary>
        private void CreateCustomThumbnailDir()
        {
            Directory.CreateDirectory(this.Configuration["Fs:CustomThumbDir"]);
        }
    }

    public static class FilesystemExistsMiddlewareExtensions
    {
        public static IApplicationBuilder EnsureFilesystemExists(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FilesystemExistsMiddleware>();
        }
    }
}
