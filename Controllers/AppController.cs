using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fs.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AppController : Controller
    {
        public IConfiguration Configuration { get; set; }

        public AppController(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        [HttpGet]
        public AppData Meta()
        {
            return new AppData()
            {
                Name = this.Configuration["App:Name"],
                Statement = this.Configuration["App:Statement"],
                PreferredThumbnailHeight = int.Parse(this.Configuration["App:PreferredThumbnailHeight"])
            };
        }
    }
}
