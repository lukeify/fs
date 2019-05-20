using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Fs.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// GET: /
        /// Fetches the homepage of the application. Any non-matching routes will
        /// return this.
        /// </summary>
        /// <returns>The index.</returns>
        public IActionResult Index()
        {
            return View("index.html");
        }
    }
}
