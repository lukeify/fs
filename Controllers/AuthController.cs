using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fs
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.AuthController"/> class.
        /// </summary>
        /// 
        /// <param name="configuration">Configuration.</param>
        public AuthController(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        ///  GET: api/auth/validate
        /// </summary>
        /// 
        /// <returns>A 204 No Content response if the key was valid, or 403 Forbidden if an invalid key was given.</returns>
        /// 
        /// <param name="key">The key provided by the user for access to the application.</param>
        [HttpPost]
        public IActionResult Validate([FromBody] string key)
        {
            if (String.IsNullOrEmpty(this.Configuration["Filesystem:Key"])) {
                return StatusCode(501);
            }

            if (this.Configuration["Filesystem:Key"] == key)
            {
                return NoContent();
            }

            return StatusCode(403);
        }
    }
}
