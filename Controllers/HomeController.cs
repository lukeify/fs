using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Fs.Controllers
{
    [Route("/")]
    [ApiController]
    public class HomeController : Controller
    {
        /// <summary>
        /// GET: /
        /// Fetches the homepage of the application. Any non-matching routes will
        /// return this.
        /// </summary>
        /// <returns>The index.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View("index.html");
        }
    }
}
